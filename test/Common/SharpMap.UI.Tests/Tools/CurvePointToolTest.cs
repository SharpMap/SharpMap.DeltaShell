using System.Windows.Forms;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries.Utilities;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Styles;
using SharpMap.UI.Forms;
using SharpMap.UI.Tools;

namespace SharpMap.UI.Tests.Tools
{
    public class CloneableFeature: Feature
    {
        public override object Clone()
        {
            return new CloneableFeature {Attributes = Attributes, Geometry = (IGeometry) Geometry.Clone()};
        }
    }

    [TestFixture]
    public class CurvePointToolTest
    {
        [Test]
        public void CanAddPointToLine()
        {
            var mapControl = new MapControl();

            var vectorLayer = new VectorLayer();
            var layerData = new FeatureCollection();
            vectorLayer.DataSource = layerData;
            layerData.FeatureType = typeof (CloneableFeature);

            layerData.Add(new LineString(new[] {new Coordinate(0, 0), new Coordinate(100, 0)}));

            mapControl.Map.Layers.Add(vectorLayer);

            var firstFeature = (IFeature) layerData.Features[0];
            mapControl.SelectTool.Select(firstFeature);

            var curveTool = mapControl.GetToolByType<CurvePointTool>();

            curveTool.IsActive = true;
            var args = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
            curveTool.OnMouseMove(new Coordinate(50, 0), new MouseEventArgs(MouseButtons.None, 1, 0, 0, 0));
            curveTool.OnMouseDown(new Coordinate(50, 0), args);
            curveTool.OnMouseUp(new Coordinate(50, 0), args);

            Assert.AreEqual(3, firstFeature.Geometry.Coordinates.Length);
            Assert.AreEqual(50.0, firstFeature.Geometry.Coordinates[1].X);
        }

        [Test] //not working in UI?
        public void CanRemovePointFromLine()
        {
            var mapControl = new MapControl();

            var vectorLayer = new VectorLayer();
            var layerData = new FeatureCollection();
            vectorLayer.DataSource = layerData;
            layerData.FeatureType = typeof(CloneableFeature);

            layerData.Add(new LineString(new[] { new Coordinate(0, 0), new Coordinate(50, 0), new Coordinate(100, 0) }));

            mapControl.Map.Layers.Add(vectorLayer);

            var firstFeature = (IFeature)layerData.Features[0];
            mapControl.SelectTool.Select(firstFeature);

            var curveTool = mapControl.GetToolByType<CurvePointTool>();

            curveTool.IsActive = true;
            curveTool.Mode = CurvePointTool.EditMode.Remove;
            var args = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
            curveTool.OnMouseMove(new Coordinate(50, 0), new MouseEventArgs(MouseButtons.None, 1, 0, 0, 0));
            curveTool.OnMouseDown(new Coordinate(50, 0), args); // delete tracker

            Assert.AreEqual(2, firstFeature.Geometry.Coordinates.Length);
            Assert.AreEqual(100.0, firstFeature.Geometry.Coordinates[1].X);
        }

        [Test]
        public void CanAddPointToPolygon()
        {
            var mapControl = new MapControl();

            var vectorLayer = new VectorLayer();
            var layerData = new FeatureCollection();
            vectorLayer.DataSource = layerData;
            layerData.FeatureType = typeof(CloneableFeature);

            layerData.Add(new Polygon(new LinearRing(
                                           new[]
                                               {
                                                   new Coordinate(0, 0),
                                                   new Coordinate(100, 0),
                                                   new Coordinate(100, 100),
                                                   new Coordinate(0, 100),
                                                   new Coordinate(0, 0),
                                               })));

            mapControl.Map.Layers.Add(vectorLayer);

            var firstFeature = (IFeature)layerData.Features[0];
            mapControl.SelectTool.Select(firstFeature);

            var curveTool = mapControl.GetToolByType<CurvePointTool>();

            curveTool.IsActive = true;
            var args = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
            curveTool.OnMouseMove(new Coordinate(50, 0), new MouseEventArgs(MouseButtons.None, 1, 0, 0, 0));
            curveTool.OnMouseDown(new Coordinate(50, 0), args);
            curveTool.OnMouseUp(new Coordinate(50, 0), args);

            Assert.AreEqual(6, firstFeature.Geometry.Coordinates.Length);
            Assert.AreEqual(50.0, firstFeature.Geometry.Coordinates[1].X);
        }

