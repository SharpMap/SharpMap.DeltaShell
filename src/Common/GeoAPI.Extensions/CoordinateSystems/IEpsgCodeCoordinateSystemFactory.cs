using GeoAPI.CoordinateSystems;

namespace GeoAPI.Extensions.CoordinateSystems
{
    public interface ISridCoordinateSystemFactory : ICoordinateSystemFactory
    {
        IProj4CoordinateSystem CreateFromSRID(long srid);
    }

    public interface IProj4CoordinateSystem : ICoordinateSystem
    {
        string PROJ4 { get; }

        bool IsGeographic { get; }
        bool IsProjected { get; }
    }

}