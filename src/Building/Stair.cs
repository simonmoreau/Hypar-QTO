using System;
using System.Linq;
using System.Collections.Generic;
using Elements;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Bim42HyparQto.BuildingCode;


namespace Bim42HyparQto
{
    /// <summary>
    /// The stairs of the buildings
    /// </summary>
    public class Stair
    {
        Model _model;
        double _levelHeight;
        double _wallThickness;
        int _upNumber;

        double _tread_depth;
        double _riser_height;
        double _run_structural_depth;
        int _riser_number;
        Transform _currentTransform;

        private Vector3 verticalThickness;


        public Stair(Model model, double levelHeight, double wallThickness, int UPNumber)
        {
            _model = model;
            _levelHeight = levelHeight;
            _wallThickness = wallThickness;
            _upNumber = UPNumber;
            _riser_number = (int)Math.Ceiling(levelHeight / 0.16);
            _riser_height = levelHeight / _riser_number;
            _tread_depth = 0.28;
            _run_structural_depth = 0.15;
            _currentTransform = new Transform();
        }

        public void PlaceStair(Vector3 basePoint, Vector3 direction)
        {
            _currentTransform = new Transform(basePoint, direction.Normalized(), Vector3.ZAxis);
            CreateStair();
        }

        private void CreateStair()
        {
            double stairRunWidth = CodeDuTravail.ConvertUpToMeter(_upNumber);

            int firstRunRiserNumber = (int)Math.Floor((double)_riser_number / 2);
            int secondRunRiserNumber = _riser_number - firstRunRiserNumber;

            Vector3 coreWallLenght = Vector3.XAxis * (_tread_depth * Math.Max(firstRunRiserNumber, secondRunRiserNumber));
            Vector3 landingElevation = Vector3.ZAxis * (_riser_height * firstRunRiserNumber);
            Vector3 lenght = Vector3.XAxis * (stairRunWidth * 2 + _wallThickness * 2) + coreWallLenght;
            Vector3 width = Vector3.YAxis * (stairRunWidth * 2 + _wallThickness * 3);

            WallType stairWallType = new WallType("stairWallType", _wallThickness);

            Vector3 firstRunBasePoint = (Vector3.XAxis + Vector3.YAxis) * (stairRunWidth + _wallThickness);
            Vector3 firstRunDirection = Vector3.XAxis;
            CreateStairRun(firstRunRiserNumber, firstRunBasePoint, firstRunDirection);

            Vector3 secondRunBasePoint = firstRunBasePoint + coreWallLenght + _wallThickness * Vector3.YAxis + landingElevation - _riser_height * Vector3.ZAxis;
            Vector3 secondRunDirection = firstRunDirection.Negated();
            CreateStairRun(secondRunRiserNumber + 1, secondRunBasePoint, secondRunDirection);

            FloorType landingType = new FloorType("LandingFloorType", verticalThickness.Length());
            Vector3 landingOrigin = new Vector3(
                stairRunWidth + _wallThickness + coreWallLenght.Length(),
                _wallThickness,
                landingElevation.Length()
                );
            Polygon firstLandingPolygon = new Polygon(new Vector3[4] {
                landingOrigin,
                landingOrigin + stairRunWidth*Vector3.XAxis,
                landingOrigin + stairRunWidth*Vector3.XAxis +(stairRunWidth * 2 + _wallThickness) * Vector3.YAxis,
                landingOrigin +(stairRunWidth * 2 + _wallThickness) * Vector3.YAxis,
            });

            firstLandingPolygon = _currentTransform.OfPolygon(firstLandingPolygon);
            Floor firstLanding = new Floor(firstLandingPolygon, landingType);
            _model.AddElement(firstLanding);


            Polygon stairCasePolygon = new Polygon(new Vector3[4] {
                Vector3.Origin,
                Vector3.Origin + lenght,
                Vector3.Origin + lenght + width,
                Vector3.Origin + width
            });

            Polygon wallPolygon = stairCasePolygon.Offset(-_wallThickness / 2)[0];
            wallPolygon = _currentTransform.OfPolygon(wallPolygon);

            foreach (Line line in Helper.GetPolygonLines(wallPolygon))
            {
                Wall stairCaseWall = new Wall(line, stairWallType, _levelHeight);
                _model.AddElement(stairCaseWall);
            }

            Vector3 pt1 = width / 2 + Vector3.XAxis * (stairRunWidth + _wallThickness);
            Vector3 pt2 = pt1 + coreWallLenght;
            Line coreWallLine = new Line(pt1, pt2);

            coreWallLine = _currentTransform.OfLine(coreWallLine);
            Wall coreWall = new Wall(coreWallLine, stairWallType, _levelHeight);
            _model.AddElement(coreWall);




        }

        public void CreateStairRun(int riser_number, Vector3 basePoint, Vector3 direction)
        {
            List<Vector3> runPoints = new List<Vector3>();

            Vector3 tread = new Vector3(_tread_depth, 0, 0);
            Vector3 riser = new Vector3(0, _riser_height, 0);

            for (int i = 0; i < riser_number; i++)
            {
                runPoints.Add(new Vector3(i * _tread_depth, i * _riser_height, 0));
                runPoints.Add(new Vector3(i * _tread_depth, (i + 1) * _riser_height, 0));
            }

            // Last step
            Vector3 lastStepPoint = new Vector3(riser_number * _tread_depth, riser_number * _riser_height, 0);
            runPoints.Add(lastStepPoint);

            // Run thickness
            Vector3 runThickness = (riser + tread).Cross(Vector3.ZAxis).Normalized() * _run_structural_depth;
            double alpha = runThickness.AngleTo(Vector3.YAxis.Negated());
            alpha = alpha * Math.PI / 180;
            verticalThickness = (runThickness.Length() / Math.Cos(alpha)) * Vector3.YAxis.Negated();
            runPoints.Add(lastStepPoint + verticalThickness);

            alpha = (riser + tread).Negated().AngleTo(Vector3.XAxis);
            alpha = alpha * Math.PI / 180;
            Vector3 horizontalThickness = (runThickness.Length() / Math.Sin(alpha)) * Vector3.XAxis;
            runPoints.Add(horizontalThickness);


            Polygon polygon = new Polygon(runPoints.ToArray());
            polygon = polygon.Reversed();

            double stairRunWidth = CodeDuTravail.ConvertUpToMeter(_upNumber);
            Vector3 runWidthDirection = Vector3.ZAxis;

            Vector3 x = Vector3.XAxis;
            Vector3 z = Vector3.YAxis.Negated();
            Vector3 y = z.Cross(x);
            Matrix transformMatrix = new Matrix(x, y, z, new Vector3());

            Transform transform = new Transform(transformMatrix);
            transform.Concatenate(new Transform(basePoint, direction.Normalized(), Vector3.ZAxis));
            transform.Concatenate(_currentTransform);
            polygon = transform.OfPolygon(polygon);
            runWidthDirection = transform.OfVector(runWidthDirection);

            Solid solid = Solid.SweepFace(polygon, new Polygon[] { }, runWidthDirection, stairRunWidth);


            Wall test = new Wall(new Solid[] { solid }, new WallType("test", 0.1));
            _model.AddElement(test);

        }

    }
}