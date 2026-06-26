namespace ToolData
{
    public static class Strings
    {
        public const string GameVersion = "3.7";
        public const string ModifierVersion = "5.0.11";
        public const string PipeName = "infinite75_pvzrhtools";
        public const string RunModifierArgument = "--run_modifier";

        public static string GetGameVersion(string gameHash) => gameHash switch
        {
            "a9d8b12bb519944f35aa83ba3f20ef24abc15e672071db605129e98a68759240" => "3.6.1",
            "a436fd03e084010b4d5c19896f12b10ebb1e44d3172534cda335f1c20b591281" => "3.7",
            _ => "<internal alpha>"
        };

        public const string Exit = "Exit";
        public const string ReloadInitData = "ReloadInitData";

        #region 通用修改

        public const string DevMode = "DevMode";
        public const string GameSpeed = "GameSpeed";
        public const string GameSpeedEnabled = "GameSpeedEnabled";
        public const string GloveNoCD = "GloveNoCD";
        public const string HammerNoCD = "HammerNoCD";
        public const string HammerFullCD = "HammerFullCD";
        public const string GloveFullCD = "GloveFullCD";
        public const string WheelNoCD = "WheelNoCD";
        public const string WheelFullCD = "WheelFullCD";
        public const string FreePlanting = "FreePlanting";
        public const string CardFreeCD = "CardFreeCD";
        public const string RemoveFusionLimit = "RemoveFusionLimit";
        public const string NewZombieUpdateCD = "NewZombieUpdateCD";
        public const string UnlimitedScore = "UnlimitedScore";
        public const string UnlimitedRefresh = "UnlimitedRefresh";

        public const string Sun = "Sun";
        public const string LockSun = "LockSun";
        public const string Money = "Money";
        public const string LockMoney = "LockMoney";
        public const string PauseSpawn = "PauseSpawn";
        public const string NoFail = "NoFail";
        public const string RemoveAllPlants = "RemoveAllPlants";
        public const string RemoveAllZombies = "RemoveAllZombies";
        public const string MindCtrlAllZombies = "MindCtrlAllZombies";
        public const string RemoveAllIceRoads = "RemoveAllIceRoads";
        public const string RemoveAllHoles = "RemoveAllHoles";
        public const string RemoveAllGraves = "RemoveAllGraves";
        public const string SetLevelName = "SetLevelName";
        public const string SetZombiesIdle = "SetZombiesIdle";

        public const string CreatePlant = "CreatePlant";
        public const string CreatePlantCard = "CreatePlantCard";
        public const string CreatePlantVase = "CreatePlantVase";
        public const string CreateZombie = "CreateZombie";
        public const string CreateMindControlledZombie = "CreateMindControlledZombie";
        public const string CreateZombieVase = "CreateZombieVase";
        public const string CreateItem = "CreateItem";
        public const string CreatePassiveMeteorite = "CreatePassiveMeteorite";
        public const string CreateActiveMeteorite = "CreateActiveMeteorite";
        public const string CreateUltimateMeteorite = "CreateUltimateMeteorite";
        public const string CreateSolarMeteorite = "CreateSolarMeteorite";
        public const string NextWave = "NextWave";
        public const string SetAward = "SetAward";
        public const string DestroyAward = "DestroyAward";
        public const string ShowText = "ShowText";
        public const string StartMower = "StartMower";
        public const string CreateMower = "CreateMower";
        public const string SpawnPetGargantuar = "SpawnPetGargantuar";
        public const string SpawnPetFootball = "SpawnPetFootball";
        public const string SpawnPetSnowBoss = "SpawnPetSnowBoss";
        public const string SpawnPetJackbox = "SpawnPetJackbox";
        public const string SpawnPetDrown = "SpawnPetDrown";
        public const string SpawnPetHorse = "SpawnPetHorse";
        public const string SpawnPetImp = "SpawnPetImp";
        public const string SpawnPetKirov = "SpawnPetKirov";
        public const string ApplyAllPlantSkins = "ApplyAllPlantSkins";
        public const string ObtainAllPlantSkins = "ObtainAllPlantSkins";
        public const string ZombieSea = "ZombieSea";

        #endregion

        #region 游戏特性修改

        public const string SuperPresent = "SuperPresent";
        public const string UltimateRandomZombie = "UltimateRandomZombie";
        public const string PresentFastOpen = "PresentFastOpen";
        public const string LockPresent = "LockPresent";

        public const string SetZombieHP = "SetZombieHP";
        public const string SetFirstArmorHP = "SetFirstArmorHP";
        public const string SetSecondArmorHP = "SetSecondArmorHP";
        public const string SetBulletDamage = "SetBulletDamage";
        public const string LockBullet = "LockBullet";

        public const string NoIceRoad = "NoIceRoad";
        public const string NoHole = "NoHole";
        public const string ItemExistForever = "ItemExistForever";
        public const string JackboxNotExplode = "JackboxNotExplode";
        public const string GarlicDay = "GarlicDay";
        public const string UnlimitedSunlight = "UnlimitedSunlight";
        public const string UnlockRedCardPlants = "UnlockRedCardPlants";
        public const string PotSmashingFix = "PotSmashingFix";
        public const string DisableIceEffect = "DisableIceEffect";

        public const string FastShooting = "FastShooting";
        public const string HardPlant = "HardPlant";
        public const string ImmuneForceDeduct = "ImmuneForceDeduct";
        public const string CurseImmunity = "CurseImmunity";
        public const string CrushImmunity = "CrushImmunity";
        public const string TrampleImmunity = "TrampleImmunity";
        public const string PickaxeImmunity = "PickaxeImmunity";
        public const string UndeadBullet = "UndeadBullet";
        public const string OldObsidianBullet = "OldObsidianBullet";
        public const string UltimateSuperGatling = "UltimateSuperGatling";
        public const string HyponoEmperorNoCD = "HyponoEmperorNoCD";
        public const string MagnetNutUnlimited = "MagnetNutUnlimited";
        public const string MineNoCD = "MineNoCD";
        public const string ChomperNoCD = "ChomperNoCD";
        public const string CobCannonNoCD = "CobCannonNoCD";
        public const string PlantUpgrade = "PlantUpgrade";
        public const string SuperStarNoCD = "SuperStarNoCD";
        public const string LockWheat = "LockWheat";

        public const string ZombieDamageLimit = "ZombieDamageLimit";
        public const string ZombieSpeedMultiplier = "ZombieSpeedMultiplier";
        public const string ZombieAttackMultiplier = "ZombieAttackMultiplier";
        public const string ZombieBulletReflect = "ZombieBulletReflect";
        public const string ZombieStatusCoexist = "ZombieStatusCoexist";
        public const string ZombieImmuneFreeze = "ZombieImmuneFreeze";
        public const string ZombieImmuneCold = "ZombieImmuneCold";
        public const string ZombieImmuneButter = "ZombieImmuneButter";
        public const string ZombieImmunePoison = "ZombieImmunePoison";
        public const string ZombieImmuneJalaed = "ZombieImmuneJalaed";
        public const string ZombieImmuneEmbered = "ZombieImmuneEmbered";
        public const string ZombieImmuneKnockback = "ZombieImmuneKnockback";
        public const string ZombieImmuneMindControl = "ZombieImmuneMindControl";
        public const string ZombieImmuneDevour = "ZombieImmuneDevour";
        public const string ZombieImmuneAllDebuffs = "ZombieImmuneAllDebuffs";

        public const string ColumnPlanting = "ColumnPlanting";
        public const string SeedRain = "SeedRain";
        public const string AutoCutFruit = "AutoCutFruit";
        public const string RandomCard = "RandomCard";
        public const string ColumnGlove = "ColumnGlove";
        public const string UnlimitedCardSlots = "UnlimitedCardSlots";
        public const string RandomBullet = "RandomBullet";
        public const string AutoRhythmGame = "AutoRhythmGame";
        public const string StarUpBuff = "StarUpBuff";

        #endregion

        #region 诸神进化

        public const string GodEvolutionUnlimitedRefresh = "GodEvolutionUnlimitedRefresh";
        public const string GodEvolutionFreeUpgradeQuality = "GodEvolutionFreeUpgradeQuality";
        public const string GodEvolutionLucky = "GodEvolutionLucky";
        public const string GodEvolutionDifficulty = "GodEvolutionDifficulty";
        public const string GodEvolutionRefreshCount = "GodEvolutionRefreshCount";
        public const string GodEvolutionMaxPlantCount = "GodEvolutionMaxPlantCount";
        public const string GodEvolutionOptionCount = "GodEvolutionOptionCount";
        public const string GodEvolutionUpgradeBuffChance = "GodEvolutionUpgradeBuffChance";
        public const string GodEvolutionSuperUpgrade = "GodEvolutionSuperUpgrade";
        public const string GodEvolutionForceSuperQuality = "GodEvolutionForceSuperQuality";
        public const string GodEvolutionUncrashable = "GodEvolutionUncrashable";
        public const string GodEvolutionQualityWeightEnabled = "GodEvolutionQualityWeightEnabled";
        public const string GodEvolutionQualityDefault = "GodEvolutionQualityDefault";
        public const string GodEvolutionQualitySilver = "GodEvolutionQualitySilver";
        public const string GodEvolutionQualityGold = "GodEvolutionQualityGold";
        public const string GodEvolutionQualityDiamond = "GodEvolutionQualityDiamond";
        public const string GodEvolutionDamageMultiplier = "GodEvolutionDamageMultiplier";
        public const string GodEvolutionResetQuality = "GodEvolutionResetQuality";
        public const string GodEvolutionUnlockAll = "GodEvolutionUnlockAll";
        public const string GodEvolutionMultiSelectBuff = "GodEvolutionMultiSelectBuff";

        #endregion

        #region 精细出怪修改

        public const string GetZombiesList = "GetZombiesList";
        public const string ChangeZombiesList = "ChangeZombiesList";

        #endregion

        #region 布阵器

        public const string GetPlantFormationCode = "GetPlantFormationCode";
        public const string ApplyPlantFormation = "ApplyPlantFormation";

        public const string GetZombieFormationCode = "GetZombieFormationCode";
        public const string ApplyZombieFormation = "ApplyZombieFormation";

        public const string GetMixedFormationCode = "GetMixedFormationCode";
        public const string ApplyMixedFormation = "ApplyMixedFormation";

        public const string GetPotFormationCode = "GetPotFormationCode";
        public const string ApplyPotFormation = "ApplyPotFormation";

        public const string ApplyBattleFormation = "ApplyBattleFormation";
        public const string KillUpgrade = "KillUpgrade";
        public const string RandomUpgradeMode = "RandomUpgradeMode";

        #endregion

        #region 旅行词条修改

        public const string UpdateAdvBuff = "UpdateAdvBuff";
        public const string UpdateUltiBuff = "UpdateUltiBuff";
        public const string UpdateDebuff = "UpdateDebuff";
        public const string UpdateInvestBuff = "UpdateInvestBuff";
        public const string UpdateInGameAdvBuff = "UpdateInGameAdvBuff";
        public const string UpdateInGameUltiBuff = "UpdateInGameUltiBuff";
        public const string UpdateInGameDebuff = "UpdateInGameDebuff";
        public const string UpdateInGameInvestBuff = "UpdateInGameInvestBuff";
        public const string UpdateUnlockedPlant = "UpdateUnlockedPlant";
        public const string UpdateInGameUnlockedPlant = "UpdateInGameUnlockedPlant";
        public const string UpdateAllBuffs = "UpdateAllBuffs";

        #endregion

        #region 旗帜波词条修改

        public const string FlagWaveBuffsEnabled = "FlagWaveBuffsEnabled";
        public const string FlagWaveBuff = "FlagWaveBuff";

        #endregion

        #region 游戏内按键绑定

        public const string KeySpeedStop = "KeySpeedStop";
        public const string KeyShowGameInfo = "KeyShowGameInfo";
        public const string KeyTopMostCardBank = "KeyTopMostCardBank";
        public const string KeyRandomCard = "KeyRandomCard";
        public const string KeyAlmanacCreatePlant = "KeyAlmanacCreatePlant";
        public const string KeyAlmanacCreatePlantVase = "KeyAlmanacCreatePlantVase";
        public const string KeyAlmanacCreateZombie = "KeyAlmanacCreateZombie";
        public const string KeyAlmanacCreateZombieVase = "KeyAlmanacCreateZombieVase";
        public const string KeyAlmanacZombieMindCtrl = "KeyAlmanacZombieMindCtrl";

        #endregion

        #region 检索专区

        public const string PlaySound = "PlaySound";
        public const string PlayParticle = "PlayParticle";

        #endregion

        #region 局内存档/回溯

        public const string GetSnapshot = "GetSnapshot";
        public const string RestoreSnapshot = "RestoreSnapshot";

        #endregion
    }
}