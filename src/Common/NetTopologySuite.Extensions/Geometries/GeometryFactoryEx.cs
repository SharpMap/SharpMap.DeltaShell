using GeoAPI.Geometries;

namespace NetTopologySuite.Extensions.Geometries
{
    public static class GeometryFactoryEx
    {
        public static Coordinate CreateCoordinate(double x, double y)
        {
            return new Coordinate(x, y);
        }
    }
}