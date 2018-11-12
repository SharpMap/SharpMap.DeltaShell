using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using BruTile.Web;
using DelftTools.Utils;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using log4net;
using SharpMap.Api;
using SharpMap.Layers;

namespace SharpMap.Extensions.Layers
{
    public class BingLayer : Layer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BingLayer)); 
        private ITileSource _tileSource;

        public static string CacheLocation
        {
            get
            {
                var path = SettingsHelper.GetApplicationLocalUserSettingsDirectory();
                return Path.Combine(path, "cache_bing");    
            }
        }

        private static ITileCache<byte[]> cache;

        public BingLayer()
        {
            if (cache == null)
            {
                //no cache so mem
                if (CacheLocation == null)
                {
                    cache = new MemoryCache<byte[]>(1000, 100000);
                }
                else
                {
                    cache = new FileCache(CacheLocation, "png");
                }
            }
        }
        
        public override Envelope Envelope
        {
            get { return new Envelope(Schema.Extent.MinX, Schema.Extent.MaxX, Schema.Extent.MinY, Schema.Extent.MaxY); }
        }

        public string BingApiKey { get; set; } = string.Empty;

        private ITileSource TileSource
        {
            get
            {
                return _tileSource ?? (_tileSource = KnownTileSources.Create(KnownTileSource.BingHybrid, BingApiKey));
            }
        }

        private ITileSchema Schema
        {
            get { return TileSource.Schema; }
        }

        /// <summary>
        /// Renders the layer
        /// </summary>
        /// <param name="g">Graphics object reference</param>
        /// <param name="map">Map which is rendered</param>
        public override void OnRender(System.Drawing.Graphics g, IMap map)
        {
            MapTransform mapTransform = new MapTransform(new PointF((float)Map.Center.X, (float)Map.Center.Y), (float)Map.PixelSize, Map.Image.Width, Map.Image.Height);

            _tileSource = KnownTileSources.Create(KnownTileSource.BingHybrid, BingApiKey);
            string level = BruTile.Utilities.GetNearestLevel(_tileSource.Schema.Resolutions, Map.PixelSize);
            
            IEnumerable<TileInfo> tileInfos = Schema.GetTileInfos(mapTransform.Extent, level);

            //new BingRequest(BingRequest.UrlBing, string.Empty, BingMapType.Hybrid);

            using (var graphics = Graphics.FromImage(Image))
            {
                // log.DebugFormat("Downloading tiles:");
                foreach (var tileInfo in tileInfos)
                {
                    var bytes = cache.Find(tileInfo.Index);

                    if (bytes == default(byte[]))
                    {
                        try
                        {
                            // log.DebugFormat("row: {0}, column: {1}, level: {2}", tileInfo.Index.Row, tileInfo.Index.Col, tileInfo.Index.Level);
                            bytes = _tileSource.GetTile(tileInfo);
                            cache.Add(tileInfo.Index, bytes);
                        }
                        catch (WebException e)
                        {
                            log.Error("Can't fetch tiles from the server", e);
                        }
                    }
                    else
                    {
                        //log.DebugFormat("row: {0}, column: {1}, level: {2} (cached)", tileInfo.Index.Row, tileInfo.Index.Col, tileInfo.Index.Level);
                    }

                    if (bytes == null) continue;
                    using (var bitmap = new Bitmap(new MemoryStream(bytes)))
                    {
                        var rectangle = mapTransform.WorldToMap(tileInfo.Extent.MinX, tileInfo.Extent.MinY,
                            tileInfo.Extent.MaxX, tileInfo.Extent.MaxY);
                        DrawTile(Schema, level, graphics, bitmap, rectangle);
                    }
                }
            }
        }

        private static RectangleF DrawTile(ITileSchema schema, string levelId, Graphics graphics, Bitmap bitmap, RectangleF extent)
        {
            // For drawing on WinForms there are two things to take into account 
            // to prevent seams between tiles.
            using (var imageAttributes = new ImageAttributes())
            {
                // 1) The WrapMode should be set to TileFlipXY. This is related 
                //    to how pixels are rounded by GDI+
                imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
                // 2) The rectangle should be rounded to actual pixels. 
                Rectangle roundedExtent = RoundToPixel(extent);
                graphics.DrawImage(bitmap, roundedExtent, 0, 0, schema.GetTileWidth(levelId), schema.GetTileHeight(levelId), GraphicsUnit.Pixel, imageAttributes);
            }
            return extent;
        }

        private static Rectangle RoundToPixel(RectangleF dest)
        {
            // To get seamless aligning you need to round the locations
            // not the width and height
            return new Rectangle(
                (int)Math.Round(dest.Left),
                (int)Math.Round(dest.Top),
                (int)(Math.Round(dest.Right) - Math.Round(dest.Left)),
                (int)(Math.Round(dest.Bottom) - Math.Round(dest.Top)));
        }
    }
}
