using LibDeltaSystem.Db.System;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Tools
{
    public static class FilterBuilderToolDb
    {
        public static FilterDefinition<T> CreateTribeFilter<T>(DbServer server, int? tribeId)
        {
            var filterBuilder = Builders<T>.Filter;
            if (tribeId.HasValue)
                return filterBuilder.Eq("server_id", server._id) & filterBuilder.Eq("tribe_id", tribeId);
            else
                return filterBuilder.Eq("server_id", server._id);
        }
    }
}
