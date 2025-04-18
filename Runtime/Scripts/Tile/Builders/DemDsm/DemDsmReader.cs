
using LasUtility.VoxelGrid;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace Kuoste.LidarWorld.Tile
{
    public class DemDsmReader : Builder, IDemDsmBuilder
    {
        public VoxelGrid Build(Tile tile)
        {
            if (CancellationToken.IsCancellationRequested)
                return new VoxelGrid();

            string sFullFilename = Path.Combine(tile.Common.DirectoryIntermediate, IDemDsmBuilder.Filename(tile.Name, tile.Common.Version));

            VoxelGrid grid = VoxelGrid.Deserialize(sFullFilename);

            return grid;
        }
    }
}
