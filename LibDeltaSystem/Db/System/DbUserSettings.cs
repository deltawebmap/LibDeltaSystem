using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.System
{
    public class DbUserSettings
    {
        public List<string> custom_vulgar_words { get; set; } = new List<string>();
        public bool vulgar_filter_on { get; set; } = true;
        public bool vulgar_show_censored_on { get; set; } = false; //If this is on, blocked names will still show, but censored.

        public int oobe_status { get; set; } //0: OOBE not started, 1: OOBE 
    }
}
