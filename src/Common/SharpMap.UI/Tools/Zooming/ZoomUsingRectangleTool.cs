﻿using System;
using System.Drawing;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System.Windows.Forms;
using SharpMap.UI.Helpers;
using Point=System.Drawing.Point;

namespace SharpMap.UI.Tools.Zooming
{
    /// <summary>
    /// Zooms to selected rectangle.
    /// </summary>
    public class ZoomUsingRectangleTool : ZoomTool
    {
        private bool zooming;
        private Point startDragPoint;
        private Point endDragPoint;

        Bitmap previewImage;

        public ZoomUsingRectangleTool()
        {
            Name = "ZoomInOutUsingRectangle";
        }

        public override void OnMouseDown(Coordinate worldPosition, MouseEventArgs e)
        {
            if(e.Button != MouseButtons.Left) return;

            zooming = true;
            startDragPoint = e.Location;
            endDragPoint = e.Location;
            previewImage = (Bitmap)Map.Image.Clone();
        }

        public override void OnMouseUp(Coordinate worldPosition, MouseEventArgs e)
        {
            if (!zooming)
            {
                return;
            }

            zooming = false;
            previewImage.Dispose();

            // check if rectangle is not too small
            if (Math.Abs(startDragPoint.X - endDragPoint.X) < 5 || Math.Abs(startDragPoint.Y - endDragPoint.Y) < 5)
            {
                MapControl.Refresh();
                return;
            }

            // zoom to selected region
            Coordinate coordinate1 = Map.ImageToWorld(startDragPoint);
            Coordinate coordinate2 = Map.ImageToWorld(endDragPoint);
            Map.ZoomToFit(new Envelope(coordinate1, coordinate2));
            MapControl.Refresh();
        }

        public override void OnMouseMove(Coordinate worldPosition, MouseEventArgs e)
        {
            if (!zooming)
            {
                return;
            }

            endDragPoint = e.Location;

            // draw image and clear background in a separate image first to prevent flickering
            Graphics g = Graphics.FromImage(previewImage);
            g.Clear(MapControl.BackColor);
            g.DrawImage(Map.Image, 0, 0);
            GraphicsHelper.DrawSelectionRectangle(g, Color.DeepSkyBlue, startDragPoint, endDragPoint);
            g.Dispose();

            // now draw it on control
            g = MapControl.CreateGraphics();
            g.DrawImage(previewImage, 0, 0);
            g.Dispose();
        }
    }
}