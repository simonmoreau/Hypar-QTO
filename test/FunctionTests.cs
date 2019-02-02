using Hypar;
using Hypar.Elements;
using Hypar.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace test
{
    /// <summary>
    /// This test suite simulates the execution of your function when running on Hypar.
    /// </summary>
    public class FunctionTest
    {
        

        private Input _data;

        public FunctionTest()
        {
            var serializer = new Amazon.Lambda.Serialization.Json.JsonSerializer();
            
            // Some test data that replicates the payload sent to your function.
            string _testData = File.ReadAllText(@"C:\Users\Simon\Github\Hypar\qto\test\outline.json");
            // Construct a stream from our test data to replicate how Hypar 
            // will get the data.
            using (var stream = GenerateStreamFromString(_testData))
            {
                _data = serializer.Deserialize<Input>(stream);
            }

            // Add a model to the input to simulate a model
            // passing from a previous execution.
            _data.Model = new Model();
            var spaceProfile = new Profile(Polygon.Rectangle(Vector3.Origin, 2, 2));
            var space = new Space(spaceProfile, 0, 2);
            _data.Model.AddElement(space);
        }

        [Fact]
        public void Test()
        {
            // Execute the function.
            var func = new Function();
            var output = func.Handler(_data, null);

            Assert.NotNull(_data.Model);

            // Check that the computed values are as expected.
            var computed = (Dictionary<string,object>)output["computed"];
            Assert.True(Math.Abs((double)computed["area"]) > 0.0);

            var model = Model.FromJson(output["elements"].ToString());
            model.SaveGltf("model.gltf");

            // Serialize the results to json, so we can preview the results.
            // When Lambda runs the function, this is not necessary because it
            // handles serializing the result to a stream.
            // var json = JsonConvert.SerializeObject(output);
            // Console.WriteLine(json);
        }

        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
