using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.System.Entities
{
    public class SavedUserServerPrefs
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public int map { get; set; }
        public string drawable_map { get; set; }

        public string canvas_id { get; set; }
        public int canvas_brush_color { get; set; } = 5;
    }
}
