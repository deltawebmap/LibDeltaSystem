using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.System.Entities
{
    public class ServerSettingsMetadata : Attribute
    {
        public string section;
        public string name;

        public ServerSettingsMetadata(string name, string section)
        {
            this.name = name;
            this.section = section;
        }
    }
}
