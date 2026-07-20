using System;
using System.IO;
using System.Text.Json;
using BepInEx;
using ToolData;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;
using Paths = ToolData.Paths;

namespace ToolMod.Components;

/// <summary>
/// 独立于其他设置的按键绑定存档读写。
/// HotKeys.json 存放在与 ModifierSettings.json 相同的目录下，
/// 每次模组启动时都会加载。
/// </summary>
public static class HotKeysLoader
{
    private static string SavePath => Path.Combine(BepInEx.Paths.GameRootPath, Paths.ConfigPath, Paths.HotKeysFileName);

    /// <summary>
    /// 从 HotKeys.json 读取按键绑定并写入 PatchDataCache。
    /// 文件不存在时使用默认值，不报错。
    /// </summary>
    public static void Load()
    {
        try
        {
            if (!File.Exists(SavePath)) return;

            var json = File.ReadAllText(SavePath);
            var data = JsonSerializer.Deserialize<HotKeysData>(json);
            if (data == null) return;

            KeySpeedStop = (KeyCode)data.KeySpeedStop;
            KeyShowGameInfo = (KeyCode)data.KeyShowGameInfo;
            KeyTopMostCardBank = (KeyCode)data.KeyTopMostCardBank;
            KeyRandomCard = (KeyCode)data.KeyRandomCard;
            KeyAlmanacCreatePlant = (KeyCode)data.KeyAlmanacCreatePlant;
            KeyAlmanacCreatePlantVase = (KeyCode)data.KeyAlmanacCreatePlantVase;
            KeyAlmanacCreateZombie = (KeyCode)data.KeyAlmanacCreateZombie;
            KeyAlmanacCreateZombieVase = (KeyCode)data.KeyAlmanacCreateZombieVase;
            KeyAlmanacZombieMindCtrl = (KeyCode)data.KeyAlmanacZombieMindCtrl;
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log.LogWarning($"加载 HotKeys.json 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 将 PatchDataCache 中的当前按键绑定写入 HotKeys.json。
    /// 每次按键重新绑定后自动调用。
    /// </summary>
    public static void Save()
    {
        try
        {
            var data = new HotKeysData
            {
                KeySpeedStop = (int)KeySpeedStop,
                KeyShowGameInfo = (int)KeyShowGameInfo,
                KeyTopMostCardBank = (int)KeyTopMostCardBank,
                KeyRandomCard = (int)KeyRandomCard,
                KeyAlmanacCreatePlant = (int)KeyAlmanacCreatePlant,
                KeyAlmanacCreatePlantVase = (int)KeyAlmanacCreatePlantVase,
                KeyAlmanacCreateZombie = (int)KeyAlmanacCreateZombie,
                KeyAlmanacCreateZombieVase = (int)KeyAlmanacCreateZombieVase,
                KeyAlmanacZombieMindCtrl = (int)KeyAlmanacZombieMindCtrl,
            };

            var dir = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(SavePath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log.LogWarning($"保存 HotKeys.json 失败: {ex.Message}");
        }
    }
}
