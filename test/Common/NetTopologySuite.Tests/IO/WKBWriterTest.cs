using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.IO
{
    [TestFixture]
    public class WKBWriterTest
    {
        [Test]
        public void TestWritingEmptyLineString()
        {
            var wkbWriter = new WKBWriter();
            var memoryStream = new MemoryStream();
            var linestring = new LineString(new Coordinate[0]);
            
            Assert.IsNull(linestring.Coordinate);

            try
            {
                wkbWriter.Write(linestring, memoryStream);
            }
            finally
            {
                memoryStream.Close();
                memoryStream.Dispose();
            }
        }
    }
}