        [Test] //not working in UI?
        public void CanRemovePointFromPolygon()
        {
            var mapControl = new MapControl();

            var vectorLayer = new VectorLayer();
            var layerData = new FeatureCollection();
            vectorLayer.DataSource = layerData;
            layerData.FeatureType = typeof(CloneableFeature);

            layerData.Add(new Polygon(new LinearRing(
                               new[]
                                               {
                                                   new Coordinate(0, 0),
                                                   new Coordinate(100, 0),
                                                   new Coordinate(100, 100),
                                                   new Coordinate(0, 100),
                                                   new Coordinate(0, 0),
                                               })));

            mapControl.Map.Layers.Add(vectorLayer);

            var firstFeature = (IFeature)layerData.Features[0];
            mapControl.SelectTool.Select(firstFeature);

            var curveTool = mapControl.GetToolByType<CurvePointTool>();

            curveTool.IsActive = true;
            curveTool.Mode = CurvePointTool.EditMode.Remove;
            var args = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
            curveTool.OnMouseMove(new Coordinate(100, 0), new MouseEventArgs(MouseButtons.None, 1, 0, 0, 0));
            curveTool.OnMouseDown(new Coordinate(100, 0), args); // delete tracker
            
            Assert.AreEqual(4, firstFeature.Geometry.Coordinates.Length);
            Assert.AreEqual(100.0, firstFeature.Geometry.Coordinates[1].X);
            Assert.AreEqual(100.0, firstFeature.Geometry.Coordinates[1].Y);
        }

        [Test] //not working in UI?
        public void CanRemoveStartPointFromPolygon()
        {
            var mapControl = new MapControl();

            var vectorLayer = new VectorLayer();
            var layerData = new FeatureCollection();
            vectorLayer.DataSource = layerData;
            layerData.FeatureType = typeof(CloneableFeature);

            layerData.Add(new Polygon(new LinearRing(
                               new[]
                                               {
                                                   new Coordinate(0, 0),
                                                   new Coordinate(100, 0),
                                                   new Coordinate(100, 100),
                                                   new Coordinate(0, 100),
                                                   new Coordinate(0, 0),
                                               })));

            mapControl.Map.Layers.Add(vectorLayer);

            var firstFeature = (IFeature)layerData.Features[0];
            mapControl.SelectTool.Select(firstFeature);

            var curveTool = mapControl.GetToolByType<CurvePointTool>();

            curveTool.IsActive = true;
            curveTool.Mode = CurvePointTool.EditMode.Remove;
            var args = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
            curveTool.OnMouseMove(new Coordinate(0, 0), new MouseEventArgs(MouseButtons.None, 1, 0, 0, 0));
            curveTool.OnMouseDown(new Coordinate(0, 0), args); // delete tracker
            
            Assert.AreEqual(4, firstFeature.Geometry.Coordinates.Length);
            Assert.AreEqual(100.0, firstFeature.Geometry.Coordinates[1].X);
            Assert.AreEqual(100.0, firstFeature.Geometry.Coordinates[1].Y);
        }
    }

    [TestFixture]
    public class CurvePointToolWithTransformationTest
    {
        private readonly AffineCoordinateTransformation _amt = new AffineCoordinateTransformation(
            new AffineTransformation(1, 0, 0, 0, 1, 1000));

