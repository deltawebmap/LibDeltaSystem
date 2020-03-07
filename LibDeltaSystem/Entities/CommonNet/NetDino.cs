using LibDeltaSystem.Db.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.CommonNet
{
    public class NetDino
    {
        public int tribe_id;
        public string dino_id;
        public bool is_female;
        public int[] colors;
        public string[] colors_hex;
        public string tamed_name;
        public string tamer_name;
        public string classname;
        public float[] current_stats;
        public float[] max_stats;
        public int[] base_levelups_applied;
        public int[] tamed_levelups_applied;
        public int base_level;
        public int level;
        public float experience;
        public bool is_baby;
        public float baby_age;
        public double next_imprint_time;
        public float imprint_quality;
        public DbLocation location;
        public string status;
        public float taming_effectiveness;
        public bool is_cryo;
        public float experience_points;

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
                experience_points = dino.experience_points
            };
        }
    }
}
