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

        public double building_width = 18.5;
        public double building_lenght;
        public double core_width = 5;
        public double module_lenght = 1.35;

        //Set building lenght
        public int module_number = 20;
        //Set level height definition
        public double level_height = 3.5;
        public double ceiling_void_height = 0.05;
        public double ceiling_thickness = 0.05;
        public double raised_floor_void_height = 0.07;
        public double raised_floor_thickness = 0.03;
        public double headspace = 2.7;


        //Set structural dimensions
        public double beam_height = 0.6; //full height, including slab thickness
        public double slab_height = 0.2;
        public double column_diameter = 0.5;
        public double facade_thickness = 0.4;

        public Output Execute(Input input)
        {

            // Create a model
            Model model = new Model();
            double area = 0;
            building_lenght = module_lenght * module_number;
            level_height =
            raised_floor_void_height +
            raised_floor_thickness +
            headspace +
            ceiling_thickness +
            ceiling_void_height +
            beam_height;

            double office_space_width = (building_width - core_width) / 2;

            Line northFacadeLine = new Line(new Vector3(0, building_width, 0), new Vector3(building_lenght, building_width, 0));
            Line northInnerLine = new Line(new Vector3(0, office_space_width + core_width, 0), new Vector3(building_lenght, office_space_width + core_width, 0));

            Line southFacadeLine = new Line(new Vector3(0, 0, 0), new Vector3(building_lenght, 0, 0));
            Line southInnerLine = new Line(new Vector3(0, office_space_width, 0), new Vector3(building_lenght, office_space_width, 0));

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

            CreateMeetingRoom(model,meetingCell);

            CreateFloors(model, coreGrid.Cells()[0,0]);
        }

        public void CreateMeetingRoom(Model model, Vector3[] cell)
        {
            WallType facadeType = new WallType("meeting", 0.1, "meeting");
            Vector3 elevation = new Vector3(0,0,raised_floor_thickness + raised_floor_void_height);

            for (int i =0; i < cell.Count();i++)
            {
                Vector3 startPoint = cell[i] + elevation;
                Vector3 endPoint = cell[i] + elevation;
                if (i== 0) {startPoint = cell[cell.Count() -1] + elevation;} else {startPoint = cell[i-1] + elevation;}

                Line wallLine = new Line(startPoint, endPoint);

                Wall meetingWall = new Wall(wallLine, facadeType, headspace, BuiltInMaterials.Glass);
                model.AddElement(meetingWall);
                
            }
        }

        public double CreateOfficeSpace(Model model, Line façadeLine, Line innerLine, double area)
        {
            Grid spaceGrid = new Grid(façadeLine, innerLine, 1, 1);
            Vector3[] spaceCell = spaceGrid.Cells()[0, 0];
            //Helper vector
            Vector3 towardInside = (spaceCell[1] - spaceCell[0]).Normalized();

            Vector3 facadeOffset = towardInside * (facade_thickness);
            Vector3 corridorOffset = towardInside * (-1.5);
            Vector3[] officeCell = new Vector3[] { spaceCell[0] + facadeOffset, spaceCell[1] + corridorOffset, spaceCell[2] + corridorOffset, spaceCell[3] + facadeOffset };
            Vector3[] corridorCell = new Vector3[] { spaceCell[1] + corridorOffset, spaceCell[1], spaceCell[2], spaceCell[2] + corridorOffset };

            //Create a space
            Polygon officePolygon = new Polygon(officeCell);
            Polygon corridorPolygon = new Polygon(corridorCell);

            Material MintMaterial = new Material("Mint", GeometryEx.Palette.Mint);
            Material GreenMaterila = new Material("Green", GeometryEx.Palette.Green);
            Space officeSpace = new Space(new Profile(officePolygon), headspace, raised_floor_thickness + raised_floor_void_height, MintMaterial, null);
            model.AddElement(officeSpace);
            area = area + officePolygon.Area;

            Space corridorSpace = new Space(new Profile(corridorPolygon), headspace, raised_floor_thickness + raised_floor_void_height, MintMaterial, null);
            model.AddElement(corridorSpace);
            area = area + corridorPolygon.Area;


            return area;
        }

        public void CreateFaçade(Model model, Line façadeLine, Line innerLine)
        {
            Grid moduleGrid = new Grid(façadeLine, innerLine, module_number, 1);

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
            WallType facadeType = new WallType("façade", facade_thickness, "façade");

            //Helper vector
            Vector3 towardInside = (cell[1] - cell[0]).Normalized();

            //Create a façade
            Vector3 facadeOffset = towardInside * (facade_thickness / 2);
            Line bottomBaseLine = new Line(cell[0] + facadeOffset, cell[3] + facadeOffset);
            Wall bottomWall = new Wall(bottomBaseLine, facadeType, level_height, BuiltInMaterials.Glass, null, null);
            model.AddElement(bottomWall);

            Vector3 innerOffset = towardInside * (facade_thickness);
            Vector3[] innerCell = new Vector3[] { cell[0] + innerOffset, cell[1], cell[2], cell[3] + innerOffset };
            CreateFloors(model, innerCell);

            if (structural)
            {
                //Create columns
                Profile circularColumnProfile = new Profile(Polygon.Circle(column_diameter / 2));
                double column_height = level_height - beam_height;
                Vector3 columnOffset = towardInside * 0.5;
                Column circularColumn = new Column(innerCell[0] + columnOffset, column_height, circularColumnProfile, BuiltInMaterials.Steel, null, 0, 0);
                model.AddElement(circularColumn);

                //Create beams
                Profile beamProfile = new Profile(Polygon.Rectangle(column_diameter, beam_height - slab_height));
                Vector3 beamElevation = new Vector3(0, 0, level_height - slab_height - (beam_height - slab_height) / 2);
                Line beamLine = new Line(innerCell[0] + beamElevation, innerCell[1] + beamElevation);


                Beam beam = new Beam(beamLine, beamProfile, BuiltInMaterials.Steel);
                model.AddElement(beam);
            }

        }

        public void CreateFloors(Model model, Vector3[] cell)
        {
            //Create wall and floor types
            FloorType slabType = new FloorType("slab", slab_height, null);
            FloorType raisedFloorType = new FloorType("Raised floor", raised_floor_thickness, null);
            FloorType ceillingType = new FloorType("Ceilling", ceiling_thickness, null);

            //Create a slab
            Polygon polygon = new Polygon(cell);
            Floor bottomFloor = new Floor(polygon, slabType, level_height, BuiltInMaterials.Steel, null, null);
            model.AddElement(bottomFloor);

            //Create a raised floor
            Floor raisedFloor = new Floor(polygon, raisedFloorType, raised_floor_thickness + raised_floor_void_height, BuiltInMaterials.Wood, null, null);
            model.AddElement(raisedFloor);

            //Create a ceilling
            double ceilingElevation = raised_floor_void_height + raised_floor_thickness + headspace + ceiling_thickness;
            Floor ceilling = new Floor(polygon, ceillingType, ceilingElevation, BuiltInMaterials.Wood, null, null);
            model.AddElement(ceilling);
        }

        public List<Polygon> CreatePolygonsFromLines(List<Outline.Line> lines)
        {
            List<Vector3> points = new List<Vector3>();
            List<Polygon> polygons = new List<Polygon>();
            Outline.Line nextLine = lines[0];

            Vector3EqualityComparer compare = new Vector3EqualityComparer();

            while (lines.Count != 0)
            {
                nextLine = lines[0];
                while (nextLine != null)
                {
                    points.Add(nextLine.Start);
                    lines.Remove(nextLine);
                    Vector3 anglePoint = nextLine.End;
                    nextLine = lines.FirstOrDefault(l => compare.Equals(l.Start, anglePoint) || compare.Equals(l.End, anglePoint));
                    if (nextLine == null)
                    {
                        break;
                    }
                    if (compare.Equals(anglePoint, nextLine.End))
                    {
                        nextLine = nextLine.Reversed();
                    }
                }
                Polygon polygon = new Polygon(points.ToArray());
                if (polygon.Area < 0) { polygon = polygon.Reversed(); }
                if (polygon.Area != 0) { polygons.Add(polygon); }

                points.Clear();
            }

            polygons = polygons.OrderByDescending(p => p.Area).ToList();

            return polygons;
        }

    }

    class Vector3EqualityComparer : IEqualityComparer<Vector3>
    {
        public bool Equals(Vector3 v1, Vector3 v2)
        {
            if (v2 == null && v1 == null)
                return true;
            else if (v1 == null || v2 == null)
                return false;
            else if (v1.X == v2.X && v1.Y == v2.Y
                                && v1.Z == v2.Z)
                return true;
            else
                return false;
        }

        public int GetHashCode(Vector3 bx)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + bx.X.GetHashCode();
                hash = hash * 23 + bx.Y.GetHashCode();
                hash = hash * 23 + bx.Z.GetHashCode();
                return hash;
            }
        }
    }
}