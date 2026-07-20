using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using PVZRHTools.Models;
using PVZRHTools.Utils;
using ToolData;

namespace PVZRHTools.Services;

public class ModifierInfoService : IModifierInfoService
{
    public ModifierInfo ModifierInfo { get; set; }
    private IModsManagementService _modsManagementService { get; set; }
    private IGameBootstrapService _gameBootstrapService { get; set; }

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
            info.ModifierEnabled = _gameBootstrapService.IsModifierEnabled(gamePath);
            info.NeedsModifierInstall = _gameBootstrapService.NeedsModifierInstallation(gamePath);
            _modsManagementService.SyncModsInfo(info);
            infos.Add(info);
        }

        return infos;
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
        var outdatedPaths = ModifierInfo.GamePaths.Where(p => _gameBootstrapService.IsGamePathOutdated(p)).ToList();
        ModifierInfo.ModifierVersion = Strings.ModifierVersion;
        return outdatedPaths;
    }

    public ModifierInfoService(IModsManagementService modsManagementService, IGameBootstrapService gameBootstrapService)
    {
        _modsManagementService = modsManagementService;
        _gameBootstrapService = gameBootstrapService;
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
    AddGamePathResult TryAddGamePath(string gameRootPath);
    void RemoveGamePath(string gameRootPath);
    void ValidateAndCleanGamePaths();
    void SaveModifierInfo();
    List<string> GetOutdatedGamePaths();
}