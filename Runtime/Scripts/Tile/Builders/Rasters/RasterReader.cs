using Kuoste.LidarWorld.Tile;
using LasUtility.Common;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Kuoste.LidarWorld.Tile
{
    public class RasterReader : IRasterBuilder
    {
        string _sRasterFilenameSpecifier;

        public void SetRasterizedClassesWithRasterValues(Dictionary<int, byte> classesToRasterValues)
        {
            throw new System.NotImplementedException();
        }

        public IRaster Build(Tile tile)
        {
            if (tile.Token.IsCancellationRequested)
                return new ByteRaster();

            string sFullFilename = Path.Combine(tile.DirectoryIntermediate, IRasterBuilder.Filename(tile.Name, _sRasterFilenameSpecifier, tile.Version));

            return ByteRaster.CreateFromAscii(sFullFilename);
        }

        public void SetShpFilenames(string[] inputFilenames)
        {
            throw new System.NotImplementedException();
        }

        public void SetRasterSpecifier(string sSpecifier)
        {
            _sRasterFilenameSpecifier = sSpecifier;
        }

        public void RemoveRasterizedClassesWithRasterValues()
        {
            throw new System.NotImplementedException();
        }
    }
}