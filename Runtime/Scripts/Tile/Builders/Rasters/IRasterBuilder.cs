using LasUtility.Common;
using System.Collections.Generic;

namespace Kuoste.LidarWorld.Tile
{
    public interface IRasterBuilder
    {
        public static string Filename(string sTileName, string sSpecifier, string sVersion) => sTileName + "_" + sSpecifier + "_v" + sVersion + ".asp";

        public static string SpecifierTerrainType = "terraintype";
        public static string SpecifierBuildingsRoads = "buildingsroads";

        public void SetRasterSpecifier(string sSpecifier);
        public void SetShpFilenames(string[] inputFilenames);
        public void SetRasterizedClassesWithRasterValues(Dictionary<int, byte> classesToRasterValues);

        public IRaster Build(Tile tile);
    }
}