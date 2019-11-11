using LibDeltaSystem.Entities.DynamicTiles;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.System.Entities
{
    public class DynamicTileTarget
    {
        public int x;
        public int y;
        public int z;
        public string server_id;
        public string map_name; //In game name
        public int tribe_id;
        public DynamicTileType map_id;

        public bool Compare(DynamicTileTarget a)
        {
            return a.x == x && a.y == y && a.z == z && a.server_id.Equals(a.server_id) && tribe_id == a.tribe_id && map_id == a.map_id;
        }

        public FilterDefinition<DbDynamicTileCache> CreateFilter()
        {
            var filterBuilder = Builders<DbDynamicTileCache>.Filter;
            return filterBuilder.Eq("target.x", x) & filterBuilder.Eq("target.y", y) & filterBuilder.Eq("target.z", z) &
                filterBuilder.Eq("target.server_id", server_id) & filterBuilder.Eq("target.tribe_id", tribe_id) &
                filterBuilder.Eq("target.map_name", map_name) & filterBuilder.Eq("target.map_id", map_id);
        }

        public TileData GetTileData(float captureSize)
        {
            TileData d = new TileData
            {
                tile_x = x,
                tile_y = y,
                tile_z = z
            };

            //Get units per tile
            d.tiles_per_axis = MathF.Pow(2, z);
            d.units_per_tile = captureSize / d.tiles_per_axis;

            //Calculate game pos
            CalculateZCoordsToGameUnits(captureSize, d.units_per_tile, x, y, out d.game_min_x, out d.game_min_y);
            CalculateZCoordsToGameUnits(captureSize, d.units_per_tile, x + 1, y + 1, out d.game_max_x, out d.game_max_y);

            return d;
        }

        private static void CalculateZCoordsToGameUnits(float captureSize, float units_per_tile, float x, float y, out float gx, out float gy)
        {
            float offset = captureSize / 2; //Because this is based in the upper left, while the game is based in the middle
            gx = (x * units_per_tile) - offset;
            gy = (y * units_per_tile) - offset;
        }
    }
}
