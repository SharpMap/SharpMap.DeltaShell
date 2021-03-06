// Copyright 2006 - Diego Guidi
//
// This file is part of NtsProvider.
// NtsProvider is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with NtsProvider; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections;
using System.Collections.Generic;
using DelftTools.Utils.Data;
using GeoAPI.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Api;

namespace SharpMap.Data.Providers
{
    /// <summary>
	/// The NtsProvider enables you to feed any SharpMap datasource through the <a href="http://sourceforge.net/projects/nts">NetTopologySuite</a>
	/// geometry using any NTS operation.
    /// </summary>
	/// <remarks>
	/// The following example shows how to apply buffers to a shapefile-based river-dataset:
	/// <code lang="C#">
	/// public void InitializeMap(SharpMap.Map map)
	/// {
	///		//Create Shapefile datasource
	///		SharpMap.Data.Providers.ShapeFile shp = new SharpMap.Data.Providers.ShapeFile("rivers.shp", true);
	///		//Create NTS Datasource that gets its data from 'shp' and calls 'NtsOperation' that defines a geoprocessing method
	///		SharpMap.Data.Providers.NtsProvider nts = new SharpMap.Data.Providers.NtsProvider(shp,new SharpMap.Data.Providers.NtsProvider.GeometryOperationDelegate(NtsOperation));
	///		//Create the layer for rendering
	///		SharpMap.Layers.VectorLayer layRivers = new SharpMap.Layers.VectorLayer("Rivers");
	///		layRivers.DataSource = nts;
	///		layRivers.Style.Fill = Brushes.Blue;
	///		map.Layers.Add(layRivers);
	/// }
	/// //Define geoprocessing delegate that buffers all geometries with a distance of 0.5 mapunits
	/// public static void NtsOperation(List<NetTopologySuite.Features.Feature> geoms)
	/// {
	///		foreach (NetTopologySuite.Features.Feature f in geoms)
	/// 		f.Geometry = f.Geometry.Buffer(0.5);
	/// }
	/// </code>
	/// </remarks>
    public class NtsProvider : Unique<long>, IFeatureProvider
    {

		/// <summary>
		/// Defines a geometry operation that will be applied to all geometries in <see cref="NtsProvider"/>.
		/// </summary>
		/// <param name="features"></param>
		public delegate void GeometryOperationDelegate(IList features);
        
        #region Fields

        // Factory for NTS features
        private NetTopologySuite.Geometries.GeometryFactory geometryFactory = null;

        // NTS features
        private IList features = null;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NtsProvider"/> class
        /// using a default <see cref="NetTopologySuite.Geometries.PrecisionModel"/> 
        /// with Floating precision.
        /// </summary>        
        protected internal NtsProvider() : this(new NetTopologySuite.Geometries.PrecisionModel()) { }

