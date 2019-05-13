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
        private Vector3 beamOrigin;
        public Output Execute(Input input)
        {
            // Create a model
            Model model = new Model();

            //Get the input values
            dim.Width = input.Width;
            dim.ModuleLenght = input.ModuleWidth;

            double area = 5;

            // Create the buildings spaces
            Interior interior = new Interior(model, dim);
            interior.Headroom = input.Headroom;

            // Create the building structure
            Structure structure = new Structure(model,dim);

            // Create building façades
            Facade facade = new Facade(model, dim, interior, structure);

            Line northFacadeLine = new Line(new Vector3(0, dim.Width, 0), new Vector3(dim.Lenght, dim.Width, 0));
            Line southFacadeLine = new Line(new Vector3(0, 0, 0), new Vector3(dim.Lenght, 0, 0));


            // Create grid divisions
            double[] uDistances = Enumerable.Repeat(dim.ModuleLenght, dim.ModuleNumber).ToArray();
            double[] vDistances = new double[] {
                dim.OfficeSpaceWidth - dim.CorridorWidth,
                dim.CorridorWidth,
                dim.CoreWidth,
                dim.CorridorWidth,
                dim.OfficeSpaceWidth - dim.CorridorWidth
            };

            // Create the main grid of the building
            GridEx buildingGrid = new GridEx(southFacadeLine, northFacadeLine, uDistances, vDistances);
            // ColoredSpaces(model, buildingGrid.TopCells,Colors.Green);
            // ColoredSpaces(model, buildingGrid.BottomCells,Colors.Yellow);
            // ColoredSpaces(model, buildingGrid.LeftCells,Colors.Red);
            // ColoredSpaces(model, buildingGrid.RightCells,Colors.Blue);

            structure.CreateStructure(buildingGrid, facade.FacadeThickness);
            facade.CreateFaçades(buildingGrid);
            interior.CreateInterior(buildingGrid, facade.FacadeThickness);

            // Create a stair
            Stair stair = new Stair(model, dim.LevelHeight,0.2,3);
            Vector3 direction  = Vector3.XAxis;
            stair.PlaceStair(buildingGrid.Cells[5,2].Points[0],direction);
            

            Column column = new Column(new Vector3(0,0,0),10, structure.ColumnType);
            model.AddElement(column);
            // CreateFloors(model, buildingGrid);

            // double levelElevation = 0;

            // for (int i = 0; i < 4; i++)
            // {
            //     if (i == 0)
            //     {
            //         levelElevation = 5;
            //         area = area + CreateGroundLevel(model);
            //     }
            //     else
            //     {
            //         //Create a level
            //         area = area + CreateLevel(model, levelElevation);
            //         levelElevation = levelElevation + dim.LevelDimensions.Height;
            //     }
            // }

            return new Output(model, area);
        }

        public void ColoredSpaces(Model model, List<Cell> cells, Color color)
        {
            foreach (Cell cell in cells)
            {
                Polygon cellPolygon = new Polygon(cell.Points);
                cellPolygon = cellPolygon.Offset(-0.1)[0];
                Material mat = new Material("color" + color.GetHashCode().ToString(),color);

                Space space = new Space(new Profile(cellPolygon),0.2,0,mat);
                model.AddElement(space);
            }
        }

        // public void CreateFloors(Model model, GridEx buildingGrid)
        // {
        //     Cell[,] cells = buildingGrid.Cells;
        //     int maxRow = cells.GetLength(0) - 1;
        //     int maxColumn = cells.GetLength(1) - 1;

        //     Vector3[] floorCell = new Vector3[] {
        //         cells[0,0].Points[0],
        //         cells[maxRow,0].Points[1],
        //         cells[maxRow,maxColumn].Points[2],
        //         cells[0,maxColumn].Points[3],
        //     };

        //     CreateFloors(model, floorCell, dim.LevelDimensions.Height);
        // }



        // public double CreateGroundLevel(Model model)
        // {
        //     double area = 0;

        //     //Create slab

        //     //Create Façade
        //     Line northFacadeLine = new Line(new Vector3(0, dim.Width, 0), new Vector3(dim.Lenght, dim.Width, 0));
        //     Line northInnerLine = new Line(new Vector3(0, dim.Width / 2, 0), new Vector3(dim.Lenght, dim.Width / 2, 0));

        //     Line southFacadeLine = new Line(new Vector3(0, 0, 0), new Vector3(dim.Lenght, 0, 0));
        //     Line southInnerLine = new Line(new Vector3(0, dim.Width / 2, 0), new Vector3(dim.Lenght, dim.Width / 2, 0));



        //     return area;
        // }

        // public double CreateLevel(Model model, double levelElevation)
        // {
        //     double area = 0;

        //     Line northFacadeLine = new Line(new Vector3(0, dim.Width, levelElevation), new Vector3(dim.Lenght, dim.Width, levelElevation));
        //     Line northInnerLine = new Line(new Vector3(0, dim.OfficeSpaceWidth + dim.CoreWidth, levelElevation), new Vector3(dim.Lenght, dim.OfficeSpaceWidth + dim.CoreWidth, levelElevation));

        //     Line southFacadeLine = new Line(new Vector3(0, 0, levelElevation), new Vector3(dim.Lenght, 0, levelElevation));
        //     Line southInnerLine = new Line(new Vector3(0, dim.OfficeSpaceWidth, levelElevation), new Vector3(dim.Lenght, dim.OfficeSpaceWidth, levelElevation));

        //     CreateFaçade(model, northFacadeLine, northInnerLine);
        //     CreateFaçade(model, southFacadeLine, southInnerLine);

        //     area = area + CreateOfficeSpace(model, northFacadeLine, northInnerLine, area);
        //     area = area + CreateOfficeSpace(model, southFacadeLine, southInnerLine, area);

        //     CreateCore(model, northInnerLine, southInnerLine);

        //     return area;
        // }

        // public void CreateCore(Model model, Line northInnerLine, Line southInnerLine)
        // {

        //     Grid coreGrid = new Grid(northInnerLine, southInnerLine, 1, 1);

        //     Vector3[] meetingCell = new Vector3[] {
        //         northInnerLine.PointAt(0.1),
        //         northInnerLine.PointAt(0.4),
        //         southInnerLine.PointAt(0.4),
        //         southInnerLine.PointAt(0.1)
        //     };

        //     CreateMeetingRoom(model, meetingCell);

        //     CreateFloors(model, coreGrid.Cells()[0, 0], dim.LevelDimensions.Height);
        // }

        // public void CreateMeetingRoom(Model model, Vector3[] cell)
        // {
        //     Vector3 elevation = new Vector3(0, 0, dim.LevelDimensions.RaisedFloorThickness + dim.LevelDimensions.RaisedFloorVoidHeight);

        //     for (int i = 0; i < cell.Count(); i++)
        //     {
        //         Vector3 startPoint = cell[i] + elevation;
        //         Vector3 endPoint = cell[i] + elevation;
        //         if (i == 0) { startPoint = cell[cell.Count() - 1] + elevation; } else { startPoint = cell[i - 1] + elevation; }

        //         Line wallLine = new Line(startPoint, endPoint);

        //         Wall meetingWall = new Wall(wallLine, dim.Types.MeetingRoomWallType, dim.LevelDimensions.Headroom);
        //         model.AddElement(meetingWall);

        //     }
        // }

        // public double CreateOfficeSpace(Model model, Line façadeLine, Line innerLine, double area)
        // {
        //     Grid spaceGrid = new Grid(façadeLine, innerLine, 1, 1);
        //     Vector3[] spaceCell = spaceGrid.Cells()[0, 0];
        //     //Helper vector
        //     Vector3 towardInside = (spaceCell[1] - spaceCell[0]).Normalized();
        //     Vector3 facadeOffset = towardInside * (1);

        //     //Create floor
        //     Vector3[] mainCell = new Vector3[] { spaceCell[0] + facadeOffset, spaceCell[1], spaceCell[2], spaceCell[3] + facadeOffset };
        //     CreateFloors(model, mainCell, dim.LevelDimensions.Height);


        //     Vector3 corridorOffset = towardInside * (-1.5);
        //     Vector3[] officeCell = new Vector3[] { spaceCell[0] + facadeOffset, spaceCell[1] + corridorOffset, spaceCell[2] + corridorOffset, spaceCell[3] + facadeOffset };
        //     Vector3[] corridorCell = new Vector3[] { spaceCell[1] + corridorOffset, spaceCell[1], spaceCell[2], spaceCell[2] + corridorOffset };

        //     //Create a space
        //     Polygon officePolygon = new Polygon(officeCell);
        //     Polygon corridorPolygon = new Polygon(corridorCell);

        //     Material MintMaterial = new Material("Mint", GeometryEx.Palette.Mint);
        //     Material GreenMaterila = new Material("Green", GeometryEx.Palette.Green);
        //     Space officeSpace = new Space(new Profile(officePolygon), dim.LevelDimensions.Headroom, dim.LevelDimensions.RaisedFloorThickness + dim.LevelDimensions.RaisedFloorVoidHeight, MintMaterial, null);
        //     //model.AddElement(officeSpace);
        //     area = area + officePolygon.Area();

        //     Space corridorSpace = new Space(new Profile(corridorPolygon), dim.LevelDimensions.Headroom, dim.LevelDimensions.RaisedFloorThickness + dim.LevelDimensions.RaisedFloorVoidHeight, MintMaterial, null);
        //     model.AddElement(corridorSpace);
        //     area = area + corridorPolygon.Area();

        //     return area;
        // }

        // public void CreateModule(Model model, Vector3[] cell, bool structural, double levelHeight)
        // {

        //     //Helper vector
        //     Vector3 towardInside = (cell[1] - cell[0]).Normalized();

        //     //Create a façade
        //     Vector3[] facadeCell = new Vector3[] {
        //         cell[0],
        //         cell[0] + new Vector3(0,0,levelHeight),
        //         cell[3] + new Vector3(0,0,levelHeight),
        //         cell[3]
        //     };





        // }

        // public void CreateFloors(Model model, Vector3[] cell, double levelHeight)
        // {

        //     //Create a slab
        //     Polygon polygon = new Polygon(cell);
        //     Plane polygonPlane = polygon.Plane();
        //     Vector3 normal = polygonPlane.Normal;
        //     if (normal.Z < 0) { polygon = polygon.Reversed(); }
        //     Floor bottomFloor = new Floor(polygon, dim.Types.SlabType, levelHeight);
        //     model.AddElement(bottomFloor);

        //     //Create a raised floor
        //     Floor raisedFloor = new Floor(polygon, dim.Types.RaisedFloorType, dim.LevelDimensions.RaisedFloorThickness + dim.LevelDimensions.RaisedFloorVoidHeight);
        //     model.AddElement(raisedFloor);

        //     //Create a ceilling
        //     double ceilingElevation = levelHeight - dim.LevelDimensions.StructuralDimensions.BeamHeight - dim.LevelDimensions.CeilingVoidHeight;
        //     Floor ceilling = new Floor(polygon, dim.Types.CeillingType, ceilingElevation);
        //     model.AddElement(ceilling);
        // }


    }
}