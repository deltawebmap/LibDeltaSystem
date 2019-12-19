using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.System
{
    public class DbOauthApp : DbBaseSystem
    {
        /// <summary>
        /// Owner user 
        /// </summary>
        public string owner_id { get; set; }

        /// <summary>
        /// The name of this application
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Subtitle shown to users
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// The URL to redirect to when auth is finished
        /// </summary>
        public string redirect_uri { get; set; }

        /// <summary>
        /// URL to an icon
        /// </summary>
        public string icon_url { get; set; }

        /// <summary>
        /// Client ID used
        /// </summary>
        public string client_id { get; set; }

        /// <summary>
        /// Secret code used
        /// </summary>
        public string client_secret { get; set; }
    }
}
