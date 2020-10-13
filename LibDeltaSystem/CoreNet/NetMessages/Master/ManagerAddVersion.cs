using LibDeltaSystem.WebFramework.Extras.HTMLForm.Attributes.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.NetMessages.Master
{
    public class ManagerAddVersion
    {
        [FormElementTypeText("Package Name", "Package Name", "package_name")]
        public string package_name { get; set; }
    }
}
