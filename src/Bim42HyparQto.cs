using Hypar.Elements;
using Hypar.Geometry;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hypar
{
    /// <summary>
    /// A generator for building a simplified model for Quantity TakeOff.
    /// </summary>
  	public class Bim42HyparQto
    {
        public Output Execute(Input input)
        {
            /// Your code here.
            // Extract the outline of the building
            Newtonsoft.Json.Linq.JArray json = (Newtonsoft.Json.Linq.JArray)input.Data["levels"];

            // Create a model
            Model model = new Model();
            double area = 0;

            foreach (Newtonsoft.Json.Linq.JToken token in json.Children())
            {
                Hypar.Level level = token.ToObject<Hypar.Level>();
                List<Vector3> points = new List<Vector3>();

                if (level.facades.Count != 0)
                {
									List<List<Line>> lines = new List<List<Line>>();

                    foreach (Line line in level.facades)
                    {
                        //Sort lines into loops
												List<Line> loop = new List<Line>();

                    }

                    Vector3EqualityComparer vector3EqualityComparer = new Vector3EqualityComparer();

                    points = points.Distinct(vector3EqualityComparer).ToList();

                    Hypar.Geometry.Polygon levelOutline = new Polygon(points);
                    // Hypar.Geometry.Profile profile = new Profile()

                    Mass mass = new Mass(levelOutline, level.elevation, 1);
                    // Add your mass element to a new Model.
                    model.AddElement(mass);
                    area = area + mass.Profile.Perimeter.Area;
                }


            }

            var output = new Output(model, area);

            return output;
        }
    }

    public class Line
    {
        public Vector3 startPoint { get; set; }
        public Vector3 endPoint { get; set; }
    }

    public class Data
    {
        public Data()
        {
            this.levels = new List<Level>();
        }
        public List<Level> levels { get; set; }
    }


    public class Level
    {
        public Level()
        {
            this.facades = new List<Line>();
            this.terraces = new List<Line>();
        }
        public string name { get; set; }
        public double elevation { get; set; }
        public List<Line> facades { get; set; }
        public List<Line> terraces { get; set; }
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