using LibDeltaSystem.WebFramework.Extras.HTMLForm.Attributes.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.NetMessages.Master
{
    public class ManagerDeleteVersion
    {
        [FormElementTypeText("Version ID", "Version ID", "version_id")]
        public string version_id { get; set; }
    }
}
