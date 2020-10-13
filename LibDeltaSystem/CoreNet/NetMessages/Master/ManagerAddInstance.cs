using LibDeltaSystem.WebFramework.Extras.HTMLForm.Attributes.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.NetMessages.Master
{
    public class ManagerAddInstance
    {
        [FormElementTypeText("Package Name", "Package Name", "package_name")]
        public string package_name { get; set; }
        [FormElementTypeNumber("Count", "count", 1, 255, 1)]
        public int count { get; set; }
    }
}
