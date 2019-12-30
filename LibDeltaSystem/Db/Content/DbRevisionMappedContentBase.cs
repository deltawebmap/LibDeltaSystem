using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Db.Content
{
    public class DbRevisionMappedContentBase : DbContentBase
    {
        /// <summary>
        /// Revision ID this is mapped to
        /// </summary>
        public ulong revision_id { get; set; }

        /// <summary>
        /// The revision index this is mapped to
        /// </summary>
        public byte revision_type { get; set; }
    }
}
