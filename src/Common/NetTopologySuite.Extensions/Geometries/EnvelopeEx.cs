using GeoAPI.Geometries;

namespace NetTopologySuite.Extensions.Geometries
{
    public static class EnvelopeEx
    {
        public static void SetCentre(this Envelope env, Coordinate centre)
        {
            if (env.IsNull)
                return;

            double dx = centre.X - env.Centre.X;
            double dy = centre.Y - env.Centre.Y;

            env.Translate(dx, dy);
        }
        public static void SetCentre(this Envelope env, Coordinate centre, double width, double height)
        {
            //if (env.IsNull)
            //    return;

            width /= 2d;
            height /= 2d;

            var c1 = new Coordinate(centre.X - width, centre.Y - height);
            var c2 = new Coordinate(centre.X + width, centre.Y + height);

            env.Init();
            env.ExpandToInclude(c1);
            env.ExpandToInclude(c2);
        }

        public static void Zoom(this Envelope env, double zoom)
        {
            double width = env.Width * zoom / 100;
            double height = env.Height * zoom / 100;

            env.SetCentre(env.Centre, width, height);
        }
    }
}