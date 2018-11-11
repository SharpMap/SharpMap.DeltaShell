using GeoAPI.CoordinateSystems;

namespace GeoAPI.Extensions.CoordinateSystems
{
    public static class CoordinateSystemEx
    {
        public static bool IsProjected(this ICoordinateSystem cs)
        {
            return cs is IProjectedCoordinateSystem;
        }
        public static bool IsGeographic(this ICoordinateSystem cs)
        {
            return cs is IGeographicCoordinateSystem;
        }
    }
}