using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using PVZRHTools.Models;
using PVZRHTools.Utils;
using Splat;
using ToolData;

namespace PVZRHTools.Services;

public class ModifierInfoService : IModifierInfoService
{
    public ModifierInfo ModifierInfo { get; set; }
    private IModsManagementService _modsManagementService { get; set; }

    public void Save(ObservableCollection<GameInstanceInfo> gameInstances)
    {
        Directory.CreateDirectory(Paths.GameDataPath);
        var writer = new StreamWriter(File.Open(Paths.ModifierDataPath, FileMode.OpenOrCreate));
        writer.Write(JsonSerializer.Serialize(ModifierInfo, JsonSGC.Default.ModifierInfo));
        writer.Flush();
        writer.Close();
    }

    public ObservableCollection<GameInstanceInfo> InitGameInstanceInfos()
    {
        var infos = new ObservableCollection<GameInstanceInfo>();

        foreach (var gamePath in ModifierInfo.GamePaths.Where(gamePath =>
                     File.Exists(Path.Combine(gamePath, Paths.GameName)) &&
                     File.Exists(Path.Combine(gamePath, Paths.GameAssemblyName))))
        {
            var info = new GameInstanceInfo();
            info.GameRootPath = gamePath;
            info.GameVersion = Strings.GetGameVersion(
                Convert.ToHexStringLower(
                    SHA256.HashData(
                        new MemoryStream(
                            File.ReadAllBytes(
                                Path.Combine(gamePath, Paths.GameAssemblyName))))));
            info.BepInExEnabled = ToolUtils.GetBepInExEnabled(gamePath);
            info.ModifierEnabled = GetModifierEnabled(gamePath);
            _modsManagementService.SyncModsInfo(info);
            infos.Add(info);
        }

        return infos;
    }

    public bool GetModifierEnabled(string gameRootPath)
    {
        if (File.Exists(Path.Combine(gameRootPath, Paths.BootConfigPath)))
        {
            try
            {
                return JsonSerializer.Deserialize(File.ReadAllText(Path.Combine(gameRootPath, Paths.BootConfigPath)),
                    JsonSGC.Default.BootConfig).ModifierEnabled;
            }
            catch
            {
                WriteBootConfig(gameRootPath, true);
                return true;
            }
        }

        WriteBootConfig(gameRootPath, true);
        return true;
    }

    public void WriteBootConfig(string gameRootPath, bool modifierEnabled)
    {
        try
        {
            Directory.CreateDirectory(Path.Combine(gameRootPath, Paths.ConfigPath));
            File.Create(Path.Combine(gameRootPath, Paths.BootConfigPath)).Close();
            File.WriteAllText(Path.Combine(gameRootPath, Paths.BootConfigPath), JsonSerializer.Serialize(
                new BootConfig()
                {
                    GameVersion = Strings.GameVersion,
                    ModifierEnabled = modifierEnabled,
                    ModifierPath = Environment.ProcessPath!
                }, JsonSGC.Default.BootConfig));
        }
        catch (UnauthorizedAccessException)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
                Locator.Current.GetService<INotificationService>()?.NotificationManager
                    ?.Show("权限不足无法操作，请尝试以管理员身份启动", NotificationType.Error));
        }
    }

    public AddGamePathResult TryAddGamePath(string gameRootPath)
    {
        if (!ToolUtils.ValidateGameDirectory(gameRootPath)) return AddGamePathResult.InvalidDirectory;
        if (ModifierInfo.GamePaths.Contains(gameRootPath)) return AddGamePathResult.AlreadyExists;
        ModifierInfo.GamePaths.Add(gameRootPath);
        SaveModifierInfo();
        return AddGamePathResult.Added;
    }

    public void RemoveGamePath(string gameRootPath)
    {
        ModifierInfo.GamePaths.Remove(gameRootPath);
        SaveModifierInfo();
    }

    public void ValidateAndCleanGamePaths()
    {
        ModifierInfo.GamePaths.RemoveAll(path => !ToolUtils.ValidateGameDirectory(path));
    }

    public void SaveModifierInfo()
    {
        Directory.CreateDirectory(Paths.GameDataPath);
        File.WriteAllText(Paths.ModifierDataPath,
            JsonSerializer.Serialize(ModifierInfo, JsonSGC.Default.ModifierInfo));
    }

    public List<string> GetOutdatedGamePaths()
    {
        var outdatedPaths = new List<string>();
        var currentVersion = Strings.ModifierVersion;

        foreach (var gamePath in ModifierInfo.GamePaths)
        {
            var gameAssemblyPath = Path.Combine(gamePath, Paths.GameAssemblyName);
            if (!File.Exists(gameAssemblyPath)) continue;

            var hash = Convert.ToHexStringLower(
                SHA256.HashData(File.ReadAllBytes(gameAssemblyPath)));
            var gameVersion = Strings.GetGameVersion(hash);
            if (gameVersion != Strings.GameVersion) continue;

            var modifierExePath = Path.Combine(gamePath, Paths.ModifierExeName);
            if (!File.Exists(modifierExePath)) continue;

            var fileVersion = FileVersionInfo.GetVersionInfo(modifierExePath).FileVersion;
            if (fileVersion == null) continue;

            if (Version.TryParse(fileVersion, out var fileVer) &&
                Version.TryParse(currentVersion, out var curVer) &&
                fileVer < curVer)
            {
                outdatedPaths.Add(gamePath);
            }
        }

        ModifierInfo.ModifierVersion = currentVersion;
        return outdatedPaths;
    }

    public void WriteBootConfig(GameInstanceInfo info) =>
        WriteBootConfig(info.GameRootPath, info.ModifierEnabled);

    public ModifierInfoService(IModsManagementService modsManagementService)
    {
        _modsManagementService = modsManagementService;
        if (Directory.Exists(Paths.GameDataPath))
        {
            try
            {
                ModifierInfo = File.Exists(Paths.ModifierDataPath)
                    ? JsonSerializer.Deserialize(File.ReadAllText(Paths.ModifierDataPath),
                        JsonSGC.Default.ModifierInfo)!
                    : new ModifierInfo();
            }
            catch
            {
                ModifierInfo = new ModifierInfo();
            }
        }
        else
        {
            Directory.CreateDirectory(Paths.GameDataPath);
            ModifierInfo = new ModifierInfo();
        }
    }
}

public enum AddGamePathResult
{
    InvalidDirectory,
    AlreadyExists,
    Added
}

public interface IModifierInfoService
{
    ModifierInfo ModifierInfo { get; set; }
    void Save(ObservableCollection<GameInstanceInfo> gameInstances);
    ObservableCollection<GameInstanceInfo> InitGameInstanceInfos();
    void WriteBootConfig(string gameRootPath, bool modifierEnabled);
    void WriteBootConfig(GameInstanceInfo info);
    AddGamePathResult TryAddGamePath(string gameRootPath);
    void RemoveGamePath(string gameRootPath);
    void ValidateAndCleanGamePaths();
    void SaveModifierInfo();
    List<string> GetOutdatedGamePaths();
}