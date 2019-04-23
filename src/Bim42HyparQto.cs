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
        public Output Execute(Input input)
        {

            // Create a model
            Model model = new Model();
            double area = 0;

            //Set building lenght
            int module_number = 20;
            building_lenght = module_lenght * module_number;

            double level_height = 3.5;

            //Set structural dimensions
            double beam_height = 0.7; //full dimension, including slab thickness
            double slab_height = 0.2;
            double column_diameter = 0.5;
            double facade_thickness = 0.4;

            //Create wall and floor types
            FloorType slabType = new FloorType("slab", slab_height, null);
            FloorType raisedFloorype = new FloorType("Raised floor", 0.03, null);

            WallType facadeType = new WallType("façade", facade_thickness, "façade");

            Line topAxe = new Line(new Vector3(0, building_width, 0), new Vector3(building_lenght, building_width, 0));
            Line bottomAxe = new Line(new Vector3(0, 0, 0), new Vector3(building_lenght, 0, 0));

            Grid grid = new Grid(bottomAxe, topAxe, module_number, 3);
            int maxRow = grid.Cells().GetLength(1) - 1;

            for (int i = 0; i < grid.Cells().GetLength(0); i++)
            {
                Vector3[] bottomCell = grid.Cells()[i, 0];
                Vector3[] topCell = grid.Cells()[i, 0];

                //Create a façade
                Vector3 facadeOffset = new Vector3(0,facade_thickness/2,0);
                Line bottomBaseLine = new Line(bottomCell[0]+facadeOffset, bottomCell[3]+facadeOffset);
                Wall bottomWall = new Wall(bottomBaseLine, facadeType, level_height, BuiltInMaterials.Glass, null, null);
                model.AddElement(bottomWall);

                Vector3 innerOffset = new Vector3(0,facade_thickness,0);
                Vector3[] innerCell = new Vector3[] {bottomCell[0] + innerOffset,bottomCell[1],bottomCell[2],bottomCell[3] + innerOffset};
                //Create a slab
                Polygon bottomPolygon = new Polygon(innerCell);
                Floor bottomFloor = new Floor(bottomPolygon, slabType, level_height, BuiltInMaterials.Steel, null, null);
                model.AddElement(bottomFloor);
                area = area + bottomFloor.Area();

                //Create a raised floor
                Floor raisedFloor = new Floor(bottomPolygon, raisedFloorype, 0.13, BuiltInMaterials.Wood, null, null);
                model.AddElement(raisedFloor);



                //Create a beam each 3 module
                Math.DivRem(i, 3, out int remainer);
                if (remainer == 0)
                {
                    //Create columns
                    Profile circularColumnProfile = new Profile(Polygon.Circle(column_diameter / 2));
                    double column_height = level_height - beam_height;
                    Column circularColumn = new Column(innerCell[0] + new Vector3(0, 0.5, 0), column_height, circularColumnProfile, BuiltInMaterials.Steel, null, 0, 0);
                    model.AddElement(circularColumn);

                    //Create beams
                    Profile beamProfile = new Profile(Polygon.Rectangle(column_diameter - 0.2, beam_height - slab_height));
                    Vector3 beamElevation = new Vector3(0, 0, level_height - slab_height - (beam_height - slab_height) / 2);
                    Line beamLine = new Line(innerCell[0] + beamElevation, innerCell[1] + beamElevation);


                    Beam beam = new Beam(beamLine, beamProfile, BuiltInMaterials.Steel);
                    model.AddElement(beam);
                }

                Polygon topPolygon = new Polygon(grid.Cells()[i, maxRow]);
                Floor topFloor = new Floor(topPolygon, slabType, 0, null, null, null);
                model.AddElement(topFloor);

                Line topBaseLine = new Line(grid.Cells()[i, maxRow][1], grid.Cells()[i, maxRow][2]);
                Wall topWall = new Wall(topBaseLine, facadeType, level_height, BuiltInMaterials.Glass, null, null);
                model.AddElement(topWall);

                area = area + topFloor.Area();
            }


            return new Output(model, area); ;
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