using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using PVZRHTools.Utils;
using ToolData;

namespace PVZRHTools.Services;

public class InitDataService(string gamePath) : IInitDataService
{
    public InitData InitData { get; } = new();

    public void Initialize()
    {
        try
        {
            var newInitData = JsonSerializer.Deserialize(File.ReadAllText(Path.Combine(gamePath, Paths.InitDataPath)),
                JsonSGC.Default.InitData);
            if (newInitData is null)
                return;
            foreach (var plant in newInitData.Plants)
            {
                InitData.Plants.TryAdd(plant.Key, plant.Value);
            }

            foreach (var zombie in newInitData.Zombies)
            {
                InitData.Zombies.TryAdd(zombie.Key, zombie.Value);
            }

            foreach (var bullet in newInitData.Bullets)
            {
                InitData.Bullets.TryAdd(bullet.Key, bullet.Value);
            }

            foreach (var firstArmor in newInitData.FirstArmors)
            {
                InitData.FirstArmors.TryAdd(firstArmor.Key, firstArmor.Value);
            }

            foreach (var secondArmor in newInitData.SecondArmors)
            {
                InitData.SecondArmors.TryAdd(secondArmor.Key, secondArmor.Value);
            }

            foreach (var advBuff in newInitData.AdvBuffs)
            {
                InitData.AdvBuffs.TryAdd(advBuff.Key, advBuff.Value);
            }

            foreach (var ultiBuff in newInitData.UltiBuffs)
            {
                InitData.UltiBuffs.TryAdd(ultiBuff.Key, ultiBuff.Value);
            }

            foreach (var debuff in newInitData.Debuffs)
            {
                InitData.Debuffs.TryAdd(debuff.Key, debuff.Value);
            }

            foreach (var investBuff in newInitData.InvestBuffs)
            {
                InitData.InvestBuffs.TryAdd(investBuff.Key, investBuff.Value);
            }

            foreach (var plant in newInitData.UnlockablePlants)
            {
                InitData.UnlockablePlants.TryAdd(plant.Key, plant.Value);
            }
        }
        catch
        {
        }
    }


    public Dictionary<int, string> PlantsWithRandom
    {
        get
        {
            var dictionary = new Dictionary<int, string>
            {
                { -2, "不选择" },
                { -1, "随机" }
            };
            return dictionary.Concat(InitData.Plants).ToDictionary(x => x.Key, x => x.Value);
        }
    }

    public Dictionary<int, string> ZombiesWithRandom
    {
        get
        {
            var dictionary = new Dictionary<int, string>
            {
                { -2, "不选择" },
                { -1, "随机" }
            };
            return dictionary.Concat(InitData.Zombies).ToDictionary(x => x.Key, x => x.Value);
        }
    }

    public Dictionary<int, string> BulletsWithRandom
    {
        get
        {
            var dictionary = new Dictionary<int, string>
            {
                { -2, "不选择" },
                { -1, "随机" }
            };
            return dictionary.Concat(InitData.Bullets).ToDictionary(x => x.Key, x => x.Value);
        }
    }

    public Dictionary<int, string> Items => new()
    {
        { 0, "肥料 Fertilizer" },
        { 1, "铁桶 Bucket" },
        { 2, "橄榄头盔 Helmet" },
        { 3, "小丑礼盒 Jackbox" },
        { 4, "镐子 Pickaxe" },
        { 5, "机甲碎片 Machine" },
        { 6, "超级机甲碎片 SuperMachine" },
        { 7, "花园植物礼盒 GardenPresent" },
        { 8, "超时空碎片 PortalHeart" },
        { 64 + 0, "阳光 Sun" },
        { 64 + 1, "大阳光 BigSun" },
        { 64 + 2, "小阳光 SmallSun" },
        //{64 + 4,"铁桶 Bucket"},
        //{64 + 6,"橄榄头盔 Helmet"},
        //{64 + 7,"小丑礼盒 Jackbox"},
        //{64 + 8,"镐子 Pickaxe"},
        { 64 + 13, "小阳光 LittleSun" },
        { 64 + 34, "银币 SilverCoin" },
        { 64 + 35, "金币 GoldCoin" },
        { 64 + 36, "钻石 DiamondCoin" },
        //{64 + 37," Bean"},
        { 64 + 38, "小银币 SmallSilverCoin" },
        { 64 + 39, "小金币 SmallGoldCoin" },
        //{64 + 41,"机甲碎片 Machine"},
        { 64 + 42, "梯子 Portal" }
    };
}

public interface IInitDataService
{
    InitData InitData { get; }
    public Dictionary<int, string> PlantsWithRandom { get; }
    public Dictionary<int, string> ZombiesWithRandom { get; }
    public Dictionary<int, string> BulletsWithRandom { get; }
    public Dictionary<int, string> Items { get; }
    public void Initialize();
}