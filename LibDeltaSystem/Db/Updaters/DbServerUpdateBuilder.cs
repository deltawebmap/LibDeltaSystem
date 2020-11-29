using LibDeltaSystem.Db.System;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LibDeltaSystem.Db.Updaters
{
    public class DbServerUpdateBuilder : DbBaseUpdateBuilder<DbServer>
    {
        internal DbServerUpdateBuilder(DeltaConnection conn, DbServer server) : base(conn, server)
        {

        }

        private List<DbUser> rpcUsersAccessChanged = new List<DbUser>();
        private bool doRpcPublicServerUpdated = false;

        protected override Task UpdateDatabaseEntry(UpdateDefinition<DbServer> update)
        {
            var filter = Builders<DbServer>.Filter.Eq("_id", context._id);
            return conn.system_servers.FindOneAndUpdateAsync(filter, update);
        }

        protected override async Task PreApply()
        {
            if(doRpcPublicServerUpdated)
                conn.events.OnServerUpdated(context);
        }

        protected override async Task PostApply()
        {
            foreach (var u in rpcUsersAccessChanged)
                await conn.events.OnUserServerAccessChangedAsync(context, u);
        }

        protected void RpcUserAccessChanged(DbUser user)
        {
            rpcUsersAccessChanged.Add(user);
        }

        //Updates

        public DbServerUpdateBuilder UpdateFlag(int index, bool value)
        {
            if(value)
                context.flags |= 1 << index;
            else
                context.flags &= ~(1 << index);
            updates.Add(Builders<DbServer>.Update.Set("flags", context.flags));
            doRpcPublicServerUpdated = true;
            return this;
        }

        public DbServerUpdateBuilder UpdatePermissionFlag(int index, bool value)
        {
            if (value)
                context.permission_flags |= 1 << index;
            else
                context.permission_flags &= ~(1 << index);
            updates.Add(Builders<DbServer>.Update.Set("permission_flags", context.permission_flags));
            doRpcPublicServerUpdated = true;
            return this;
        }

        public DbServerUpdateBuilder UpdatePermissionFlags(int value)
        {
            context.permission_flags = value;
            updates.Add(Builders<DbServer>.Update.Set("permission_flags", context.permission_flags));
            doRpcPublicServerUpdated = true;
            return this;
        }

        public DbServerUpdateBuilder UpdatePermissionTemplate(string value)
        {
            context.permissions_template = value;
            updates.Add(Builders<DbServer>.Update.Set("permissions_template", context.permissions_template));
            doRpcPublicServerUpdated = true;
            return this;
        }

        public DbServerUpdateBuilder AddAdmin(DbUser user)
        {
            if (!context.admins.Contains(user._id))
            {
                context.admins.Add(user._id);
                updates.Add(Builders<DbServer>.Update.Set("admins", context.admins));
                doRpcPublicServerUpdated = true;
                RpcNotifyGroupsUpdated(user._id);
                RpcUserAccessChanged(user);
            }
            return this;
        }

        public DbServerUpdateBuilder RemoveAdmin(DbUser user)
        {
            if (context.admins.Contains(user._id))
            {
                context.admins.Remove(user._id);
                updates.Add(Builders<DbServer>.Update.Set("admins", context.admins));
                doRpcPublicServerUpdated = true;
                RpcNotifyGroupsUpdated(user._id);
                RpcUserAccessChanged(user);
            }
            return this;
        }

        public DbServerUpdateBuilder UpdateServerIcon(string url)
        {
            context.image_url = url;
            context.has_custom_image = true;
            updates.Add(Builders<DbServer>.Update.Set("image_url", context.image_url));
            updates.Add(Builders<DbServer>.Update.Set("has_custom_image", context.has_custom_image));
            doRpcPublicServerUpdated = true;
            return this;
        }

        public DbServerUpdateBuilder UpdateServerName(string name)
        {
            context.display_name = name;
            updates.Add(Builders<DbServer>.Update.Set("display_name", context.display_name));
            doRpcPublicServerUpdated = true;
            return this;
        }

        public DbServerUpdateBuilder UpdateServerNameValidated(string name)
        {
            if (name.Length < 2 || name.Length > 32)
                throw new WebFramework.DeltaWebException("Server name must be between 2-32 characters.", 400);
            return UpdateServerName(name);
        }

        public DbServerUpdateBuilder UpdateSecureMode(bool secure)
        {
            context.secure_mode = secure;
            updates.Add(Builders<DbServer>.Update.Set("secure_mode", context.secure_mode));
            doRpcPublicServerUpdated = true;
            return this;
        }

        public DbServerUpdateBuilder UpdateLastSyncState(ObjectId state)
        {
            context.last_sync_state = state;
            updates.Add(Builders<DbServer>.Update.Set("last_sync_state", context.last_sync_state));
            return this;
        }

        public DbServerUpdateBuilder UpdateLastSyncConnectedTime(DateTime time)
        {
            context.last_connected_time = time;
            updates.Add(Builders<DbServer>.Update.Set("last_connected_time", context.last_connected_time));
            return this;
        }

        public DbServerUpdateBuilder UpdateLastSyncPingedTime(DateTime time)
        {
            context.last_pinged_time = time;
            updates.Add(Builders<DbServer>.Update.Set("last_pinged_time", context.last_pinged_time));
            return this;
        }
        
        public DbServerUpdateBuilder UpdateLastSyncClientVersion(int version)
        {
            context.last_client_version = version;
            updates.Add(Builders<DbServer>.Update.Set("last_client_version", context.last_client_version));
            return this;
        }
    }
}
