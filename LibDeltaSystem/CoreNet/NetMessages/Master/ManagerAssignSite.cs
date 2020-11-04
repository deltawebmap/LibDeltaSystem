using LibDeltaSystem.WebFramework.Extras.HTMLForm.Attributes.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.NetMessages.Master
{
    public class ManagerAssignSite
    {
        [FormElementTypeText("Instance ID", "Instance ID", "instance_id")]
        public string instance_id { get; set; }
        [FormElementTypeText("Site ID", "Site ID", "site_id")]
        public string site_id { get; set; }
    }
}
