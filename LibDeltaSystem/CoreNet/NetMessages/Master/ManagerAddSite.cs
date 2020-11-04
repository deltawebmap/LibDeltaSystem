using LibDeltaSystem.WebFramework.Extras.HTMLForm.Attributes.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.NetMessages.Master
{
    public class ManagerAddSite
    {
        [FormElementTypeText("Domain", "deltamap.net", "domain")]
        public string domain { get; set; }
        [FormElementTypeText("Document Root", "/var/www/delta/null/", "droot")]
        public string document_root { get; set; }
        [FormElementTypeText("Proxy Root", "/", "proot")]
        public string proxy_root { get; set; }
        [FormElementTypeText("Proxy Protocol", "http", "proto")]
        public string proto { get; set; }
    }
}
