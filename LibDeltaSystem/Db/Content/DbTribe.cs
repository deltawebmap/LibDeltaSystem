﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.Content
{
    /// <summary>
    /// Tribe that sits in it's own file on the disk
    /// </summary>
    public class DbTribe : DbContentBase
    {
        /// <summary>
        /// Name of the tribe
        /// </summary>
        public string tribe_name { get; set; }

        /// <summary>
        /// Owner of the tribe
        /// </summary>
        public uint tribe_owner { get; set; }
    }
}
