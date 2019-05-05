using System;
using System.Linq;
using System.Collections.Generic;
using Elements;
using Elements.Geometry;
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

        public Stair(Model model, double levelHeight, double wallThickness, int UPNumber)
        {
            _model = model;
            _levelHeight = levelHeight;
            _wallThickness = wallThickness;
            _upNumber = UPNumber;
        }

        public void CreateStair(Vector3 basePoint, Vector3 direction)
        {
            double stairFlightWidth = CodeDuTravail.ConvertUpToMeter(_upNumber);
            Vector3 lenght = direction.Normalized() * 8;
            Vector3 width = Vector3.ZAxis.Cross(direction).Normalized() * (stairFlightWidth * 2 + _wallThickness *3);
            WallType stairWallType = new WallType("stairWallType", 0.2);

            Polygon stairCasePolygon = new Polygon(new Vector3[4] {
                basePoint,
                basePoint + lenght,
                basePoint + lenght + width,
                basePoint + width
            });

            Polygon wallPolygon = stairCasePolygon.Offset(-_wallThickness / 2)[0];

            foreach (Line line in Helper.GetPolygonLines(wallPolygon))
            {
                Wall stairCaseWall = new Wall(line, stairWallType, _levelHeight);
                _model.AddElement(stairCaseWall);
            }

            Vector3 pt1 = basePoint + width/2 + direction*(stairFlightWidth + _wallThickness);
            Vector3 pt2 = basePoint + lenght + width/2 - direction*(stairFlightWidth + _wallThickness);
            Line coreWallLine = new Line(pt1, pt2);
            Wall coreWall = new Wall(coreWallLine,stairWallType,_levelHeight);
            _model.AddElement(coreWall);

        }

    }
}