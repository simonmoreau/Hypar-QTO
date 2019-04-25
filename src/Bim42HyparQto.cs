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


            Line northFacadeLine = new Line(new Vector3(0, dim.Width, 0), new Vector3(dim.Lenght, dim.Width, 0));
            Line northInnerLine = new Line(new Vector3(0, dim.OfficeSpaceWidth + dim.CoreWidth, 0), new Vector3(dim.Lenght, dim.OfficeSpaceWidth + dim.CoreWidth, 0));

            Line southFacadeLine = new Line(new Vector3(0, 0, 0), new Vector3(dim.Lenght, 0, 0));
            Line southInnerLine = new Line(new Vector3(0, dim.OfficeSpaceWidth, 0), new Vector3(dim.Lenght, dim.OfficeSpaceWidth, 0));

            CreateFaçade(model, northFacadeLine, northInnerLine);
            CreateFaçade(model, southFacadeLine, southInnerLine);

            area = area + CreateOfficeSpace(model, northFacadeLine, northInnerLine, area);
            area = area + CreateOfficeSpace(model, southFacadeLine, southInnerLine, area);

            CreateCore(model, northInnerLine, southInnerLine);


            return new Output(model, area);
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
            Vector3 corridorOffset = towardInside * (-1.5);
            Vector3[] officeCell = new Vector3[] { spaceCell[0] + facadeOffset, spaceCell[1] + corridorOffset, spaceCell[2] + corridorOffset, spaceCell[3] + facadeOffset };
            Vector3[] corridorCell = new Vector3[] { spaceCell[1] + corridorOffset, spaceCell[1], spaceCell[2], spaceCell[2] + corridorOffset };

            //Create a space
            Polygon officePolygon = new Polygon(officeCell);
            Polygon corridorPolygon = new Polygon(corridorCell);

            Material MintMaterial = new Material("Mint", GeometryEx.Palette.Mint);
            Material GreenMaterila = new Material("Green", GeometryEx.Palette.Green);
            Space officeSpace = new Space(new Profile(officePolygon), dim.LevelDimensions.Headspace, dim.LevelDimensions.RaisedFloorThickness + dim.LevelDimensions.RaisedFloorVoidHeight, MintMaterial, null);
            model.AddElement(officeSpace);
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
            Vector3 facadeOffset = towardInside * (dim.FacadeDimensions.FacadeThickness / 2);
            Line facadeBaseLine = new Line(cell[0] + facadeOffset, cell[3] + facadeOffset);
            Wall facadeWall = new Wall(facadeBaseLine, dim.Types.FacadeType, dim.LevelDimensions.Height, null, null);
            model.AddElement(facadeWall);

            Vector3 innerOffset = towardInside * (dim.FacadeDimensions.FacadeThickness);
            Vector3[] innerCell = new Vector3[] { cell[0] + innerOffset, cell[1], cell[2], cell[3] + innerOffset };
            CreateFloors(model, innerCell);

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
            Floor bottomFloor = new Floor(polygon, dim.Types.SlabType, dim.LevelDimensions.Height, BuiltInMaterials.Steel, null, null);
            model.AddElement(bottomFloor);

            //Create a raised floor
            Floor raisedFloor = new Floor(polygon, dim.Types.RaisedFloorType, dim.LevelDimensions.RaisedFloorThickness + dim.LevelDimensions.RaisedFloorVoidHeight, BuiltInMaterials.Wood, null, null);
            model.AddElement(raisedFloor);

            //Create a ceilling
            double ceilingElevation = dim.LevelDimensions.RaisedFloorVoidHeight + dim.LevelDimensions.RaisedFloorThickness + dim.LevelDimensions.Headspace + dim.LevelDimensions.CeilingThickness;
            Floor ceilling = new Floor(polygon, dim.Types.CeillingType, ceilingElevation, BuiltInMaterials.Wood, null, null);
            model.AddElement(ceilling);
        }
    }
}