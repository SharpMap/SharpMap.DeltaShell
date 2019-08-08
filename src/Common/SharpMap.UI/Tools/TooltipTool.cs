using System;
using System.Windows.Forms;
using GeoAPI.Geometries;
using log4net;
using SharpMap.UI.Forms;
using ToolTip=System.Windows.Forms.ToolTip;

namespace SharpMap.UI.Tools
{
    public class ToolTipTool: MapTool
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (ToolTipTool));

        private System.Drawing.Point _hoverLocation;

        private readonly ToolTip _toolTip;
        private string _toolTipText;

        public ToolTipTool()
        {
            Name = "ToolTipTool";
            _toolTip = new ToolTip();
            _toolTip.ReshowDelay = 1500;
            _toolTip.InitialDelay = 500;
            _toolTip.IsBalloon = true;
            _toolTip.AutoPopDelay = 3000;
        }

        public int InitialDelay
        {
            get => _toolTip.InitialDelay;
            set => _toolTip.InitialDelay = value;
        }
        public int ReshowDelay
        {
            get => _toolTip.ReshowDelay;
            set => _toolTip.ReshowDelay = value;
        }

        public int AutoPopDelay
        {
            get => _toolTip.AutoPopDelay;
            set => _toolTip.AutoPopDelay = value;
        }

        public bool IsBalloon
        {
            get => _toolTip.IsBalloon;
            set => _toolTip.IsBalloon = value;
        }

        public override bool AlwaysActive
        {
            get => true;
        }

        /// <summary>
        /// Gets or sets the value that a mouse movement is tolerated prior to removing tool tip
        /// </summary>
        public int MouseChangeTolerance { get; set; }

        public override void OnMouseMove(Coordinate worldPosition, MouseEventArgs e)
        {
            if (_toolTip.Active && (
                Math.Abs(_hoverLocation.X - e.Location.X) > MouseChangeTolerance ||
                Math.Abs(_hoverLocation.Y - e.Location.Y) > MouseChangeTolerance))
                _toolTip.Active = false;

            base.OnMouseMove(worldPosition, e);
        }

        public override void OnMouseDown(Coordinate worldPosition, MouseEventArgs e)
        {
            _toolTip.Active = false;
            base.OnMouseDown(worldPosition, e);
        }

        public override void OnMouseHover(Coordinate worldPosition, EventArgs e)
        {
            // Allow for 3px offset
            float limit = 12f * (float)Map.PixelWidth;
            var feature = FindNearestFeature(worldPosition, limit, out var layer, t => t.IsSelectable);

            if (feature == null || layer == null)
            {
                _toolTip.Active = false;
                return;
            }

            _hoverLocation = MapControl.PointToClient(Control.MousePosition);
            string message = "Feature: " + feature + "\nLayer: " + layer.Name;

            if (log.IsDebugEnabled)
                log.Debug(_toolTipText);

            _toolTip.SetToolTip((Control)MapControl, message);
            _toolTip.Active = true;

            base.OnMouseHover(worldPosition, e);
        }
    }
}
