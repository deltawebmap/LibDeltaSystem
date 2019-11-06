﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.Content
{
    public class DbPlayerProfile : DbContentBase
    {
        /// <summary>
        /// Player Steam name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// In game name
        /// </summary>
        public string ig_name { get; set; }

        /// <summary>
        /// Ark player ID
        /// </summary>
        public ulong ark_id { get; set; }

        /// <summary>
        /// The Steam ID used for this character
        /// </summary>
        public string steam_id { get; set; }

        /// <summary>
        /// Last time this person logged into the game
        /// </summary>
        public double last_login { get; set; }

        /// <summary>
        /// Steam icon
        /// </summary>
        public string icon { get; set; }
    }
}
