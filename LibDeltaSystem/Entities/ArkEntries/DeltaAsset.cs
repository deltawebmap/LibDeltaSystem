using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.ArkEntries
{
    /// <summary>
    /// Represents an asset
    /// </summary>
    public class DeltaAsset
    {
        public string image_url { get; set; }
        public string image_thumb_url { get; set; }

        public static readonly DeltaAsset MISSING_ICON = new DeltaAsset
        {
            image_thumb_url = "https://icon-assets.deltamap.net/legacy/broken_item_thumb.png",
            image_url = "https://icon-assets.deltamap.net/legacy/broken_item.png"
        };
    }
}
