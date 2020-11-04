using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.CoreNet.NetMessages.Master.Entities
{
    public class NetManagerSite
    {
        public string id;
        public DateTime cert_expiry;
        public string site_domain;
        public string cert_name;
        public string document_root;
        public string proxy_root;
        public string proto;
    }
}
