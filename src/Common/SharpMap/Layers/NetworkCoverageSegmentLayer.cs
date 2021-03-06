﻿using System.Drawing;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Api;
using SharpMap.Data.Providers;
using SharpMap.Rendering;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;

namespace SharpMap.Layers
{
    public class NetworkCoverageSegmentLayer : NetworkCoverageBaseLayer
    {
        private readonly NetworkCoverageSegmentRenderer segmentRenderer;

        public NetworkCoverageSegmentLayer()
        {
            Name = "Cells";
            DataSource = new NetworkCoverageFeatureCollection
            {
                NetworkCoverageFeatureType = NetworkCoverageFeatureType.Segments
            };

            segmentRenderer = new NetworkCoverageSegmentRenderer();
            CustomRenderers.Add(segmentRenderer);
        }

        protected override void OnInitializeDefaultStyle()
        {
            Style = new VectorStyle
            {
                GeometryType = typeof(ILineString),
                Fill = new SolidBrush(Color.Tomato),
                Line = new Pen(Color.SteelBlue, 3)
            };
        }

        public override Envelope Envelope
        {
            get
            {
                //intersectect the geometries of all the segments.
                if (Coverage == null)
                {
                    return null;
                }

                var result = new Envelope();
                foreach (var s in ((INetworkCoverage)Coverage).Segments.Values)
                {
                    result.ExpandToInclude(s.Geometry.EnvelopeInternal);
                }
                return result;
            }
        }

        protected override void CreateDefaultTheme()
        {
            // If there was no theme attached to the layer yet, generate a default theme
            if (Theme != null) return;

            //looks like min/max should suffice
            var minMaxValue = GetMinMaxValue();
            if (minMaxValue != null)
            {
                var attributeName = ((INetworkCoverage)Coverage).SegmentGenerationMethod == SegmentGenerationMethod.SegmentPerLocation
                                        ? Coverage.Components[0].Name
                                        : "Chainage";

                Theme = ThemeFactory.CreateGradientTheme(attributeName, Style, ColorBlend.Rainbow7,
                                                         (float)minMaxValue.First, (float)minMaxValue.Second,
                                                         10, 25, false, false, 12);
            }
            AutoUpdateThemeOnDataSourceChanged = true;
        }

        public override void OnRender(Graphics g, IMap map)
        {
            segmentRenderer.Render(Coverage, g, this);
        }
    }
}
