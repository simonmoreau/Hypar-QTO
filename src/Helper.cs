using Elements;
using Elements.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Bim42HyparQto
{
    /// <summary>
    /// A helper class
    /// </summary>
  	public static class Helper
    {

        public static Line[] GetPolygonLines(Polygon polygon)
        {
            var result = new Line[polygon.Vertices.Length];
            for (var i = 0; i < result.Length; i++)
            {
                Vector3 a = polygon.Vertices[i];

                // Get the list of lines of the polygon
                Vector3 b = i == polygon.Vertices.Length - 1 ? polygon.Vertices[0] : polygon.Vertices[i + 1];
                result[i] = new Line(a, b);
            }
            return result;
        }

        public static Transform[] GetTransformsAtDistance(Line line, double distance)
        {

            var result = new Transform[(int)Math.Floor(line.Length() / distance) + 1];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = line.TransformAt(i * distance / line.Length());
            }
            result[(int)Math.Floor(line.Length() / distance)] = line.TransformAt(1);
            return result;
        }
    }
}