using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Editing;
using GeoAPI.Geometries;
using System.Windows.Forms;
using SharpMap.Api;
using SharpMap.Api.Editors;
using SharpMap.Editors.Snapping;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.CoordinateSystems.Transformations;
using NetTopologySuite.Geometries;
using SharpMap.Editors;
using SharpMap.UI.Helpers;

namespace SharpMap.UI.Tools
{
    public class CurvePointTool : MapTool
    {
        private bool isMoving;
        private bool isBusy;
        private EditMode mode;
        private SnapResult SnapResult { get; set; }

        public EditMode Mode
        {
            get
            {
                return (Control.ModifierKeys & Keys.Alt) == Keys.Alt ? EditMode.Remove : mode;
            }
            set { mode = value; }
        }

        public enum EditMode { Add, Remove };

        private string ActionName
        {
            get { return Mode == EditMode.Add ? "Adding coordinate to geometry of" : "Removing coordinate from geometry of"; }
        }

        public CurvePointTool()
        {
            Name = "CurvePoint";
        }

        private SelectTool SelectTool { get { return MapControl.SelectTool; } }

        private MoveTool MoveTool { get { return MapControl.MoveTool; } }

        /// <summary>
        /// snapping specific for a tool. Called before layer specific snapping is applied.
        /// </summary>
        /// <returns></returns>
        private void Snap(IGeometry snapSource, Coordinate worldPos)
        {
            SnapResult = null;
            var fi = SelectTool.SelectedFeatureInteractors[0];
            var sourceFeature = fi.SourceFeature;
            if (!Equals(sourceFeature.Geometry, snapSource))
            {
                return;
            }

            //if (fi.Layer.CoordinateTransformation != null)
            //    worldPos = fi.Layer.CoordinateTransformation.MathTransform.Inverse().Transform(worldPos);

            SnapRole snapRole;
            if (Mode == EditMode.Add)
            {
                snapRole = SnapRole.FreeAtObject;
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                    snapRole = SnapRole.Free;
            }
            else
            {
                snapRole = SnapRole.AllTrackers;
            }

            ISnapRule snapRule = new SnapRule {Obligatory = true, SnapRole = snapRole, PixelGravity = 4};

            SnapResult = MapControl.SnapTool.ExecuteSnapRule(snapRule, sourceFeature, sourceFeature.Geometry,
                                                             new List<IFeature> {sourceFeature}, worldPos, -1);
        }

        public override bool IsBusy
        {
            get { return isBusy; }
        }

        private static bool SupportedGeometry(IGeometry geometry)
        {
            return geometry is ILineString || geometry is IPolygon;
        }

        public override void OnMouseUp(Coordinate worldPosition, MouseEventArgs e)
        {
            if (isMoving)
            {
                MoveTool.OnMouseUp(worldPosition, e);
            }
            else
            {
                SelectTool.OnMouseUp(worldPosition, e);
            }

            isBusy = false;
            isMoving = false;
        }

        public override void OnMouseMove(Coordinate worldPosition, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.None && !isMoving)
            {
                return;
            }

            if (isMoving)
            {
                MoveTool.OnMouseMove(worldPosition, e);
            }
            else if ((SelectTool.SelectedFeatureInteractors.Count == 1)
                     && SupportedGeometry(SelectTool.SelectedFeatureInteractors[0].SourceFeature.Geometry))
            {
                SelectTool.OnMouseMove(worldPosition, e);
                Snap(SelectTool.SelectedFeatureInteractors[0].SourceFeature.Geometry, worldPosition);
                SetMouseCursor(worldPosition);
                StartDrawing();
                DoDrawing(true);
                StopDrawing();
            }
        }

        private void SetMouseCursor(Coordinate worldPosition)
        {
            //var cursor = SnapResult == null ? Cursors.Default : Mode == EditMode.Add ? MapCursors.AddPoint : MapCursors.RemovePoint;
            var cursor = MapControl.Cursor;
            if (SnapResult?.Location != null)
            {
                var fi = SelectTool.SelectedFeatureInteractors.FirstOrDefault();
                if (fi != null)
                {
                    var tf = fi.GetTrackerAtCoordinate(SnapResult.Location);
                    if (Mode == EditMode.Add && tf != null && tf.Index != -1)
                        cursor = MapCursors.MovePoint;
                    else
                        cursor = Mode == EditMode.Add ? MapCursors.AddPoint : MapCursors.RemovePoint;
                }
            }

            if (!ReferenceEquals(MapControl.Cursor, cursor))
            {
                MapControl.Cursor = cursor;
            }
        }

        public override void OnMouseDown(Coordinate worldPosition, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            isBusy = true;

            if (SelectTool.SelectedFeatureInteractors.Count != 1)
            {
                SelectTool.OnMouseDown(worldPosition, e);
                return;
            }
            var featureInteractor = SelectTool.SelectedFeatureInteractors[0];

            if (SupportedGeometry(featureInteractor.SourceFeature.Geometry))
            {
                if (SnapResult == null)
                {
                    SelectTool.OnMouseDown(worldPosition, e);
                    return;
                }

                var trackerFeature = featureInteractor.GetTrackerAtCoordinate(SnapResult.Location);

                // if user click visible tracker it will be handled as move, otherwise a Trackers will be added.
                if (Mode == EditMode.Add && trackerFeature != null && trackerFeature.Index != -1)
                {
                    MoveTool.OnMouseDown(worldPosition, e);
                    isMoving = true;
                    return;
                }

                featureInteractor.Start();

                if (featureInteractor.EditableObject != null)
                {
                    var featureName = featureInteractor.SourceFeature is INameable
                                          ? ((INameable) featureInteractor.SourceFeature).Name
                                          : "";

                    featureInteractor.EditableObject.BeginEdit(string.Format(ActionName + " {0}", featureName));
                }

                var result = Mode == EditMode.Add
                                  ? featureInteractor.InsertTracker(SnapResult.Location, SnapResult.SnapIndexPrevious + 1)
                                  : featureInteractor.RemoveTracker(trackerFeature);

                featureInteractor.Stop();

                if (featureInteractor.EditableObject != null)
                {
                    featureInteractor.EditableObject.EndEdit();
                }

                SelectTool.Select(featureInteractor.Layer, featureInteractor.SourceFeature);

                if (result)
                {
                    featureInteractor.Layer.RenderRequired = true;

                    MapControl.Refresh();

                    return;
                }
            }

            // if no curve point modification, handle as normal selection
            SelectTool.OnMouseDown(worldPosition, e);
        }


        

        public override void Render(Graphics graphics, Map mapBox)
        {
            MapControl.MoveTool.Render(graphics, mapBox);
        }
    }
}
