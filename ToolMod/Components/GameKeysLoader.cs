using System;
using System.IO;
using System.Text.Json;
using BepInEx;
using ToolData;
using UnityEngine;
using Paths = ToolData.Paths;

namespace ToolMod.Components;

/// <summary>
/// 独立于修改器按键的游戏原版按键存档读写。
/// GameKeys.json 存放在与 HotKeys.json 相同的目录下，
/// 每次模组启动时都会加载并直接写入 KeyCodeManager 的静态字段，
/// 从而使游戏本身的热键（铁铲、手套、锤子等）立即生效。
/// </summary>
public static class GameKeysLoader
{
    private static string SavePath => Path.Combine(BepInEx.Paths.GameRootPath, Paths.ConfigPath, Paths.GameKeysFileName);

    /// <summary>
    /// 从 GameKeys.json 读取游戏原版按键并写入 KeyCodeManager。
    /// 文件不存在时不做任何修改（保留游戏默认按键）。
    /// </summary>
    public static void Load()
    {
        try
        {
            if (!File.Exists(SavePath)) return;

            var json = File.ReadAllText(SavePath);
            var data = JsonSerializer.Deserialize<GameKeysData>(json);
            if (data == null) return;

            Apply(data);
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log.LogWarning($"加载 GameKeys.json 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 将当前 KeyCodeManager 的游戏原版按键整体写入 GameKeys.json。
    /// 每次按键重新绑定后自动调用，未改动的按键也会一并保存当前值。
    /// </summary>
    public static void Save()
    {
        try
        {
            var data = new GameKeysData
            {
                Shovel = (int)KeyCodeManager.Shovel,
                Glove = (int)KeyCodeManager.Glove,
                SlowTrigger = (int)KeyCodeManager.SlowTrigger,
                Hammer = (int)KeyCodeManager.Hammer,
                Wheel = (int)KeyCodeManager.Wheel,
                ShowPlantHealth = (int)KeyCodeManager.ShowPlantHealth,
                ShowZombieHealth = (int)KeyCodeManager.ShowZombieHealth,
                UseGoldBean = (int)KeyCodeManager.UseGoldBean,
                CheckPlantAlmanac = (int)KeyCodeManager.CheckPlantAlmanac,
                ShowBulletDamage = (int)KeyCodeManager.ShowBulletDamage,
                ZombieGlove = (int)KeyCodeManager.ZombieGlove,
                LookPlantData = (int)KeyCodeManager.LookPlantData,
                FullScreen = (int)KeyCodeManager.FullScreen,
                NormalScreen = (int)KeyCodeManager.NormalScreen,
                Ra2Sound = (int)KeyCodeManager.Ra2Sound,
                HideUI = (int)KeyCodeManager.HideUI,
            };

            var dir = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(SavePath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log.LogWarning($"保存 GameKeys.json 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 将存档数据写入 KeyCodeManager 的对应静态字段。
    /// 保存时是整体快照，因此这里无条件应用全部字段（包括 KeyCode.None）。
    /// </summary>
    private static void Apply(GameKeysData data)
    {
        KeyCodeManager.Shovel = (KeyCode)data.Shovel;
        KeyCodeManager.Glove = (KeyCode)data.Glove;
        KeyCodeManager.SlowTrigger = (KeyCode)data.SlowTrigger;
        KeyCodeManager.Hammer = (KeyCode)data.Hammer;
        KeyCodeManager.Wheel = (KeyCode)data.Wheel;
        KeyCodeManager.ShowPlantHealth = (KeyCode)data.ShowPlantHealth;
        KeyCodeManager.ShowZombieHealth = (KeyCode)data.ShowZombieHealth;
        KeyCodeManager.UseGoldBean = (KeyCode)data.UseGoldBean;
        KeyCodeManager.CheckPlantAlmanac = (KeyCode)data.CheckPlantAlmanac;
        KeyCodeManager.ShowBulletDamage = (KeyCode)data.ShowBulletDamage;
        KeyCodeManager.ZombieGlove = (KeyCode)data.ZombieGlove;
        KeyCodeManager.LookPlantData = (KeyCode)data.LookPlantData;
        KeyCodeManager.FullScreen = (KeyCode)data.FullScreen;
        KeyCodeManager.NormalScreen = (KeyCode)data.NormalScreen;
        KeyCodeManager.Ra2Sound = (KeyCode)data.Ra2Sound;
        KeyCodeManager.HideUI = (KeyCode)data.HideUI;
    }
}
