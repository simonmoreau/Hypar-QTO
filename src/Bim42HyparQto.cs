using Elements;
using Elements.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using Bim42HyparQto.Outline;

namespace Bim42HyparQto
{
    /// <summary>
    /// A generator for building a simplified model for Quantity TakeOff.
    /// </summary>
  	public class Bim42HyparQto
    {
        public Output Execute(Input input)
        {
            List<Newtonsoft.Json.Linq.JObject> values = input.Data[0].Cast<Newtonsoft.Json.Linq.JObject>().ToList();

            List<Outline.Line> baseLines = values.Select(value => new Outline.Line(value.Children().First().First.ToString())).ToList();

            Dictionary<Level, List<Outline.Line>> baseLinesByLevel = baseLines.GroupBy(x => x.Level.Name)
                                                            .ToDictionary(x => x.First().Level, x => x.ToList());


            // Create a model
            Model model = new Model();
            double area = 0;

            //Create a floor type
            FloorType floorType = new FloorType("Main Floor", 0.2, null);

            Vector3 origin = new Vector3(0, 0, 0);
            Polygon openingPolygon = Polygon.Rectangle(1, 1, origin, 0, 0);
            Opening opening = new Opening(openingPolygon, 0, 0);

            baseLinesByLevel.Keys.ElementAt(baseLinesByLevel.Keys.Count - 1).Height = 1;

            for (int i = 0; i < baseLinesByLevel.Keys.Count - 1; i++)
            {
                baseLinesByLevel.Keys.ElementAt(i).Height = baseLinesByLevel.Keys.ElementAt(i + 1).Elevation - baseLinesByLevel.Keys.ElementAt(i).Elevation;
            }

            foreach (Level level in baseLinesByLevel.Keys)
            {
                List<Outline.Line> facades = baseLinesByLevel[level].Where(l => l.LineType == "facade").ToList();
                List<Polygon> polygons = CreatePolygonsFromLines(facades);
                Polygon perimeter = polygons[0];
                polygons.RemoveAt(0);
                Polygon[] voids = polygons.ToArray();

                List<Opening> openings = polygons.Select(p => new Opening(p, 0, 0)).ToList();
                openings.Add(opening);
                Floor floor = new Floor(perimeter, floorType, level.Elevation, null, null, openings.ToArray());


                Profile levelProfile = null;
                if (voids.Length != 0)
                {
                    levelProfile = new Profile(perimeter, voids, level.Name);
                }
                else
                {
                    levelProfile = new Profile(perimeter, level.Name);
                }
                Transform levelTransform = new Transform(0, 0, level.Elevation);
                Mass levelMass = new Mass(levelProfile, level.Height, null, levelTransform);

                area = area + levelMass.Profile.Perimeter.Area;
                // Add your mass element to a new Model.
                model.AddElement(levelMass);
                model.AddElement(floor);

            }

            // //Create a vertical opening on every floor
            // Vector3 origin = new Vector3(0, 0, 0);
            // Polygon openingPolygon = Polygon.Rectangle(1, 1, origin, 0, 0);
            // Mass openingMass = new Mass(new Profile(openingPolygon, null), 10, null, null);
            // model.AddElement(openingMass);

            // List<Floor> newFloors = new List<Floor>();
            // foreach (Floor floor in model.ElementsOfType<Floor>())
            // {
            //     Opening opening = new Opening(openingPolygon, 0, 0);
            //     List<Opening> openings = new List<Opening>();
            //     openings.Add(opening);
            //     if (floor.Openings != null)
            //     {
            //         openings.AddRange(floor.Openings);
            //     }

            //     Floor newFloor = new Floor(floor.ProfileTransformed.Perimeter, floor.ElementType, floor.Elevation, null, floor.Transform, openings.ToArray());
            //     newFloors.Add(newFloor);
            // }


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