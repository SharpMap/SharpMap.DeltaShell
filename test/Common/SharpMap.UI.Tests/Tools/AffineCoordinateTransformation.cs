using System;
using System.Collections.Generic;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;
using Rhino.Mocks.Constraints;

namespace SharpMap.UI.Tests.Tools
{
    internal class AffineCoordinateTransformation : ICoordinateTransformation
    {
        private class AffineMathTransform : IMathTransform
        {
            private readonly NetTopologySuite.Geometries.Utilities.AffineTransformation _at;
            private AffineMathTransform _inverse;
            private bool _isInverted;

            public AffineMathTransform(AffineTransformation at)
            {
                _at = at;
            }

            internal AffineTransformation AT { get => _isInverted ? ((AffineMathTransform)Inverse())._at : _at; }

            public bool Identity() => AT.IsIdentity;

            public double[,] Derivative(double[] point)
            {
                throw new System.NotSupportedException();
            }

            public List<double> GetCodomainConvexHull(List<double> points)
            {
                throw new System.NotSupportedException();
            }

            public DomainFlags GetDomainFlags(List<double> points)
            {
                throw new System.NotSupportedException();
            }

            public IMathTransform Inverse()
            {
                return _inverse ?? (_inverse = new AffineMathTransform(_at.GetInverse()));
            }

            public double[] Transform(double[] point)
            {
                var s = new Coordinate(point[0], point[1]);
                var t = s.Copy();
                AT.Transform(s, t);
                double[] res = (double[])point.Clone();
                res[0] = t.X;
                res[1] = t.Y;
                return res;
            }

            [Obsolete]
            public ICoordinate Transform(ICoordinate coordinate)
            {
                return Transform(new Coordinate(coordinate));
            }

            public Coordinate Transform(Coordinate s)
            {
                var t = s.Copy();
                return AT.Transform(s, t);
            }

            public IList<double[]> TransformList(IList<double[]> points)
            {
                var res = new List<double[]>(points.Count);
                foreach (var point in points)
                    res.Add(Transform(point));
                return res;
            }

            public IList<Coordinate> TransformList(IList<Coordinate> points)
            {
                var res = new List<Coordinate>(points.Count);
                foreach (var point in points)
                    res.Add(Transform(point));
                return res;
            }

            public void Invert()
            {
                _isInverted = !_isInverted;
            }

            public ICoordinateSequence Transform(ICoordinateSequence coordinateSequence)
            {
                var res = coordinateSequence.Copy();
                for (int i = 0; i < coordinateSequence.Count; i++)
                {
                    double[] pt = Transform(new []{coordinateSequence.GetX(i), coordinateSequence.GetY(i)});
                    res.SetOrdinate(i, Ordinate.X, pt[0]);
                    res.SetOrdinate(i, Ordinate.Y, pt[1]);
                }

                return res;
            }

            public int DimSource { get => 2; }
            public int DimTarget { get => 2; }
            public string WKT { get => "AFFINETRANSFORM"; }
            public string XML { get => string.Empty; }
        }

        private class AffineCoordinateSystem : ICoordinateSystem
        {
            public bool EqualParams(object obj)
            {
                if (obj is AffineCoordinateSystem cs)
                {
                    return (Authority == cs.Authority) & AuthorityCode == cs.AuthorityCode;
                }

                return false;
            }

            public string Name { get; set; }
            public string Authority { get; set; }
            public long AuthorityCode { get; set;  }
            public string Alias { get; }
            public string Abbreviation { get; }
            public string Remarks { get; }
            public string WKT { get; }
            public string XML { get; }

            public AxisInfo GetAxis(int dimension)
            {
                if (dimension == 0) return new AxisInfo("X", AxisOrientationEnum.East);
                if (dimension == 1) return new AxisInfo("Y", AxisOrientationEnum.North);
                throw new ArgumentOutOfRangeException(nameof(dimension));
            }

            private class LinearUnit : IUnit {
                public static IUnit Instance { get; } = new LinearUnit();

                public bool EqualParams(object obj)
                {
                    throw new NotImplementedException();
                }

                public string Name { get => "metre"; }
                public string Authority { get => "NTS"; }
                public long AuthorityCode { get => 1000002; }
                public string Alias { get; }
                public string Abbreviation { get; }
                public string Remarks { get; }
                public string WKT { get; }
                public string XML { get; }
            }

            public IUnit GetUnits(int dimension)
            {
                if (dimension == 0) return LinearUnit.Instance;
                if (dimension == 1) return LinearUnit.Instance;
                throw new ArgumentOutOfRangeException(nameof(dimension));
            }

            public int Dimension { get; }

            public double[] DefaultEnvelope { get; }
        }

        private readonly AffineMathTransform _amt;

        public AffineCoordinateTransformation(AffineTransformation at)
        {
            _amt = new AffineMathTransform(at);
        }

        public string AreaOfUse { get; }
        public string Authority { get => "NTS"; }
        public long AuthorityCode { get => 1000001; }
        public IMathTransform MathTransform { get => _amt; }
        public string Name { get => "affine_coordinate_transformation"; }
        public string Remarks { get; }
        public ICoordinateSystem SourceCS { get; }
        public ICoordinateSystem TargetCS { get; }
        public TransformType TransformType { get => TransformType.Conversion; }
    }
}
