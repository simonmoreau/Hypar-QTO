using Elements;
using Elements.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Bim42HyparQto.Outline
{
    public class Line
    {
        public Line(string line)
        {
            string[] data = line.Split(',');
            this.Level = new Level(data[0], Convert.ToDouble(data[1]));
            this.LineType = data[2];
            this.Start = new Vector3(
                Convert.ToDouble(data[3]),
                Convert.ToDouble(data[4]),
                Convert.ToDouble(data[5])
            );

            this.End = new Vector3(
                Convert.ToDouble(data[6]),
                Convert.ToDouble(data[7]),
                Convert.ToDouble(data[8])
            );

        }

        public Level Level { get; set; }
        public string LineType { get; set; }
        public Vector3 Start { get; set; }
        public Vector3 End { get; set; }

        public Elements.Geometry.Line GetGeometryLine()
        {
            return new Elements.Geometry.Line(this.Start, this.End);
        }

        public Line Reversed()
        {
            Vector3 start = this.Start;
            Vector3 end = this.End;
            this.Start = end;
            this.End = start;
            return this;
        }

    }

    public class Level
    {
        public Level(string name, double elevation)
        {
            this.Name = name;
            this.Elevation = elevation;
            this.Height = 1;
        }

        public Level(string name, double elevation, double height)
        {
            this.Name = name;
            this.Elevation = elevation;
            this.Height = height;
        }

        public string Name { get; set; }
        public double Elevation { get; set; }
        public double Height { get; set; }
    }
}