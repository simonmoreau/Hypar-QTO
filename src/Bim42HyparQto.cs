using Elements;
using Elements.Geometry;
using System.Linq;
using System.Collections.Generic;

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

						foreach (Newtonsoft.Json.Linq.JObject value in values) 
						{
							Newtonsoft.Json.Linq.JToken dataline = value.Children().First();
							
							string test = dataline.First.ToString().Split(',')[3];
						}
						
            // Create a model
            Model model = new Model();
            double area = 0;

            List<Vector3> points = new List<Vector3>();
            points.Add(new Vector3(0, 0, 0));
            points.Add(new Vector3(0, 10, 0));
            points.Add(new Vector3(10, 12, 0));
            points.Add(new Vector3(10, 2, 0));

            Polygon levelOutline = new Polygon(points.ToArray());
            Profile profile = new Profile(levelOutline);

            Mass mass = new Mass(profile, 1, null);
            // Add your mass element to a new Model.
            model.AddElement(mass);
            area = area + mass.Profile.Perimeter.Area;

            return new Output(model, area); ;
        }
    }

    public class dataline
    {
			public dataline()
			{

			}
			public string levels {get;set;}
    }
}