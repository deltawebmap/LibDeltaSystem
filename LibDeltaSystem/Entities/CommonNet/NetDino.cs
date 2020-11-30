using LibDeltaSystem.Db.Content;
using LibDeltaSystem.Db.System.Entities;
using LibDeltaSystem.Tools.DeltaWebFormat.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.CommonNet
{
    public class NetDino
    {
        public int tribe_id { get; set; }
        public string dino_id { get; set; }
        public bool is_female { get; set; }
        public int[] colors { get; set; }
        public string[] colors_hex { get; set; }
        public string tamed_name { get; set; }
        [WebFormatAttributeUseNameTable]
        public string tamer_name { get; set; }
        [WebFormatAttributeUseNameTable]
        public string classname { get; set; }
        public float[] current_stats { get; set; }
        public float[] max_stats { get; set; }
        public int[] base_levelups_applied { get; set; }
        public int[] tamed_levelups_applied { get; set; }
        public int base_level { get; set; }
        public int level { get; set; }
        public float experience { get; set; }
        public bool is_baby { get; set; }
        public float baby_age { get; set; }
        public double next_imprint_time { get; set; }
        public float imprint_quality { get; set; }
        [WebFormatAttributeUseObject]
        public DbLocation location { get; set; }
        [WebFormatAttributeUseNameTable]
        public string status { get; set; }
        public float taming_effectiveness { get; set; }
        public bool is_cryo { get; set; }
        public float experience_points { get; set; }
        public DateTime last_sync_time { get; set; }
        public bool is_alive { get; set; }
        [WebFormatAttributeUseObject]
        public SavedDinoTribePrefs tribe_prefs { get; set; }
        public int commit_type { get; set; }
        public string commit_id { get; set; }

        public static NetDino ConvertDbDino(DbDino dino)
        {
            return new NetDino
            {
                tribe_id = dino.tribe_id,
                dino_id = dino.dino_id.ToString(),
                is_female = dino.is_female,
                colors = new int[6],
                colors_hex = new string[6],
                tamed_name = dino.tamed_name,
                tamer_name = dino.tamer_name,
                classname = dino.classname,
                current_stats = dino.current_stats,
                max_stats = dino.max_stats,
                base_levelups_applied = dino.base_levelups_applied,
                tamed_levelups_applied = dino.tamed_levelups_applied,
                base_level = dino.base_level,
                level = dino.level,
                experience = dino.experience,
                is_baby = dino.is_baby,
                baby_age = dino.baby_age,
                next_imprint_time = dino.next_imprint_time,
                imprint_quality = dino.imprint_quality,
                location = dino.location,
                status = dino.status,
                taming_effectiveness = dino.taming_effectiveness,
                is_cryo = dino.is_cryo,
                experience_points = dino.experience_points,
                last_sync_time = dino.last_sync_time,
                is_alive = dino.is_alive,
                tribe_prefs = dino.prefs
            };
        }
    }
}
