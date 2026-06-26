using System;
using System.Collections.Generic;

namespace ToolData;

[Serializable]
public struct SyncData
{
    public string Command { get; set; }
    public List<string> Parameters { get; set; }
}

[Serializable]
public struct ZombiesListData
{
    public int CurrentWave { get; set; }
    public Dictionary<int, Dictionary<int, int>> ZombiesList { get; set; }
}

[Serializable]
public struct ZombieSea
{
    public ZombieSea()
    {
    }

    public bool ZombieSeaEnabled { get; set; } = false;
    public int ZombieSeaCD { get; set; } = 40;
    public bool ZombieSeaLowEnabled { get; set; } = false;
    public List<int> ZombieSeaTypes { get; set; } = [];
}

[Serializable]
public struct PlantInfo
{
    public int Column { get; set; }
    public int ID { get; set; }
    public int LilyType { get; set; }
    public int Row { get; set; }
}

[Serializable]
public struct VaseInfo
{
    public int Col { get; set; }
    public int PlantType { get; set; }
    public int Row { get; set; }
    public int ZombieType { get; set; }
}

[Serializable]
public struct ZombieInfo
{
    public int ID { get; set; }
    public int Row { get; set; }
    public float X { get; set; }
}

[Serializable]
public struct SyncTravelBuffs
{
    public SyncTravelBuffs()
    {
    }

    public Dictionary<int, int> AdvBuffs { get; set; } = [];
    public Dictionary<int, int> UltiBuffs { get; set; } = [];
    public Dictionary<int, bool> Debuffs { get; set; } = [];
    public Dictionary<int, bool> InvestBuffs { get; set; } = [];
    public Dictionary<int, int> InGameAdvBuffs { get; set; } = [];
    public Dictionary<int, int> InGameUltiBuffs { get; set; } = [];
    public Dictionary<int, bool> InGameDebuffs { get; set; } = [];
    public Dictionary<int, bool> InGameInvestBuffs { get; set; } = [];
    public Dictionary<int, bool> UnlockedPlants { get; set; } = [];
    public Dictionary<int, bool> InGameUnlockedPlants { get; set; } = [];
}

[Serializable]
public struct FlagWaveBuff
{
    public int Wave { get; set; }
    public List<int> AdvBuffs { get; set; } = [];
    public List<int> UltiBuffs { get; set; } = [];
    public List<int> Debuffs { get; set; } = [];
    public List<int> InvestBuffs { get; set; } = [];
    public string Description { get; set; } = "";

    public FlagWaveBuff()
    {
    }
}

// --- 局内快照/回溯（环形缓冲实现） ---
public class Snapshot
{
    public string MixCodeCompressed { get; set; } = "";
    public int Sun { get; set; }
    public int Money { get; set; }
    public int Wave { get; set; }
    public int MaxWave { get; set; }
    public bool IsHugeWave { get; set; }
    public int BoardType { get; set; } // GameAPP.theBoardType (LevelType) as int
    public int SurvivalRound { get; set; } // Board.Instance.theCurrentSurvivalRound if available
    public int LevelNumber { get; set; } // 当前关卡编号
    public List<int> AdvOn { get; set; } = [];
    public List<int> UltiOn { get; set; } = [];
    public List<int> DebuffOn { get; set; } = [];
    public List<int> InvestOn { get; set; } = [];

    public List<VaseInfo> Vases { get; set; } = [];

    // 扩展：BoardTag/计时、网格物件、小推车
    public BoardTagSnapshot BoardTag { get; set; }
    public float TimeUntilNextWave { get; set; }
    public float WaveInterval { get; set; }
    public List<GridItemInfo> GridItems { get; set; } = [];

    public List<MowerInfo> Mowers { get; set; } = [];

    // 卡片/物品冷却（尽力匹配）
    public List<float> CardCDs { get; set; } = [];
    public List<float> CardFullCDs { get; set; } = [];
    public List<float> DroppedCDs { get; set; } = [];
    public List<float> DroppedFullCDs { get; set; } = [];

    public List<CardSnapshot> CardBank { get; set; } = [];

    // 单位生命值
    public List<PlantHealthInfo> PlantHealths { get; set; } = [];

    public List<ZombieHealthInfo> ZombieHealths { get; set; } = [];

    // 随机种子
    public int RandomSeed { get; set; }
    public DateTime CapturedAt { get; set; }
}

public struct BoardTagSnapshot
{
    public bool IsSeedRain { get; set; }
    public bool IsColumn { get; set; }
    public bool IsScaredyDream { get; set; }
}

public struct GridItemInfo
{
    public int Row { get; set; }
    public int Col { get; set; }
    public int ItemType { get; set; } // (int)GridItemType
    public int ExtraA { get; set; } // 可用于携带类型相关数据（如耐久、层数），暂未使用
    public int ExtraB { get; set; }
}

public struct MowerInfo
{
    public int Row { get; set; }
    public float X { get; set; }
}

public struct PlantHealthInfo
{
    public int Row { get; set; }
    public int Col { get; set; }
    public int PlantType { get; set; }
    public int Health { get; set; }
}

public struct ZombieHealthInfo
{
    public int Row { get; set; }
    public int ZombieType { get; set; }
    public int Health { get; set; }
}

public struct CardSnapshot
{
    public int PlantType { get; set; }
    public float CD { get; set; }
    public float FullCD { get; set; }
    public int SeedCost { get; set; }
}