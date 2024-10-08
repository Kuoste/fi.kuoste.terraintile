using LasUtility.Nls;
using LasUtility.ShapefileRasteriser;
using NetTopologySuite.Geometries;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Kuoste.LidarWorld.Tile
{
    public class TileRasterService : TileService, ITileBuilderService
    {
        private readonly IRasterBuilder _reader, _creator;

        private readonly Dictionary<int, byte> _buildingRoadClassesToRasterValues = new();
        private readonly Dictionary<int, byte> _terrainTypeClassesToRasterValues = new();

        public TileRasterService(IRasterBuilder reader, IRasterBuilder creator, CancellationToken token)
        {
            _reader = reader;
            _creator = creator;

            _reader.SetCancellationToken(token);
            _creator.SetCancellationToken(token);


            // Combine raster values for buildings and roads

            foreach (var mapper in TopographicDb.RoadLineClassesToRasterValues)
                _buildingRoadClassesToRasterValues[mapper.Key] = mapper.Value;

            foreach (var mapper in TopographicDb.BuildingPolygonClassesToRasterValues)
                _buildingRoadClassesToRasterValues[mapper.Key] = mapper.Value;


            // Combine raster values for terrrain features

            foreach (var mapper in TopographicDb.WaterPolygonClassesToRasterValues)
                _terrainTypeClassesToRasterValues[mapper.Key] = mapper.Value;

            foreach (var mapper in TopographicDb.SwampPolygonClassesToRasterValues)
                _terrainTypeClassesToRasterValues[mapper.Key] = mapper.Value;

            foreach (var mapper in TopographicDb.RockPolygonClassesToRasterValues)
                _terrainTypeClassesToRasterValues[mapper.Key] = mapper.Value;

            foreach (var mapper in TopographicDb.SandPolygonClassesToRasterValues)
                _terrainTypeClassesToRasterValues[mapper.Key] = mapper.Value;

            foreach (var mapper in TopographicDb.FieldPolygonClassesToRasterValues)
                _terrainTypeClassesToRasterValues[mapper.Key] = mapper.Value;

            foreach (var mapper in TopographicDb.RockLineClassesToRasterValues)
                _terrainTypeClassesToRasterValues[mapper.Key] = mapper.Value;

            _token = token;
        }

        public void BuilderThread()
        {
            while (true)
            {
                if (_token.IsCancellationRequested)
                    return;

                if (_tileQueue.Count > 0 && _tileQueue.TryDequeue(out Tile tile))
                {
                    //Stopwatch sw = Stopwatch.StartNew();

                    TileNamer.Decode(tile.Name, out Envelope bounds);
                    string s12km12kmMapTileName = TileNamer.Encode((int)bounds.MinX, (int)bounds.MinY, TopographicDb.iMapTileEdgeLengthInMeters);


                    // Process terrain type raster

                    string sFilename = IRasterBuilder.Filename(tile.Name, IRasterBuilder.SpecifierTerrainType, tile.Common.Version);
                    string sFullFilename = Path.Combine(tile.Common.DirectoryIntermediate, sFilename);

                    if (File.Exists(sFullFilename))
                    {
                        // Load raster from filesystem
                        _reader.SetRasterSpecifier(IRasterBuilder.SpecifierTerrainType);
                        tile.TerrainType = _reader.Build(tile);
                    }
                    else
                    {
                        // Create raster from shapefiles
                        _creator.SetRasterSpecifier(IRasterBuilder.SpecifierTerrainType);
                        _creator.SetRasterizedClassesWithRasterValues(_terrainTypeClassesToRasterValues);
                        _creator.SetShpFilenames(new string[] { TopographicDb.sPrefixForTerrainType + s12km12kmMapTileName + TopographicDb.sPostfixForPolygon + ".shp" });
                        tile.TerrainType = _creator.Build(tile);
                    }


                    // Process buildings & roads raster

                    sFilename = IRasterBuilder.Filename(tile.Name, IRasterBuilder.SpecifierBuildingsRoads, tile.Common.Version);
                    sFullFilename = Path.Combine(tile.Common.DirectoryIntermediate, sFilename);

                    if (File.Exists(sFullFilename))
                    {
                        // Load raster from filesystem
                        _reader.SetRasterSpecifier(IRasterBuilder.SpecifierBuildingsRoads);
                        tile.BuildingsRoads = _reader.Build(tile);
                    }
                    else
                    {
                        // Create raster from shapefiles
                        _creator.SetRasterSpecifier(IRasterBuilder.SpecifierBuildingsRoads);
                        _creator.SetRasterizedClassesWithRasterValues(_buildingRoadClassesToRasterValues);
                        _creator.SetShpFilenames(new string[]
                        {
                            TopographicDb.sPrefixForRoads + s12km12kmMapTileName + TopographicDb.sPostfixForLine + ".shp",
                            TopographicDb.sPrefixForBuildings + s12km12kmMapTileName + TopographicDb.sPostfixForPolygon + ".shp"
                        });
                        tile.BuildingsRoads = _creator.Build(tile);
                    }

                    Interlocked.Increment(ref tile.CompletedCount);

                    //sw.Stop();
                    //Debug.Log($"Tile {tile.Name} rasters built in {sw.ElapsedMilliseconds} ms.");

                    Thread.Sleep(10);
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
