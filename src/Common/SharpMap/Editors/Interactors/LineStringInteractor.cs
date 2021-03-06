﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Editing;
using SharpMap.Api;
using SharpMap.Api.Editors;
using SharpMap.Editors.FallOff;
using SharpMap.Editors.Snapping;
using SharpMap.Layers;
using SharpMap.Styles;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using System.Windows.Forms;
using System.Reflection;
using GeometryFactory = SharpMap.Converters.Geometries.GeometryFactory;

namespace SharpMap.Editors.Interactors
{
    public class LineStringInteractor : FeatureInteractor
    {
#if DEBUG
        public TrackerFeature AllTracker { get; protected set; }
#else
        protected TrackerFeature AllTracker { get; set; }
#endif
        static private Bitmap trackerSmallStart;
        static private Bitmap trackerSmallEnd;
        static private Bitmap trackerSmall;
        static private Bitmap selectedTrackerSmall;

        public LineStringInteractor(ILayer layer, IFeature feature, VectorStyle vectorStyle, IEditableObject editableObject)
            : base(layer, feature, vectorStyle, editableObject)
        {
        }

        protected override void CreateTrackers()
        {
            if (SourceFeature == null || SourceFeature.Geometry == null)
            {
                return;
            }

            if (trackerSmallStart == null)
            {
                trackerSmallStart = TrackerSymbolHelper.GenerateSimple(new Pen(Color.Blue), new SolidBrush(Color.DarkBlue), 6, 6);
                trackerSmallEnd = TrackerSymbolHelper.GenerateSimple(new Pen(Color.Tomato), new SolidBrush(Color.Maroon), 6, 6);
                trackerSmall = TrackerSymbolHelper.GenerateSimple(new Pen(Color.Green), new SolidBrush(Color.Lime), 6, 6);
                selectedTrackerSmall = TrackerSymbolHelper.GenerateSimple(new Pen(Color.DarkMagenta), new SolidBrush(Color.Magenta), 6, 6);
            }

            Trackers.Clear();
            Trackers.AddRange(CreateTrackersForGeometry(SourceFeature.Geometry));

            AllTracker = new TrackerFeature(this, null, -1, null);
        }

        protected IEnumerable<TrackerFeature> CreateTrackersForGeometry(IGeometry geometry)
        {
            var coordinates = geometry.Coordinates;
            if (coordinates.Length == 0)
            {
                yield break;
            }
            
            yield return new TrackerFeature(this, GeometryFactory.CreatePoint(coordinates[0].X, coordinates[0].Y), 0,
                                            trackerSmallStart);

            for (var i = 1; i < coordinates.Length - 1; i++)
            {
                yield return
                    new TrackerFeature(this, GeometryFactory.CreatePoint(coordinates[i].X, coordinates[i].Y), i,
                                       trackerSmall);
            }

            if (coordinates.Length > 1)
            {
                yield return
                    new TrackerFeature(this, GeometryFactory.CreatePoint(coordinates.Last().X, coordinates.Last().Y),
                                       coordinates.Length - 1, trackerSmallEnd);
            }
        }

        public override TrackerFeature GetTrackerAtCoordinate(Coordinate worldPos)
        {
            var trackerFeature = base.GetTrackerAtCoordinate(worldPos);

            if (trackerFeature == null)
            {
                var org = Layer.Map.ImageToWorld(new PointF(0, 0));
                var range = Layer.Map.ImageToWorld(new PointF(6, 6)); // todo make attribute
                if (SourceFeature.Geometry.Distance(GeometryFactory.CreatePoint(worldPos)) < Math.Abs(range.X - org.X))
                {
                    return AllTracker;
                }
            }
            return trackerFeature;
        }

        public override bool MoveTracker(TrackerFeature trackerFeature, double deltaX, double deltaY,
                                         SnapResult snapResult = null)
        {
            if (trackerFeature == AllTracker)
            {
                if (FallOffPolicy == null)
                {
                    FallOffPolicy = new NoFallOffPolicy();
                }

                var handles = TrackerIndices.ToList();

                FallOffPolicy.Move(TargetFeature.Geometry, Trackers.Select(t => t.Geometry).ToList(),
                                   handles, -1, deltaX, deltaY);

                foreach (var topologyRule in FeatureRelationEditors)
                {
                    topologyRule.UpdateRelatedFeatures(SourceFeature, TargetFeature.Geometry, handles);
                }

                return true;
            }
            return base.MoveTracker(trackerFeature, deltaX, deltaY, snapResult);
        }

        public override Cursor GetCursor(TrackerFeature trackerFeature)
        {
            if (trackerFeature == AllTracker)
            {
                return Cursors.SizeAll;
            }
            return new Cursor(Assembly.GetExecutingAssembly().GetManifestResourceStream("SharpMap.Editors.Cursors.Move.cur"));
        }

        public override void SetTrackerSelection(TrackerFeature trackerFeature, bool select)
        {
            trackerFeature.Selected = select;
            trackerFeature.Bitmap = select ? selectedTrackerSmall : trackerSmall;
        }
    }
}