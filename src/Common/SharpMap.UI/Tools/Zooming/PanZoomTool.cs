﻿using System.Drawing;
using GeoAPI.Geometries;
using System.Windows.Forms;

namespace SharpMap.UI.Tools.Zooming
{
    /// <summary>
    /// Drags map around.
    /// </summary>
    public class PanZoomTool : ZoomTool
    {
        private Bitmap DragImage { get; set; }
        private Bitmap StaticToolsImage { get; set; }
        private bool Dragging { get; set; }
        private Point DragStartPoint { get; set; }
        private Point DragEndPoint { get; set; }

        public PanZoomTool()
        {
            Name = "PanZoom";
        }

        public override bool IsBusy
        {
            get { return Dragging; }
        }

        public override void OnMouseDown(Coordinate worldPosition, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            MapControl.Cursor = Cursors.Hand;

            Dragging = true;
            DragImage = (Bitmap)Map.Image.Clone();
            StaticToolsImage = new Bitmap(Map.Image.Width, Map.Image.Height);
            Graphics g = Graphics.FromImage(StaticToolsImage);

            foreach (IMapTool tool in MapControl.Tools)
            {
                if (tool.IsActive)
                {
                    tool.OnPaint(new PaintEventArgs(g, MapControl.ClientRectangle));
                }
            }
            g.Dispose();

            DragStartPoint = e.Location;
            DragEndPoint = e.Location;
        }

        public override void OnMouseUp(Coordinate worldPosition, MouseEventArgs e)
        {
            if (!Dragging)
            {
                return;
            }
            DragImage.Dispose();
            StaticToolsImage.Dispose();
            Dragging = false;
            Point point = new Point((MapControl.ClientSize.Width/2 + (DragStartPoint.X - DragEndPoint.X)),
                                    (MapControl.ClientSize.Height/2 + (DragStartPoint.Y - DragEndPoint.Y)));
            Map.Center = Map.ImageToWorld(point);

            MapControl.Cursor = Cursors.Default;
            MapControl.Refresh();
        }

        public override void OnMouseMove(Coordinate worldPosition, MouseEventArgs e)
        {
            if (!Dragging)
            {
                return;
            }
            DragEndPoint = e.Location;

            var point = new Point((DragEndPoint.X - DragStartPoint.X),
                                    (DragEndPoint.Y - DragStartPoint.Y));

            var previewImage = new Bitmap(DragImage.Width, DragImage.Height);
            Graphics g = Graphics.FromImage(previewImage);
            g.Clear(MapControl.BackColor);
            g.DrawImageUnscaled(DragImage, point);
            g.DrawImageUnscaled(StaticToolsImage, 0, 0);
            g.Dispose();

            g = MapControl.CreateGraphics();
            g.DrawImage(previewImage, 0, 0);
            g.Dispose();

            previewImage.Dispose();
        }
    }
}