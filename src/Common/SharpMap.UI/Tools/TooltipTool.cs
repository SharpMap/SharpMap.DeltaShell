using System;
using System.Windows.Forms;
using GeoAPI.Geometries;
using log4net;
using ToolTip=System.Windows.Forms.ToolTip;

namespace SharpMap.UI.Tools
{
    public class ToolTipTool: MapTool
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (ToolTipTool));

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

        public bool IsBalloon
        {
            get => _toolTip.IsBalloon;
            set => _toolTip.IsBalloon = true;
        }

        public override bool AlwaysActive
        {
            get => true;
        }

        public override bool IsActive
        {
            get { return true; } // always active
            set { }
        }

        public override void OnMouseMove(Coordinate worldPosition, MouseEventArgs e)
        {
            _toolTip.Active = false;
            
            base.OnMouseMove(worldPosition, e);
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

            string message = "Feature: " + feature + "\nLayer: " + layer.Name;

            //if (_toolTipText != message)
            //{
            //    _toolTipText = message;

                if (log.IsDebugEnabled)
                    log.Debug(_toolTipText);

                _toolTip.SetToolTip((Control)MapControl, message);
                //_toolTip.Active = false;
                _toolTip.Active = true;
            //}

            base.OnMouseHover(worldPosition, e);
        }

        //public override OnM
    }
}
