using Elements;
using Elements.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Bim42HyparQto
{
    /// <summary>
    /// A generator for building a simplified model for Quantity TakeOff.
    /// </summary>
  	public class Bim42HyparQto
    {
        private BuildingDimensions dim = BuildingDimensions.Instance;
        public Output Execute(Input input)
        {
            // Create a model
            Model model = new Model();
            double area = 0;

            for (int i = 0; i < 5; i++)
            {
                //Create a level
                area = area + CreateLevel(model,i*dim.LevelDimensions.Height);
            }

            return new Output(model, area);
        }

        public double CreateLevel(Model model, double levelElevation)
        {
            double area = 0;

            Line northFacadeLine = new Line(new Vector3(0, dim.Width, levelElevation), new Vector3(dim.Lenght, dim.Width, levelElevation));
            Line northInnerLine = new Line(new Vector3(0, dim.OfficeSpaceWidth + dim.CoreWidth, levelElevation), new Vector3(dim.Lenght, dim.OfficeSpaceWidth + dim.CoreWidth, levelElevation));

            Line southFacadeLine = new Line(new Vector3(0, 0, levelElevation), new Vector3(dim.Lenght, 0, levelElevation));
            Line southInnerLine = new Line(new Vector3(0, dim.OfficeSpaceWidth, levelElevation), new Vector3(dim.Lenght, dim.OfficeSpaceWidth, levelElevation));

            CreateFaçade(model, northFacadeLine, northInnerLine);
            CreateFaçade(model, southFacadeLine, southInnerLine);

            area = area + CreateOfficeSpace(model, northFacadeLine, northInnerLine, area);
            area = area + CreateOfficeSpace(model, southFacadeLine, southInnerLine, area);

            CreateCore(model, northInnerLine, southInnerLine);

            return area;
        }

        public void CreateCore(Model model, Line northInnerLine, Line southInnerLine)
        {

            Grid coreGrid = new Grid(northInnerLine, southInnerLine, 1, 1);

            Vector3[] meetingCell = new Vector3[] {
                northInnerLine.PointAt(0.1),
                northInnerLine.PointAt(0.4),
                southInnerLine.PointAt(0.4),
                southInnerLine.PointAt(0.1)
            };

            CreateMeetingRoom(model, meetingCell);

            CreateFloors(model, coreGrid.Cells()[0, 0]);
        }

        public void CreateMeetingRoom(Model model, Vector3[] cell)
        {
            Vector3 elevation = new Vector3(0, 0, dim.LevelDimensions.RaisedFloorThickness + dim.LevelDimensions.RaisedFloorVoidHeight);

            for (int i = 0; i < cell.Count(); i++)
            {
                Vector3 startPoint = cell[i] + elevation;
                Vector3 endPoint = cell[i] + elevation;
                if (i == 0) { startPoint = cell[cell.Count() - 1] + elevation; } else { startPoint = cell[i - 1] + elevation; }

                Line wallLine = new Line(startPoint, endPoint);

                Wall meetingWall = new Wall(wallLine, dim.Types.MeetingRoomWallType, dim.LevelDimensions.Headspace);
                model.AddElement(meetingWall);

            }
        }

        public double CreateOfficeSpace(Model model, Line façadeLine, Line innerLine, double area)
        {
            Grid spaceGrid = new Grid(façadeLine, innerLine, 1, 1);
            Vector3[] spaceCell = spaceGrid.Cells()[0, 0];
            //Helper vector
            Vector3 towardInside = (spaceCell[1] - spaceCell[0]).Normalized();
            Vector3 facadeOffset = towardInside * (dim.FacadeDimensions.FacadeThickness);

            //Create floor
            Vector3[] mainCell =  new Vector3[] { spaceCell[0] + facadeOffset, spaceCell[1], spaceCell[2], spaceCell[3] + facadeOffset };
            CreateFloors(model, mainCell);

            
            Vector3 corridorOffset = towardInside * (-1.5);
            Vector3[] officeCell = new Vector3[] { spaceCell[0] + facadeOffset, spaceCell[1] + corridorOffset, spaceCell[2] + corridorOffset, spaceCell[3] + facadeOffset };
            Vector3[] corridorCell = new Vector3[] { spaceCell[1] + corridorOffset, spaceCell[1], spaceCell[2], spaceCell[2] + corridorOffset };

            //Create a space
            Polygon officePolygon = new Polygon(officeCell);
            Polygon corridorPolygon = new Polygon(corridorCell);

            Material MintMaterial = new Material("Mint", GeometryEx.Palette.Mint);
            Material GreenMaterila = new Material("Green", GeometryEx.Palette.Green);
            Space officeSpace = new Space(new Profile(officePolygon), dim.LevelDimensions.Headspace, dim.LevelDimensions.RaisedFloorThickness + dim.LevelDimensions.RaisedFloorVoidHeight, MintMaterial, null);
            //model.AddElement(officeSpace);
            area = area + officePolygon.Area();

            Space corridorSpace = new Space(new Profile(corridorPolygon), dim.LevelDimensions.Headspace, dim.LevelDimensions.RaisedFloorThickness + dim.LevelDimensions.RaisedFloorVoidHeight, MintMaterial, null);
            model.AddElement(corridorSpace);
            area = area + corridorPolygon.Area();

            return area;
        }

        public void CreateFaçade(Model model, Line façadeLine, Line innerLine)
        {
            Grid moduleGrid = new Grid(façadeLine, innerLine, dim.ModuleNumber, 1);

            for (int i = 0; i < moduleGrid.Cells().GetLength(0); i++)
            {
                Vector3[] cell = moduleGrid.Cells()[i, 0];

                //Create a beam each 3 module
                Math.DivRem(i, 3, out int remainer);
                if (remainer == 0)
                {
                    CreateModule(model, cell, true);
                }
                else
                {
                    CreateModule(model, cell, false);
                }
            }
        }

        public void CreateModule(Model model, Vector3[] cell, bool structural)
        {

            //Helper vector
            Vector3 towardInside = (cell[1] - cell[0]).Normalized();

            //Create a façade
            Vector3[] facadeCell = new Vector3[] {
                cell[0],
                cell[0] + new Vector3(0,0,dim.LevelDimensions.Height),
                cell[3] + new Vector3(0,0,dim.LevelDimensions.Height),
                cell[3]
            };
            CreateFacadeModule(model, facadeCell, towardInside);

            Vector3 innerOffset = towardInside * (dim.FacadeDimensions.FacadeThickness);
            Vector3[] innerCell = new Vector3[] { cell[0] + innerOffset, cell[1], cell[2], cell[3] + innerOffset };

            if (structural)
            {

                double column_height = dim.LevelDimensions.Height - dim.LevelDimensions.StructuralDimensions.BeamHeight;
                Vector3 columnOffset = towardInside * 0.5;
                Column circularColumn = new Column(innerCell[0] + columnOffset, column_height, dim.Types.ColumnType, null, 0, 0);
                model.AddElement(circularColumn);

                Vector3 beamElevation = new Vector3(0, 0, dim.LevelDimensions.Height - dim.LevelDimensions.StructuralDimensions.SlabHeight - (dim.LevelDimensions.StructuralDimensions.BeamHeight - dim.LevelDimensions.StructuralDimensions.SlabHeight) / 2);
                Line beamLine = new Line(innerCell[0] + beamElevation, innerCell[1] + beamElevation);


                Beam beam = new Beam(beamLine, dim.Types.BeamType);
                model.AddElement(beam);
            }
        }

        public void CreateFloors(Model model, Vector3[] cell)
        {

            //Create a slab
            Polygon polygon = new Polygon(cell);
            Plane polygonPlane = polygon.Plane();
            Vector3 normal = polygonPlane.Normal;
            if (normal.Z < 0) {polygon = polygon.Reversed();}
            Floor bottomFloor = new Floor(polygon, dim.Types.SlabType, dim.LevelDimensions.Height);
            model.AddElement(bottomFloor);

            //Create a raised floor
            Floor raisedFloor = new Floor(polygon, dim.Types.RaisedFloorType, dim.LevelDimensions.RaisedFloorThickness + dim.LevelDimensions.RaisedFloorVoidHeight);
            model.AddElement(raisedFloor);

            //Create a ceilling
            double ceilingElevation = dim.LevelDimensions.RaisedFloorVoidHeight + dim.LevelDimensions.RaisedFloorThickness + dim.LevelDimensions.Headspace + dim.LevelDimensions.CeilingThickness;
            Floor ceilling = new Floor(polygon, dim.Types.CeillingType, ceilingElevation);
            model.AddElement(ceilling);
        }

        public void CreateFacadeModule(Model model, Vector3[] cell, Vector3 towardInside)
        {
            Vector3 innerVector = towardInside * dim.FacadeDimensions.FacadeThickness;
            Polygon cellPolygon = new Polygon(cell);
            if (cellPolygon.Plane().Normal.Normalized().Equals(innerVector.Normalized()))
            {
                cell = cellPolygon.Reversed().Vertices;
            }

            Vector3[] innerCell = cell.Select(v => v + innerVector).ToArray();

            double[,] panelsDimensions = new double[4, 2] {
{0.15,0.35},
{0.15,0.25},
{0.05,0.05},
{0.15,0.65}
};

            Random rnd = new Random();
            int panelNumber = rnd.Next(0, 3);

            Transform panelTransform = new Transform(innerCell[0], innerCell[3] - innerCell[0], innerVector.Negated());
            double leftThickness = panelsDimensions[panelNumber, 0];
            double topThickness = dim.LevelDimensions.CeilingThickness + dim.LevelDimensions.CeilingVoidHeight + dim.LevelDimensions.StructuralDimensions.BeamHeight;
            double rightThickness = panelsDimensions[panelNumber, 1];
            double bottomThickness = dim.LevelDimensions.RaisedFloorVoidHeight + dim.LevelDimensions.RaisedFloorThickness;
            Vector3[] glassCell = new Vector3[4];
            glassCell[0] = innerCell[0] + panelTransform.OfVector(new Vector3(leftThickness, (-1) * bottomThickness, 0));
            glassCell[1] = innerCell[1] + panelTransform.OfVector(new Vector3(leftThickness, topThickness, 0));
            glassCell[2] = innerCell[2] + panelTransform.OfVector(new Vector3((-1) * rightThickness, topThickness, 0));
            glassCell[3] = innerCell[3] + panelTransform.OfVector(new Vector3((-1) * rightThickness, (-1) * bottomThickness, 0));

            Polygon glassPolygon = new Polygon(glassCell);

            Material facadePanelMaterial = new Material("facadeMaterial", Colors.White,(float)0.1,(float)0.1,true);

            Panel glassPanel = new Panel(glassCell, BuiltInMaterials.Glass);
            model.AddElement(glassPanel);

            Vector3 mullionStart = glassCell[0] + new Vector3(0, 0, 1);
            Vector3 mullionEnd = glassCell[3] + new Vector3(0, 0, 1);
            Beam mullion = new Beam(new Line(mullionStart, mullionEnd), dim.Types.MullionType);
            model.AddElement(mullion);


            for (int i = 0; i < 4; i++)
            {
                int j = i + 1;
                if (i == 3) { j = 0; }
                Panel panel = new Panel(new Vector3[] {
                innerCell[i],
                cell[i],
                cell[j],
                innerCell[j]
            }, facadePanelMaterial);
                model.AddElement(panel);

                Panel sidePanel = new Panel(new Vector3[] {
                glassCell[i],
                cell[i],
                cell[j],
                glassCell[j]
            }, facadePanelMaterial);
                model.AddElement(sidePanel);

                Panel innerPannel = new Panel(new Vector3[] {
                glassCell[i],
                innerCell[i],
                innerCell[j],
                glassCell[j]
            }, facadePanelMaterial);
                model.AddElement(innerPannel);
            }

        }
    }
}