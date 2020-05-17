using LibDeltaSystem.Db.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.CommonNet
{
    public class NetCluster
    {
        public string name;
        public string id;

        public static NetCluster GetCluster(DbCluster cluster)
        {
            return new NetCluster
            {
                id = cluster.id,
                name = cluster.name
            };
        }
    }
}
