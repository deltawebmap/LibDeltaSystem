using LibDeltaSystem.Db.System;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.Updaters
{
    public abstract class DbBaseUpdateBuilder<T>
    {
        protected DeltaConnection conn;
        protected List<UpdateDefinition<T>> updates;
        protected T context;

        private List<ObjectId> rpcUsersNotifyGroupsUpdated = new List<ObjectId>();

        protected DbBaseUpdateBuilder(DeltaConnection conn, T context)
        {
            this.conn = conn;
            this.context = context;
            updates = new List<UpdateDefinition<T>>();
        }

        protected void RpcNotifyGroupsUpdated(ObjectId user)
        {
            rpcUsersNotifyGroupsUpdated.Add(user);
        }

        protected abstract Task UpdateDatabaseEntry(UpdateDefinition<T> update);
        protected abstract Task PreApply();
        protected abstract Task PostApply();

        public async Task Apply()
        {
            //Write changes to database
            if(updates.Count > 0)
            {
                UpdateDefinition<T> update = Builders<T>.Update.Combine(updates);
                await UpdateDatabaseEntry(update);
            }

            //Run pre apply stuff
            await PreApply();

            //Apply RPC changes
            foreach (var u in rpcUsersNotifyGroupsUpdated)
                conn.events.NotifyUserGroupsUpdated(u);

            //Run post apply
            await PostApply();
        }
    }
}
