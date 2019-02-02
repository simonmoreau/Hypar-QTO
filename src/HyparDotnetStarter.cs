using Hypar.Elements;
using Hypar.Geometry;
using System.Linq;
using System.Collections.Generic;

namespace Hypar
{
    /// <summary>
    /// The Hypar starter generator.
    /// </summary>
  	public class HyparDotnetStarter
	{
		public Output Execute(Input input)
		{
			// Extract the outline of the building
            Newtonsoft.Json.Linq.JArray outlineObject = (Newtonsoft.Json.Linq.JArray)input.Data["outline"];

            List<Vector3> outline = (List<Vector3>)outlineObject.ToObject<List<Vector3>>();
            Hypar.Geometry.Polygon outlinePolygon = new Polygon(outline);

            var origin = outline[0];

            var mass = new Mass(outlinePolygon, 0, 1);

            // Add your mass element to a new Model.
            var model = new Model();
            model.AddElement(mass);

            // Set the origin of the model to convey to Hypar
            // where to position the generated 3D model.
            // model.Origin = origin;

            var output = new Output(model, mass.Profile.Perimeter.Area);

            return output;
		}
  	}
}
