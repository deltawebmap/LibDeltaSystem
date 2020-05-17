using LibDeltaSystem.Tools.DeltaWebFormat.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.ArkEntries.Dinosaur
{
    public class DinosaurEntry
    {
        public string screen_name { get; set; }
        public ushort hash { get; set; }
        public float colorizationIntensity { get; set; }
        public float babyGestationSpeed { get; set; }
        public float extraBabyGestationSpeedMultiplier { get; set; }
        public float babyAgeSpeed { get; set; }
        public float extraBabyAgeSpeedMultiplier { get; set; }
        public bool useBabyGestation { get; set; }
        public float extraBabyAgeMultiplier { get; set; }

        [WebFormatAttributeUseObject]
        public DinosaurEntryStatusComponent statusComponent { get; set; }

        [WebFormatAttributeUseObject]
        public DinosaurEntryFood[] adultFoods { get; set; }
        [WebFormatAttributeUseObject]
        public DinosaurEntryFood[] childFoods { get; set; }

        public string classname { get; set; }

        [WebFormatAttributeUseObject]
        public DeltaAsset icon { get; set; }

        public float[] baseLevel { get; set; }
        public float[] increasePerWildLevel { get; set; }
        public float[] increasePerTamedLevel { get; set; }
        public float[] additiveTamingBonus { get; set; }
        public float[] multiplicativeTamingBonus { get; set; }
        public float[] statImprintMult { get; set; }

        private const double ROUND_UP_DELTA = 0.0001;

        /// <summary>
        /// Calculates the maximum stats of a dinosaur.
        /// </summary>
        /// <param name="dino">Dinosaur to compute.</param>
        /// <param name="babyImprintingStatScaleMultiplier">Server setting.</param>
        /// <returns></returns>
        public Db.Content.DbArkDinosaurStats CalculateMaxStats(Db.Content.DbDino dino, float babyImprintingStatScaleMultiplier)
        {
            return new Db.Content.DbArkDinosaurStats
            {
                health = (float)CalculateValue(0, dino.base_level, dino.level, dino.is_tamed, dino.taming_effectiveness, dino.imprint_quality, true, babyImprintingStatScaleMultiplier),
                stamina = (float)CalculateValue(1, dino.base_level, dino.level, dino.is_tamed, dino.taming_effectiveness, dino.imprint_quality, true, babyImprintingStatScaleMultiplier),
                unknown1 = (float)CalculateValue(2, dino.base_level, dino.level, dino.is_tamed, dino.taming_effectiveness, dino.imprint_quality, true, babyImprintingStatScaleMultiplier),
                oxygen = (float)CalculateValue(3, dino.base_level, dino.level, dino.is_tamed, dino.taming_effectiveness, dino.imprint_quality, true, babyImprintingStatScaleMultiplier),
                food = (float)CalculateValue(4, dino.base_level, dino.level, dino.is_tamed, dino.taming_effectiveness, dino.imprint_quality, true, babyImprintingStatScaleMultiplier),
                water = (float)CalculateValue(5, dino.base_level, dino.level, dino.is_tamed, dino.taming_effectiveness, dino.imprint_quality, true, babyImprintingStatScaleMultiplier),
                unknown2 = (float)CalculateValue(6, dino.base_level, dino.level, dino.is_tamed, dino.taming_effectiveness, dino.imprint_quality, true, babyImprintingStatScaleMultiplier),
                inventoryWeight = (float)CalculateValue(7, dino.base_level, dino.level, dino.is_tamed, dino.taming_effectiveness, dino.imprint_quality, true, babyImprintingStatScaleMultiplier),
                meleeDamageMult = (float)CalculateValue(8, dino.base_level, dino.level, dino.is_tamed, dino.taming_effectiveness, dino.imprint_quality, true, babyImprintingStatScaleMultiplier),
                movementSpeedMult = (float)CalculateValue(9, dino.base_level, dino.level, dino.is_tamed, dino.taming_effectiveness, dino.imprint_quality, true, babyImprintingStatScaleMultiplier),
                unknown3 = (float)CalculateValue(10, dino.base_level, dino.level, dino.is_tamed, dino.taming_effectiveness, dino.imprint_quality, true, babyImprintingStatScaleMultiplier),
                unknown4 = (float)CalculateValue(11, dino.base_level, dino.level, dino.is_tamed, dino.taming_effectiveness, dino.imprint_quality, true, babyImprintingStatScaleMultiplier),
            };
        }

        /// <summary>
        /// Calculates a stat. More info here: https://github.com/cadon/ARKStatsExtractor/blob/c599dcdbef341feec1f9530fe7986de260b152cf/ARKBreedingStats/Stats.cs
        /// </summary>
        /// <param name="stat">Stat index</param>
        /// <param name="levelWild"></param>
        /// <param name="levelDom"></param>
        /// <param name="dom"></param>
        /// <param name="tamingEff"></param>
        /// <param name="imprintingBonus"></param>
        /// <param name="roundToIngamePrecision">Should be true.</param>
        /// <param name="babyImprintingStatScaleMultiplier">Server setting BabyImprintingStatScaleMultiplier</param>
        /// <returns></returns>
        public double CalculateValue(int stat, int levelWild, int levelDom, bool dom, double tamingEff, double imprintingBonus, bool roundToIngamePrecision, float babyImprintingStatScaleMultiplier)
        {
            var species = this;

            // if stat is generally available but level is set to -1 (== unknown), return -1 (== unknown)
            if (levelWild < 0 && species.increasePerWildLevel[stat] != 0)
                return -1;

            double add = 0, domMult = 1, imprintingM = 1, tamedBaseHP = 1;
            if (dom)
            {
                add = species.additiveTamingBonus[stat];//species.stats[stat].AddWhenTamed;
                double domMultAffinity = species.multiplicativeTamingBonus[stat]; //species.stats[stat].MultAffinity;
                // the multiplicative bonus is only multiplied with the TE if it is positive (i.e. negative boni won't get less bad if the TE is low)
                if (domMultAffinity >= 0)
                    domMultAffinity *= tamingEff;
                domMult = (tamingEff >= 0 ? (1 + domMultAffinity) : 1) * (1 + levelDom * species.increasePerTamedLevel[stat]/*species.stats[stat].IncPerTamedLevel*/);
                if (imprintingBonus > 0
                    && species.statImprintMult[stat] != 0
                    )
                    imprintingM = 1 + species.statImprintMult[stat] * imprintingBonus * babyImprintingStatScaleMultiplier;
                if (stat == 0)
                    tamedBaseHP = (float)species.statusComponent.tamedBaseHealthMultiplier;
            }
            //double result = Math.Round((species.stats[stat].BaseValue * tamedBaseHP * (1 + species.stats[stat].IncPerWildLevel * levelWild) * imprintingM + add) * domMult, Utils.precision(stat), MidpointRounding.AwayFromZero);
            // double is too precise and results in wrong values due to rounding. float results in better values, probably ARK uses float as well.
            // or rounding first to a precision of 7, then use the rounding of the precision
            //double resultt = Math.Round((species.stats[stat].BaseValue * tamedBaseHP * (1 + species.stats[stat].IncPerWildLevel * levelWild) * imprintingM + add) * domMult, 7);
            //resultt = Math.Round(resultt, Utils.precision(stat), MidpointRounding.AwayFromZero);

            // adding an epsilon to handle rounding-errors
            double result = (species.baseLevel[stat] * tamedBaseHP *
                    (1 + species.increasePerWildLevel[stat] * levelWild) * imprintingM + add) *
                    domMult + (precision(stat) == 3 ? ROUND_UP_DELTA * 0.01 : ROUND_UP_DELTA);

            if (result <= 0) return 0;

            if (roundToIngamePrecision)
                return Math.Round(result, precision(stat), MidpointRounding.AwayFromZero);

            return result;
        }

        public static int precision(int s)
        {
            // damage and speed are percentagevalues, need more precision
            return (s == (int)StatNames.SpeedMultiplier || s == (int)StatNames.MeleeDamageMultiplier || s == (int)StatNames.CraftingSpeedMultiplier) ? 3 : 1;
        }
    }

    public class DinosaurEntryStatusComponent
    {
        public float baseFoodConsumptionRate { get; set; }
        public float babyDinoConsumingFoodRateMultiplier { get; set; }
        public float extraBabyDinoConsumingFoodRateMultiplier { get; set; }
        public float foodConsumptionMultiplier { get; set; }
        public float tamedBaseHealthMultiplier { get; set; }
    }

    public class DinosaurEntryFood
    {
        [WebFormatAttributeUseNameTable]
        public string classname { get; set; }
        public float foodEffectivenessMultiplier { get; set; }
        public float affinityOverride { get; set; }
        public float affinityEffectivenessMultiplier { get; set; }
        public int foodCategory { get; set; }
        public float priority { get; set; }
    }
}
