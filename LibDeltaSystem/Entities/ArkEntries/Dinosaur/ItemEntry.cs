using LibDeltaSystem.Tools.DeltaWebFormat.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.ArkEntries.Dinosaur
{
    public class ItemEntry
    {
        public string classname { get; set; }
        public ushort hash { get; set; }

        [WebFormatAttributeUseObject]
        public DeltaAsset icon { get; set; }

        public bool hideFromInventoryDisplay { get; set; }
        public bool useItemDurability { get; set; }
        public bool isTekItem { get; set; }
        public bool allowUseWhileRiding { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public float spoilingTime { get; set; } //0 if not spoiling
        public float baseItemWeight { get; set; }
        public float useCooldownTime { get; set; }
        public float baseCraftingXP { get; set; }
        public float baseRepairingXP { get; set; }
        public int maxItemQuantity { get; set; }

        public string structure_classname { get; set; } //The name of the structure this builds, if any

        //Consumables
        public ItemEntry_ConsumableAddStatusValue[] addStatusValues { get; set; }
    }

    public class ItemEntry_ConsumableAddStatusValue
    {
        public float baseAmountToAdd { get; set; }
        public bool percentOfMaxStatusValue { get; set; }
        public bool percentOfCurrentStatusValue { get; set; }
        public bool useItemQuality { get; set; }
        public bool addOverTime { get; set; }
        public bool setValue { get; set; }
        public bool setAdditionalValue { get; set; }
        public float addOverTimeSpeed { get; set; }
        public float itemQualityAddValueMultiplier { get; set; }

        [WebFormatAttributeUseNameTable]
        public string statusValueType { get; set; }
    }
}
