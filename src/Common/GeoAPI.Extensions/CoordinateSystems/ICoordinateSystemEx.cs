using GeoAPI.CoordinateSystems;

namespace GeoAPI.Extensions.CoordinateSystems
{
    public static class CoordinateSystemEx
    {
        public static bool IsProjected(this ICoordinateSystem cs)
        {
            if (cs is IProj4CoordinateSystem p4)
                return p4.IsProjected;
            return cs is IProjectedCoordinateSystem;
        }
        public static bool IsGeographic(this ICoordinateSystem cs)
        {
            if (cs is IProj4CoordinateSystem p4)
                return p4.IsGeographic;
            return cs is IGeographicCoordinateSystem;
        }
    }
}