        [Test]
        public void CanAddPointToLine()
        {
            var mapControl = new MapControl {AllowDrop = false};

            var vectorLayer = new VectorLayer();
            var layerData = new FeatureCollection();
            vectorLayer.DataSource = layerData;
            layerData.FeatureType = typeof(CloneableFeature);

            layerData.Add(new LineString(new[] { new Coordinate(0, 0), new Coordinate(100, 0) }));

            mapControl.Map.Layers.Add(vectorLayer);
            vectorLayer.CoordinateTransformation = _amt;

            var firstFeature = (IFeature)layerData.Features[0];
            mapControl.SelectTool.Select(firstFeature);

            var curveTool = mapControl.GetToolByType<CurvePointTool>();

            curveTool.IsActive = true;
            var args = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
            curveTool.OnMouseMove(new Coordinate(50, 1000), new MouseEventArgs(MouseButtons.None, 1, 0, 0, 0));
            curveTool.OnMouseDown(new Coordinate(50, 1000), args);
            curveTool.OnMouseUp(new Coordinate(50, 1000), args);

            Assert.AreEqual(3, firstFeature.Geometry.Coordinates.Length);
            Assert.AreEqual(50.0, firstFeature.Geometry.Coordinates[1].X);
            Assert.AreEqual(0, firstFeature.Geometry.Coordinates[1].Y);
        }

