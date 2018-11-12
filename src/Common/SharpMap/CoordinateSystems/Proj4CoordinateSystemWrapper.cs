using GeoAPI.CoordinateSystems;
using GeoAPI.Extensions.CoordinateSystems;

namespace SharpMap.CoordinateSystems
{
    public class Proj4CoordinateSystemWrapper : IProj4CoordinateSystem
    {
        private readonly ICoordinateSystem _coordinateSystem;

        public bool IsGeographic => _coordinateSystem is IGeographicCoordinateSystem;

        public bool IsProjected => _coordinateSystem is IProjectedCoordinateSystem;

        public Proj4CoordinateSystemWrapper(ICoordinateSystem coordinateSystem)
        {
            _coordinateSystem = coordinateSystem;
        }

        public string PROJ4 { get; set; }

        public bool EqualParams(object obj)
        {
            return _coordinateSystem.EqualParams(obj);
        }

        public string Name => _coordinateSystem.Name;

        public string Authority => _coordinateSystem.Authority;

        public long AuthorityCode => _coordinateSystem.AuthorityCode;

        public string Alias => _coordinateSystem.Alias;

        public string Abbreviation => _coordinateSystem.Abbreviation;

        public string Remarks => _coordinateSystem.Remarks;

        public string WKT => _coordinateSystem.WKT;

        public string XML => _coordinateSystem.XML;

        public AxisInfo GetAxis(int dimension)
        {
            return _coordinateSystem.GetAxis(dimension);
        }

        public IUnit GetUnits(int dimension)
        {
            return _coordinateSystem.GetUnits(dimension);
        }

        public int Dimension => _coordinateSystem.Dimension;

        public double[] DefaultEnvelope => _coordinateSystem.DefaultEnvelope;
    }
}