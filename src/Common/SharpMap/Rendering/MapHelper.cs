using System;
using System.Drawing;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using SharpMap.Api;

namespace SharpMap.Rendering
{
    public static class MapHelper
    {
        public static double ImageToWorld(IMap map, float imageSize)
        {
            Coordinate c1 = map.ImageToWorld(new PointF(0, 0));
            Coordinate c2 = map.ImageToWorld(new PointF(imageSize, imageSize));
            return Math.Abs(c1.X - c2.X);
        }

        public static Coordinate ImageToWorld(IMap map, double width, double height)
        {
            Coordinate c1 = map.ImageToWorld(new PointF(0, 0));
            Coordinate c2 = map.ImageToWorld(new PointF((float)width, (float)height));
            return GeometryFactoryEx.CreateCoordinate(Math.Abs(c1.X - c2.X), Math.Abs(c1.Y - c2.Y));
        }

        public static Envelope GetEnvelope(Coordinate worldPos, double width, double height)
        {
            // maak een rectangle in wereldcoordinaten ter grootte van 20 pixels rondom de click
            var Envelope = new Envelope(worldPos);
            Envelope.SetCentre(worldPos, width, height);
            return Envelope;
        }

        public static Envelope GetEnvelope(Coordinate worldPos, float radius)
        {
            // maak een rectangle in wereldcoordinaten ter grootte van 20 pixels rondom de click
            var Envelope = new Envelope(worldPos);
            Envelope.SetCentre(worldPos, radius, radius);
            return Envelope;
        }
        public static Envelope GetEnvelopeForImage(IMap map, Coordinate centre, double pixelWidth, double pixelHeight)
        {
            var envelope = new Envelope();
            Coordinate size = ImageToWorld(map, pixelWidth, pixelHeight);
            envelope.SetCentre(centre, size.X, size.Y);
            return envelope;
        }
    }
}
