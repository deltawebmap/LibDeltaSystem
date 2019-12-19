using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace LibDeltaSystem.Db.System.Entities
{
    public class DbServerGameSettings
    {
        [ServerSettingsMetadata("ListenServerTetherDistanceMultiplier", "ServerSettings")]
        public float ListenServerTetherDistanceMultiplier { get; set; }
        [ServerSettingsMetadata("RaidDinoCharacterFoodDrainMultiplier", "ServerSettings")]
        public float RaidDinoCharacterFoodDrainMultiplier { get; set; }
        [ServerSettingsMetadata("StructurePreventResourceRadiusMultiplier", "ServerSettings")]
        public float StructurePreventResourceRadiusMultiplier { get; set; }
        [ServerSettingsMetadata("PvEDinoDecayPeriodMultiplier", "ServerSettings")]
        public float PvEDinoDecayPeriodMultiplier { get; set; }
        [ServerSettingsMetadata("AllowRaidDinoFeeding", "ServerSettings")]
        public bool AllowRaidDinoFeeding { get; set; }
        [ServerSettingsMetadata("PerPlatformMaxStructuresMultiplier", "ServerSettings")]
        public float PerPlatformMaxStructuresMultiplier { get; set; }
        [ServerSettingsMetadata("GlobalVoiceChat", "ServerSettings")]
        public bool GlobalVoiceChat { get; set; }
        [ServerSettingsMetadata("ProximityChat", "ServerSettings")]
        public bool ProximityChat { get; set; }
        [ServerSettingsMetadata("NoTributeDownloads", "ServerSettings")]
        public bool NoTributeDownloads { get; set; }
        [ServerSettingsMetadata("AllowThirdPersonPlayer", "ServerSettings")]
        public bool AllowThirdPersonPlayer { get; set; }
        [ServerSettingsMetadata("AlwaysNotifyPlayerLeft", "ServerSettings")]
        public bool AlwaysNotifyPlayerLeft { get; set; }
        [ServerSettingsMetadata("DontAlwaysNotifyPlayerJoined", "ServerSettings")]
        public bool DontAlwaysNotifyPlayerJoined { get; set; }
        [ServerSettingsMetadata("ServerHardcore", "ServerSettings")]
        public bool ServerHardcore { get; set; }
        [ServerSettingsMetadata("ServerPVE", "ServerSettings")]
        public bool ServerPVE { get; set; }
        [ServerSettingsMetadata("ServerCrosshair", "ServerSettings")]
        public bool ServerCrosshair { get; set; }
        [ServerSettingsMetadata("ServerForceNoHUD", "ServerSettings")]
        public bool ServerForceNoHUD { get; set; }
        [ServerSettingsMetadata("ShowMapPlayerLocation", "ServerSettings")]
        public bool ShowMapPlayerLocation { get; set; }
        [ServerSettingsMetadata("EnablePvPGamma", "ServerSettings")]
        public bool EnablePvPGamma { get; set; }
        [ServerSettingsMetadata("DisableStructureDecayPvE", "ServerSettings")]
        public bool DisableStructureDecayPvE { get; set; }
        [ServerSettingsMetadata("AllowFlyerCarryPvE", "ServerSettings")]
        public bool AllowFlyerCarryPvE { get; set; }
        [ServerSettingsMetadata("OnlyAllowSpecifiedEngrams", "ServerSettings")]
        public bool OnlyAllowSpecifiedEngrams { get; set; }
        [ServerSettingsMetadata("PreventDownloadSurvivors", "ServerSettings")]
        public bool PreventDownloadSurvivors { get; set; }
        [ServerSettingsMetadata("PreventDownloadItems", "ServerSettings")]
        public bool PreventDownloadItems { get; set; }
        [ServerSettingsMetadata("PreventDownloadDinos", "ServerSettings")]
        public bool PreventDownloadDinos { get; set; }
        [ServerSettingsMetadata("DisablePvEGamma", "ServerSettings")]
        public bool DisablePvEGamma { get; set; }
        [ServerSettingsMetadata("DisableDinoDecayPvE", "ServerSettings")]
        public bool DisableDinoDecayPvE { get; set; }
        [ServerSettingsMetadata("AdminLogging", "ServerSettings")]
        public bool AdminLogging { get; set; }
        [ServerSettingsMetadata("AllowCaveBuildingPvE", "ServerSettings")]
        public bool AllowCaveBuildingPvE { get; set; }
        [ServerSettingsMetadata("ForceAllowCaveFlyers", "ServerSettings")]
        public bool ForceAllowCaveFlyers { get; set; }
        [ServerSettingsMetadata("PreventOfflinePvP", "ServerSettings")]
        public bool PreventOfflinePvP { get; set; }
        [ServerSettingsMetadata("PvPDinoDecay", "ServerSettings")]
        public bool PvPDinoDecay { get; set; }
        [ServerSettingsMetadata("OverrideStructurePlatformPrevention", "ServerSettings")]
        public bool OverrideStructurePlatformPrevention { get; set; }
        [ServerSettingsMetadata("AllowAnyoneBabyImprintCuddle", "ServerSettings")]
        public bool AllowAnyoneBabyImprintCuddle { get; set; }
        [ServerSettingsMetadata("DisableImprintDinoBuff", "ServerSettings")]
        public bool DisableImprintDinoBuff { get; set; }
        [ServerSettingsMetadata("ShowFloatingDamageText", "ServerSettings")]
        public bool ShowFloatingDamageText { get; set; }
        [ServerSettingsMetadata("PreventDiseases", "ServerSettings")]
        public bool PreventDiseases { get; set; }
        [ServerSettingsMetadata("NonPermanentDiseases", "ServerSettings")]
        public bool NonPermanentDiseases { get; set; }
        [ServerSettingsMetadata("EnableExtraStructurePreventionVolumes", "ServerSettings")]
        public bool EnableExtraStructurePreventionVolumes { get; set; }
        [ServerSettingsMetadata("PreventTribeAlliances", "ServerSettings")]
        public bool PreventTribeAlliances { get; set; }
        [ServerSettingsMetadata("PreventOfflinePvPInterval", "ServerSettings")]
        public float PreventOfflinePvPInterval { get; set; }
        [ServerSettingsMetadata("DifficultyOffset", "ServerSettings")]
        public float DifficultyOffset { get; set; }
        [ServerSettingsMetadata("DayCycleSpeedScale", "ServerSettings")]
        public float DayCycleSpeedScale { get; set; }
        [ServerSettingsMetadata("DayTimeSpeedScale", "ServerSettings")]
        public float DayTimeSpeedScale { get; set; }
        [ServerSettingsMetadata("NightTimeSpeedScale", "ServerSettings")]
        public float NightTimeSpeedScale { get; set; }
        [ServerSettingsMetadata("DinoDamageMultiplier", "ServerSettings")]
        public float DinoDamageMultiplier { get; set; }
        [ServerSettingsMetadata("PlayerDamageMultiplier", "ServerSettings")]
        public float PlayerDamageMultiplier { get; set; }
        [ServerSettingsMetadata("StructureDamageMultiplier", "ServerSettings")]
        public float StructureDamageMultiplier { get; set; }
        [ServerSettingsMetadata("PlayerResistanceMultiplier", "ServerSettings")]
        public float PlayerResistanceMultiplier { get; set; }
        [ServerSettingsMetadata("DinoResistanceMultiplier", "ServerSettings")]
        public float DinoResistanceMultiplier { get; set; }
        [ServerSettingsMetadata("StructureResistanceMultiplier", "ServerSettings")]
        public float StructureResistanceMultiplier { get; set; }
        [ServerSettingsMetadata("XPMultiplier", "ServerSettings")]
        public float XPMultiplier { get; set; }
        [ServerSettingsMetadata("TamingSpeedMultiplier", "ServerSettings")]
        public float TamingSpeedMultiplier { get; set; }
        [ServerSettingsMetadata("HarvestAmountMultiplier", "ServerSettings")]
        public float HarvestAmountMultiplier { get; set; }
        [ServerSettingsMetadata("PlayerCharacterWaterDrainMultiplier", "ServerSettings")]
        public float PlayerCharacterWaterDrainMultiplier { get; set; }
        [ServerSettingsMetadata("PlayerCharacterFoodDrainMultiplier", "ServerSettings")]
        public float PlayerCharacterFoodDrainMultiplier { get; set; }
        [ServerSettingsMetadata("DinoCharacterFoodDrainMultiplier", "ServerSettings")]
        public float DinoCharacterFoodDrainMultiplier { get; set; }
        [ServerSettingsMetadata("PlayerCharacterStaminaDrainMultiplier", "ServerSettings")]
        public float PlayerCharacterStaminaDrainMultiplier { get; set; }
        [ServerSettingsMetadata("DinoCharacterStaminaDrainMultiplier", "ServerSettings")]
        public float DinoCharacterStaminaDrainMultiplier { get; set; }
        [ServerSettingsMetadata("PlayerCharacterHealthRecoveryMultiplier", "ServerSettings")]
        public float PlayerCharacterHealthRecoveryMultiplier { get; set; }
        [ServerSettingsMetadata("DinoCharacterHealthRecoveryMultiplier", "ServerSettings")]
        public float DinoCharacterHealthRecoveryMultiplier { get; set; }
        [ServerSettingsMetadata("DinoCountMultiplier", "ServerSettings")]
        public float DinoCountMultiplier { get; set; }
        [ServerSettingsMetadata("HarvestHealthMultiplier", "ServerSettings")]
        public float HarvestHealthMultiplier { get; set; }
        [ServerSettingsMetadata("PvEStructureDecayPeriodMultiplier", "ServerSettings")]
        public float PvEStructureDecayPeriodMultiplier { get; set; }
        [ServerSettingsMetadata("ResourcesRespawnPeriodMultiplier", "ServerSettings")]
        public float ResourcesRespawnPeriodMultiplier { get; set; }
        [ServerSettingsMetadata("ActiveMods", "ServerSettings")]
        public string ActiveMods { get; set; }
        [ServerSettingsMetadata("ActiveMapMod", "ServerSettings")]
        public int ActiveMapMod { get; set; }
        [ServerSettingsMetadata("ServerPassword", "ServerSettings")]
        public string ServerPassword { get; set; }
        [ServerSettingsMetadata("ServerAdminPassword", "ServerSettings")]
        public string ServerAdminPassword { get; set; }
        [ServerSettingsMetadata("RCONPort", "ServerSettings")]
        public int RCONPort { get; set; }
        [ServerSettingsMetadata("TheMaxStructuresInRange", "ServerSettings")]
        public float TheMaxStructuresInRange { get; set; }
        [ServerSettingsMetadata("OxygenSwimSpeedStatMultiplier", "ServerSettings")]
        public float OxygenSwimSpeedStatMultiplier { get; set; }
        [ServerSettingsMetadata("TribeNameChangeCooldown", "ServerSettings")]
        public float TribeNameChangeCooldown { get; set; }
        [ServerSettingsMetadata("PlatformSaddleBuildAreaBoundsMultiplier", "ServerSettings")]
        public float PlatformSaddleBuildAreaBoundsMultiplier { get; set; }
        [ServerSettingsMetadata("StructurePickupTimeAfterPlacement", "ServerSettings")]
        public float StructurePickupTimeAfterPlacement { get; set; }
        [ServerSettingsMetadata("StructurePickupHoldDuration", "ServerSettings")]
        public float StructurePickupHoldDuration { get; set; }
        [ServerSettingsMetadata("AllowIntegratedSPlusStructures", "ServerSettings")]
        public bool AllowIntegratedSPlusStructures { get; set; }
        [ServerSettingsMetadata("AllowHideDamageSourceFromLogs", "ServerSettings")]
        public bool AllowHideDamageSourceFromLogs { get; set; }
        [ServerSettingsMetadata("KickIdlePlayersPeriod", "ServerSettings")]
        public float KickIdlePlayersPeriod { get; set; }
        [ServerSettingsMetadata("AutoSavePeriodMinutes", "ServerSettings")]
        public float AutoSavePeriodMinutes { get; set; }
        [ServerSettingsMetadata("MaxTamedDinos", "ServerSettings")]
        public float MaxTamedDinos { get; set; }
        [ServerSettingsMetadata("ItemStackSizeMultiplier", "ServerSettings")]
        public float ItemStackSizeMultiplier { get; set; }
        [ServerSettingsMetadata("RCONServerGameLogBuffer", "ServerSettings")]
        public float RCONServerGameLogBuffer { get; set; }
        [ServerSettingsMetadata("AllowHitMarkers", "ServerSettings")]
        public bool AllowHitMarkers { get; set; }

        [ServerSettingsMetadata("KillXPMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float KillXPMultiplier { get; set; }
        [ServerSettingsMetadata("HarvestXPMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float HarvestXPMultiplier { get; set; }
        [ServerSettingsMetadata("CraftXPMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float CraftXPMultiplier { get; set; }
        [ServerSettingsMetadata("GenericXPMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float GenericXPMultiplier { get; set; }
        [ServerSettingsMetadata("SpecialXPMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float SpecialXPMultiplier { get; set; }
        [ServerSettingsMetadata("PvPZoneStructureDamageMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float PvPZoneStructureDamageMultiplier { get; set; }
        [ServerSettingsMetadata("OverrideMaxExperiencePointsPlayer", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public int OverrideMaxExperiencePointsPlayer { get; set; }
        [ServerSettingsMetadata("OverrideMaxExperiencePointsDino", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public int OverrideMaxExperiencePointsDino { get; set; }
        [ServerSettingsMetadata("GlobalSpoilingTimeMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float GlobalSpoilingTimeMultiplier { get; set; }
        [ServerSettingsMetadata("GlobalItemDecompositionTimeMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float GlobalItemDecompositionTimeMultiplier { get; set; }
        [ServerSettingsMetadata("GlobalCorpseDecompositionTimeMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float GlobalCorpseDecompositionTimeMultiplier { get; set; }
        [ServerSettingsMetadata("bAutoPvETimer", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public bool bAutoPvETimer { get; set; }
        [ServerSettingsMetadata("bAutoPvEUseSystemTime", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public bool bAutoPvEUseSystemTime { get; set; }
        [ServerSettingsMetadata("AutoPvEStartTimeSeconds", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float AutoPvEStartTimeSeconds { get; set; }
        [ServerSettingsMetadata("AutoPvEStopTimeSeconds", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float AutoPvEStopTimeSeconds { get; set; }
        [ServerSettingsMetadata("bIncreasePvPRespawnInterval", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public bool bIncreasePvPRespawnInterval { get; set; }
        [ServerSettingsMetadata("IncreasePvPRespawnIntervalCheckPeriod", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float IncreasePvPRespawnIntervalCheckPeriod { get; set; }
        [ServerSettingsMetadata("IncreasePvPRespawnIntervalMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float IncreasePvPRespawnIntervalMultiplier { get; set; }
        [ServerSettingsMetadata("IncreasePvPRespawnIntervalBaseAmount", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float IncreasePvPRespawnIntervalBaseAmount { get; set; }
        [ServerSettingsMetadata("ResourceNoReplenishRadiusStructures", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float ResourceNoReplenishRadiusStructures { get; set; }
        [ServerSettingsMetadata("ResourceNoReplenishRadiusPlayers", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float ResourceNoReplenishRadiusPlayers { get; set; }
        [ServerSettingsMetadata("CropGrowthSpeedMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float CropGrowthSpeedMultiplier { get; set; }
        [ServerSettingsMetadata("LayEggIntervalMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float LayEggIntervalMultiplier { get; set; }
        [ServerSettingsMetadata("PoopIntervalMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float PoopIntervalMultiplier { get; set; }
        [ServerSettingsMetadata("CropDecaySpeedMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float CropDecaySpeedMultiplier { get; set; }
        [ServerSettingsMetadata("bFlyerPlatformAllowUnalignedDinoBasing", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public bool bFlyerPlatformAllowUnalignedDinoBasing { get; set; }
        [ServerSettingsMetadata("MaxNumberOfPlayersInTribe", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public int MaxNumberOfPlayersInTribe { get; set; }
        [ServerSettingsMetadata("MatingIntervalMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float MatingIntervalMultiplier { get; set; }
        [ServerSettingsMetadata("EggHatchSpeedMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float EggHatchSpeedMultiplier { get; set; }
        [ServerSettingsMetadata("BabyMatureSpeedMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float BabyMatureSpeedMultiplier { get; set; }
        [ServerSettingsMetadata("BabyFoodConsumptionSpeedMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float BabyFoodConsumptionSpeedMultiplier { get; set; }
        [ServerSettingsMetadata("StructureDamageRepairCooldown", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float StructureDamageRepairCooldown { get; set; }
        [ServerSettingsMetadata("CustomRecipeEffectivenessMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float CustomRecipeEffectivenessMultiplier { get; set; }
        [ServerSettingsMetadata("CustomRecipeSkillMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float CustomRecipeSkillMultiplier { get; set; }
        [ServerSettingsMetadata("bPvEAllowTribeWar", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public bool bPvEAllowTribeWar { get; set; }
        [ServerSettingsMetadata("bPvEAllowTribeWarCancel", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public bool bPvEAllowTribeWarCancel { get; set; }
        [ServerSettingsMetadata("bAllowCustomRecipes", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public bool bAllowCustomRecipes { get; set; }
        [ServerSettingsMetadata("bPassiveDefensesDamageRiderlessDinos", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public bool bPassiveDefensesDamageRiderlessDinos { get; set; }
        [ServerSettingsMetadata("bDisableFriendlyFire", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public bool bDisableFriendlyFire { get; set; }
        [ServerSettingsMetadata("DinoHarvestingDamageMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float DinoHarvestingDamageMultiplier { get; set; }
        [ServerSettingsMetadata("PlayerHarvestingDamageMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float PlayerHarvestingDamageMultiplier { get; set; }
        [ServerSettingsMetadata("DinoTurretDamageMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float DinoTurretDamageMultiplier { get; set; }
        [ServerSettingsMetadata("bDisableLootCrates", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public bool bDisableLootCrates { get; set; }
        [ServerSettingsMetadata("BabyImprintingStatScaleMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float BabyImprintingStatScaleMultiplier { get; set; }
        [ServerSettingsMetadata("BabyCuddleIntervalMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float BabyCuddleIntervalMultiplier { get; set; }
        [ServerSettingsMetadata("BabyCuddleGracePeriodMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float BabyCuddleGracePeriodMultiplier { get; set; }
        [ServerSettingsMetadata("BabyCuddleLoseImprintQualitySpeedMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float BabyCuddleLoseImprintQualitySpeedMultiplier { get; set; }
        [ServerSettingsMetadata("bDisableDinoTaming", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public bool bDisableDinoTaming { get; set; }
        [ServerSettingsMetadata("bDisableDinoRiding", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public bool bDisableDinoRiding { get; set; }
        [ServerSettingsMetadata("bUseSingleplayerSettings", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public bool bUseSingleplayerSettings { get; set; }
        [ServerSettingsMetadata("bUseCorpseLocator", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public bool bUseCorpseLocator { get; set; }
        [ServerSettingsMetadata("bDisableStructurePlacementCollision", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public bool bDisableStructurePlacementCollision { get; set; }
        [ServerSettingsMetadata("SupplyCrateLootQualityMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float SupplyCrateLootQualityMultiplier { get; set; }
        [ServerSettingsMetadata("FishingLootQualityMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float FishingLootQualityMultiplier { get; set; }
        [ServerSettingsMetadata("CraftingSkillBonusMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float CraftingSkillBonusMultiplier { get; set; }
        [ServerSettingsMetadata("bAllowPlatformSaddleMultiFloors", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public bool bAllowPlatformSaddleMultiFloors { get; set; }
        [ServerSettingsMetadata("bAllowUnlimitedRespecs", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public bool bAllowUnlimitedRespecs { get; set; }
        [ServerSettingsMetadata("FuelConsumptionIntervalMultiplier", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public float FuelConsumptionIntervalMultiplier { get; set; }
        [ServerSettingsMetadata("bShowCreativeMode", "/Game/PrimalEarth/CoreBlueprints/TestGameMode.TestGameMode_C")]
        public bool bShowCreativeMode { get; set; }
    }
}
