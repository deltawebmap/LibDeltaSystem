using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.DynamicTiles
{
    public class TileData
    {
        public float tile_x;
        public float tile_y;
        public float tile_z;

        public float units_per_tile;
        public float tiles_per_axis;

        public float game_min_x;
        public float game_min_y;

        public float game_max_x;
        public float game_max_y;
    }
}
