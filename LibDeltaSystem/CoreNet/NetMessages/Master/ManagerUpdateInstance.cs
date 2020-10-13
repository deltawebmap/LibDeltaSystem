using LibDeltaSystem.WebFramework.Extras.HTMLForm.Attributes.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.NetMessages.Master
{
    public class ManagerUpdateInstance
    {
        [FormElementTypeText("Instance ID", "Instance ID", "instance_id")]
        public string instance_id { get; set; }
    }
}
