using System;
using System.IO;
using Avalonia;
using Avalonia.Platform;
using PVZRHTools.Models;
using PVZRHTools.Utils;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using ToolData;

namespace PVZRHTools.Services;

public class GameBootstrapService : IGameBootstrapService
{
    private readonly IModifierInfoService _modifierInfoService;

    public GameBootstrapService(IModifierInfoService modifierInfoService)
    {
        _modifierInfoService = modifierInfoService;
    }

    public AddGamePathResult AddGameDirectory(string path) =>
        _modifierInfoService.TryAddGamePath(path);

    public bool NeedsBepInExInstallation(string gameRootPath) =>
        !File.Exists(Path.Combine(gameRootPath, "winhttp.dll")) ||
        !Directory.Exists(Path.Combine(gameRootPath, "BepInEx")) ||
        !Directory.Exists(Path.Combine(gameRootPath, "dotnet"));

    public bool IsBepInExInstalled(string gameRootPath) =>
        !NeedsBepInExInstallation(gameRootPath);

    public void InstallBepInEx(string gameRootPath)
    {
        using (var assetStream = AssetLoader.Open(new Uri("avares://PVZRHTools/Assets/BepInEx.7z")))
        using (var memoryStream = new MemoryStream())
        {
            assetStream.CopyTo(memoryStream);
            memoryStream.Position = 0;
            using var archive = SevenZipArchive.OpenArchive(memoryStream);
            archive.WriteToDirectory(gameRootPath,
                new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
        }

        using (var assetStream = AssetLoader.Open(new Uri("avares://PVZRHTools/Assets/InteropLibraries.7z")))
        using (var memoryStream = new MemoryStream())
        {
            assetStream.CopyTo(memoryStream);
            memoryStream.Position = 0;
            using var archive = SevenZipArchive.OpenArchive(memoryStream);
            archive.WriteToDirectory(gameRootPath,
                new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
        }
    }

    public bool NeedsModifierInstallation(string gameRootPath) =>
        !File.Exists(Path.Combine(gameRootPath, "PVZRHTools.exe"))||
        !File.Exists(Path.Combine(gameRootPath,Paths.PluginsPath, "ToolMod.dll"))||
        !File.Exists(Path.Combine(gameRootPath,Paths.PluginsPath, "ToolData.dll"));

    public bool IsModifierInstalled(string gameRootPath) =>
        !NeedsModifierInstallation(gameRootPath);

    public void InstallModifier(string gameRootPath)
    {
        if(!File.Exists(Path.Combine(gameRootPath, "PVZRHTools.exe"))) 
            File.Copy(Environment.ProcessPath!, Path.Combine(gameRootPath, "PVZRHTools.exe"), true);

        var pluginsPath = Path.Combine(gameRootPath, "BepInEx", "plugins");
        using var assetStream = AssetLoader.Open(new Uri("avares://PVZRHTools/Assets/plugins.7z"));
        using var memoryStream = new MemoryStream();
        assetStream.CopyTo(memoryStream);
        memoryStream.Position = 0;
        using var archive = SevenZipArchive.OpenArchive(memoryStream);
        archive.WriteToDirectory(pluginsPath,
            new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
    }

    public void EnableBepInEx(string gameRootPath) =>
        ToolUtils.SetBepInExEnabled(gameRootPath, true);

    public void DisableBepInEx(string gameRootPath) =>
        ToolUtils.SetBepInExEnabled(gameRootPath, false);
}

public interface IGameBootstrapService
{
    AddGamePathResult AddGameDirectory(string path);
    bool NeedsBepInExInstallation(string gameRootPath);
    bool IsBepInExInstalled(string gameRootPath);
    void InstallBepInEx(string gameRootPath);
    bool NeedsModifierInstallation(string gameRootPath);
    bool IsModifierInstalled(string gameRootPath);
    void InstallModifier(string gameRootPath);
    void EnableBepInEx(string gameRootPath);
    void DisableBepInEx(string gameRootPath);
}