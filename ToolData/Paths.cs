using System;
using System.IO;

namespace ToolData;

public static class Paths
{
    public const string BootConfigPath = @"BepInEx\config\ModifierBootConfig.json";

    public const string GameName = @"PlantsVsZombiesRH.exe";
    public const string GameAssemblyName = @"GameAssembly.dll";
    public const string ModifierExeName = @"PVZRHTools.exe";
    public const string PluginsPath = @"BepInEx\plugins";
    public const string DoorstopConfigName = @"doorstop_config.ini";
    public const string ConfigPath = @"BepInEx\config";
    public const string InitDataPath = @"BepInEx\config\InitData.json";
    public const string SaveSettingsPath = @"BepInEx\config\ModifierSettings.json";
    public const string HotKeysFileName = @"HotKeys.json";

    public const string GithubLink = @"https://github.com/Infinite75-2nd/PVZRHTools";

    public static string GameDataPath =>
        $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low\LanPiaoPiao\PlantsVsZombiesRH";

    public static string ModifierDataPath => $@"{GameDataPath}\Infinite75.PVZRHTools.ModifierData.json";
    public static readonly string LatestSnapshotPath = Path.Combine(ConfigPath, "LatestSnapshot.json");
}