        [Test]
        public void CanMovePointOfLine()
        {
            var mapControl = new MapControl { AllowDrop = false };

            var vectorLayer = new VectorLayer();
            var layerData = new FeatureCollection();
            vectorLayer.DataSource = layerData;
            layerData.FeatureType = typeof(CloneableFeature);

            layerData.Add(new LineString(new[] { new Coordinate(0, 0), new Coordinate(50, 0), new Coordinate(100, 0) }));

            mapControl.Map.Layers.Add(vectorLayer);
            vectorLayer.CoordinateTransformation = _amt;

            var firstFeature = (IFeature)layerData.Features[0];
            mapControl.SelectTool.Select(firstFeature);
            //((LineStringFeatureInteractor)mapControl.SelectTool.GetFeatureInteractor(vectorLayer, firstFeature)).MoveVerticesAllowed = true;

            var curveTool = mapControl.GetToolByType<CurvePointTool>();

            curveTool.IsActive = true;
            var args = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
            curveTool.OnMouseMove(new Coordinate(50.1, 999.9), new MouseEventArgs(MouseButtons.None, 1, 0, 0, 0));
            curveTool.OnMouseDown(new Coordinate(50.1, 999.9), args);
            curveTool.OnMouseMove(new Coordinate(60, 1010), new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
            curveTool.OnMouseUp(new Coordinate(60, 1010), args);

            Assert.AreEqual(3, firstFeature.Geometry.Coordinates.Length);
            Assert.AreEqual(60.0, firstFeature.Geometry.Coordinates[1].X);
            Assert.AreEqual(10, firstFeature.Geometry.Coordinates[1].Y);
        }

        [Test] //not working in UI?
        public void CanRemovePointFromLine()
        {
            var mapControl = new MapControl { AllowDrop = false };

            var vectorLayer = new VectorLayer();
            var layerData = new FeatureCollection();
            vectorLayer.DataSource = layerData;
            layerData.FeatureType = typeof(CloneableFeature);

            layerData.Add(new LineString(new[] { new Coordinate(0, 0), new Coordinate(50, 0), new Coordinate(100, 0) }));

            mapControl.Map.Layers.Add(vectorLayer);
            vectorLayer.CoordinateTransformation = _amt;

            var firstFeature = (IFeature)layerData.Features[0];
            mapControl.SelectTool.Select(firstFeature);

            var curveTool = mapControl.GetToolByType<CurvePointTool>();

            curveTool.IsActive = true;
            curveTool.Mode = CurvePointTool.EditMode.Remove;
            var args = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
            curveTool.OnMouseMove(new Coordinate(50, 1000), new MouseEventArgs(MouseButtons.None, 1, 0, 0, 0));
            curveTool.OnMouseDown(new Coordinate(50, 1000), args); // delete tracker

            Assert.AreEqual(2, firstFeature.Geometry.Coordinates.Length);
            Assert.AreEqual(100.0, firstFeature.Geometry.Coordinates[1].X);
        }

        [Test]
        public void CanAddPointToPolygon()
        {
            var mapControl = new MapControl { AllowDrop = false };

            var vectorLayer = new VectorLayer();
            var layerData = new FeatureCollection();
            vectorLayer.DataSource = layerData;
            layerData.FeatureType = typeof(CloneableFeature);

            layerData.Add(new Polygon(new LinearRing(
                                           new[]
                                               {
                                                   new Coordinate(0, 0),
                                                   new Coordinate(100, 0),
                                                   new Coordinate(100, 100),
                                                   new Coordinate(0, 100),
                                                   new Coordinate(0, 0),
                                               })));

            mapControl.Map.Layers.Add(vectorLayer);
            vectorLayer.CoordinateTransformation = _amt;

            var firstFeature = (IFeature)layerData.Features[0];
            mapControl.SelectTool.Select(firstFeature);

            var curveTool = mapControl.GetToolByType<CurvePointTool>();

            curveTool.IsActive = true;
            var args = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
            curveTool.OnMouseMove(new Coordinate(50, 1000), new MouseEventArgs(MouseButtons.None, 1, 0, 0, 0));
            curveTool.OnMouseDown(new Coordinate(50, 1000), args);
            curveTool.OnMouseUp(new Coordinate(50, 1000), args);

            Assert.AreEqual(6, firstFeature.Geometry.Coordinates.Length);
            Assert.AreEqual(50.0, firstFeature.Geometry.Coordinates[1].X);
        }

        [Test] //not working in UI?
        public void CanRemovePointFromPolygon()
        {
            var mapControl = new MapControl {AllowDrop = false};

            var vectorLayer = new VectorLayer();
            var layerData = new FeatureCollection();
            vectorLayer.DataSource = layerData;
            layerData.FeatureType = typeof(CloneableFeature);

            layerData.Add(new Polygon(new LinearRing(
                               new[]
                                               {
                                                   new Coordinate(0, 0),
                                                   new Coordinate(100, 0),
                                                   new Coordinate(100, 100),
                                                   new Coordinate(0, 100),
                                                   new Coordinate(0, 0),
                                               })));

            mapControl.Map.Layers.Add(vectorLayer);
            vectorLayer.CoordinateTransformation = _amt;

            var firstFeature = (IFeature)layerData.Features[0];
            mapControl.SelectTool.Select(firstFeature);

            var curveTool = mapControl.GetToolByType<CurvePointTool>();

            curveTool.IsActive = true;
            curveTool.Mode = CurvePointTool.EditMode.Remove;
            var args = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
            curveTool.OnMouseMove(new Coordinate(100, 1000), new MouseEventArgs(MouseButtons.None, 1, 0, 0, 0));
            curveTool.OnMouseDown(new Coordinate(100, 1000), args); // delete tracker

            Assert.AreEqual(4, firstFeature.Geometry.Coordinates.Length);
            Assert.AreEqual(100.0, firstFeature.Geometry.Coordinates[1].X);
            Assert.AreEqual(100.0, firstFeature.Geometry.Coordinates[1].Y);
        }

        [Test] //not working in UI?
        public void CanRemoveStartPointFromPolygon()
        {
            var mapControl = new MapControl {AllowDrop = false};

            var vectorLayer = new VectorLayer();
            var layerData = new FeatureCollection();
            vectorLayer.DataSource = layerData;
            layerData.FeatureType = typeof(CloneableFeature);

            layerData.Add(new Polygon(new LinearRing(
                               new[]
                                               {
                                                   new Coordinate(0, 0),
                                                   new Coordinate(100, 0),
                                                   new Coordinate(100, 100),
                                                   new Coordinate(0, 100),
                                                   new Coordinate(0, 0),
                                               })));

            mapControl.Map.Layers.Add(vectorLayer);
            vectorLayer.CoordinateTransformation = _amt;

            var firstFeature = (IFeature)layerData.Features[0];
            mapControl.SelectTool.Select(firstFeature);

            var curveTool = mapControl.GetToolByType<CurvePointTool>();

            curveTool.IsActive = true;
            curveTool.Mode = CurvePointTool.EditMode.Remove;
            var args = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
            curveTool.OnMouseMove(new Coordinate(0, 1000), new MouseEventArgs(MouseButtons.None, 1, 0, 0, 0));
            curveTool.OnMouseDown(new Coordinate(0, 1000), args); // delete tracker

            Assert.AreEqual(4, firstFeature.Geometry.Coordinates.Length);
            Assert.AreEqual(100.0, firstFeature.Geometry.Coordinates[1].X);
            Assert.AreEqual(100.0, firstFeature.Geometry.Coordinates[1].Y);
        }
    }

}
