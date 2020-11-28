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
        public NetManagerSite_Proxy[] proxies = new NetManagerSite_Proxy[0];
    }

    public class NetManagerSite_Proxy
    {
        public string from_path; //Ex: /api/
        public string to_path; //Ex: /
        public string proto; //Ex: http
        //This example takes a request from /api/ and forwards it to / using the protocol http. This is balanced between all sites assigned to this
    }
}