        /// <summary>
		/// Initializes a new instance of the <see cref="NtsProvider"/> class
        /// using the given <paramref name="precisionModel"/>.
        /// </summary>
        /// <param name="precisionModel">
        /// The <see cref="NetTopologySuite.Geometries.PrecisionModel"/>  
        /// to use for define the precision of the geometry operations.
        /// </param>
        /// <seealso cref="PrecisionModels"/>
        /// <seealso cref="NetTopologySuite.Geometries.GeometryFactory"/>
        protected internal NtsProvider(NetTopologySuite.Geometries.PrecisionModel precisionModel)
        {
            geometryFactory = new NetTopologySuite.Geometries.GeometryFactory(precisionModel);
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="NtsProvider"/> class 
        /// from another <see cref="IFeatureProvider" />.
        /// </summary>
        /// <param name="provider">
        /// The base <see cref="IFeatureProvider"/> 
		/// from witch initialize the <see cref="NtsProvider"/> instance.
        /// </param>
        public NtsProvider(IFeatureProvider provider) : this()
        {                        
            BuildFromProvider(provider);            
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="NtsProvider"/> class
        /// from another <see cref="IFeatureProvider" />.
        /// </summary>
        /// <param name="provider">
        /// The base <see cref="IFeatureProvider"/> 
		/// from witch initialize the <see cref="NtsProvider"/> instance.
        /// </param>
        /// <param name="precisionModel">
        /// The <see cref="NetTopologySuite.Geometries.PrecisionModel"/>  
        /// to use for define the precision of the geometry operations.
        /// </param>
        /// <seealso cref="PrecisionModels"/>     
        /// <seealso cref="NetTopologySuite.Geometries.GeometryFactory"/>
        public NtsProvider(IFeatureProvider provider, 
            NetTopologySuite.Geometries.PrecisionModel precisionModel) : this(precisionModel)
        {                       
            BuildFromProvider(provider);            
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="NtsProvider"/> class
        /// from another <see cref="IFeatureProvider" />.
        /// </summary>
        /// <param name="provider">
        /// The base <see cref="IFeatureProvider"/> 
		/// from witch initialize the <see cref="NtsProvider"/> instance.
        /// </param>
        /// <param name="operation">
        /// The <see cref="GeometryOperationDelegate"/> to apply 
        /// to all geometry elements in the <paramref name="provider"/>.
        /// </param>  
        public NtsProvider(IFeatureProvider provider, GeometryOperationDelegate operation) : this(provider)
        {            
            operation(features);
         }

        /// <summary>
		 /// Initializes a new instance of the <see cref="NtsProvider"/> class
        /// from another <see cref="IFeatureProvider" />.
        /// </summary>
        /// <param name="provider">
        /// The base <see cref="IFeatureProvider"/> 
		 /// from witch initialize the <see cref="NtsProvider"/> instance.
        /// </param>
        /// <param name="operation">
        /// The <see cref="GeometryOperationDelegate"/> to apply 
        /// to all geometry elements in the <paramref name="provider"/>.
        /// </param>         
        /// <param name="precisionModel">
        /// The <see cref="NetTopologySuite.Geometries.PrecisionModel"/>  
        /// to use for define the precision of the geometry operations.
        /// </param>
        /// <seealso cref="PrecisionModels"/> 
        /// <seealso cref="NetTopologySuite.Geometries.GeometryFactory"/>
        public NtsProvider(IFeatureProvider provider, GeometryOperationDelegate operation,
            NetTopologySuite.Geometries.PrecisionModel precisionModel) : this(provider, precisionModel)
        {            
            operation(features);         
        }

        /// <summary>
        /// Builds from the given provider.
        /// </summary>
        /// <param name="provider">
        /// The base <see cref="IFeatureProvider"/> 
		/// from witch initialize the <see cref="NtsProvider"/> instance.
        /// </param>
        private void BuildFromProvider(IFeatureProvider provider)
        {            
            // Load all features from the given provider
            features = new ArrayList();
            foreach (var feature in provider.Features)
            {
                features.Add(feature);
            }
        }

        #endregion

        #region IFeatureProvider Members
		
        /// <summary>
        /// Gets the connection ID.
        /// </summary>
        /// <value>The connection ID.</value>
        [Obsolete("Does nothing at all")]
        public int IndexOf(IFeature feature)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Returns the BoundingBox of the dataset.
        /// </summary>
        /// <returns>BoundingBox</returns>
        public Envelope GetExtents()
        {            
            if (features == null || features.Count == 0)
            {
                return null;
            }
            
            Envelope envelope = new Envelope();
            foreach (IFeature feature in features)
                envelope.ExpandToInclude(feature.Geometry.EnvelopeInternal);
            return envelope;
        }

        /// <summary>
        /// Gets the feature identified from the given <paramref name="rowID" />.
        /// </summary>
        /// <param name="rowID">The row ID.</param>
        /// <returns></returns>
        public IFeature GetFeature(int rowID)
        {
            return (IFeature) features[rowID];
        }

        public bool Contains(IFeature feature)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Returns the number of features in the dataset.
        /// </summary>
        /// <returns>number of features</returns>
        public int GetFeatureCount()
        {
            return features.Count;
        }
        
        public Type FeatureType
        {
            get { return typeof(FeatureDataRow); }
        }

        public IList Features
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public bool IsReadOnly { get { return true; } }

        public IFeature Add(IGeometry geometry)
        {
            throw new System.NotImplementedException();
        }

        public Func<IFeatureProvider, IGeometry, IFeature> AddNewFeatureFromGeometryDelegate { get; set; }
        public event EventHandler FeaturesChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="box"></param>
        public IEnumerable<IFeature> GetFeatures(Envelope box)
		{
			// Identifies all the features within the given BoundingBox
			var results = new List<IFeature>(features.Count);
			foreach (IFeature feature in features)
				if (box.Intersects(feature.Geometry.EnvelopeInternal))
					results.Add(feature);

            return results;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        public IEnumerable<IFeature> GetFeatures(IGeometry geom)
		{
            var result = new List<IFeature>();
			foreach (IFeature feature in features)
				if (feature.Geometry.Intersects(geom))
					result.Add(feature);

            return result;
		}

        /// <summary>
        /// Gets the geometry by ID.
        /// </summary>
        /// <param name="oid">The oid.</param>
        /// <returns></returns>
        public IGeometry GetGeometryByID(int oid)
        {
            return ((IFeature)features[oid]).Geometry;
        }

        private string srsWkt;

		/// <summary>
		/// The spatial reference ID (CRS)
		/// </summary>
		public string SrsWkt
		{
			get { return srsWkt; }
			set { srsWkt = value; }
		}

        public Envelope GetBounds(int recordIndex)
        {
            return GetFeature(recordIndex).Geometry.EnvelopeInternal;
        }

        public ICoordinateSystem CoordinateSystem { get; set; }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() { }

        #endregion

	}
}
