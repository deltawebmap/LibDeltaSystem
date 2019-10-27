using System;
using System.Collections.Generic;
using System.Text;

namespace LibDeltaSystem.Entities.ArkEntries.Dinosaur
{
    public class DinosaurEntry
    {
        public string screen_name;
        public float colorizationIntensity;
        public float babyGestationSpeed;
        public float extraBabyGestationSpeedMultiplier;
        public float babyAgeSpeed;
        public float extraBabyAgeSpeedMultiplier;
        public bool useBabyGestation;
        public float extraBabyAgeMultiplier;

        public DinosaurEntryStatusComponent statusComponent;

        public List<DinosaurEntryFood> adultFoods;
        public List<DinosaurEntryFood> childFoods;

        public string classname;

        public DeltaAsset icon;

        public float[] baseLevel;
        public float[] increasePerWildLevel;
        public float[] increasePerTamedLevel;
        public float[] additiveTamingBonus;
        public float[] multiplicativeTamingBonus;

        public int version;
    }

    public class DinosaurEntryStatusComponent
    {
        public float baseFoodConsumptionRate;
        public float babyDinoConsumingFoodRateMultiplier;
        public float extraBabyDinoConsumingFoodRateMultiplier;
        public float foodConsumptionMultiplier;
        public float tamedBaseHealthMultiplier;
    }

    public class DinosaurEntryFood
    {
        public string classname;
        public float foodEffectivenessMultiplier;
        public float affinityOverride;
        public float affinityEffectivenessMultiplier;
        public int foodCategory;
        public float priority;
    }
}
