using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Platform;
using Avalonia.Threading;
using PVZRHTools.Models;
using PVZRHTools.Utils;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using Splat;
using ToolData;
using Ursa.Controls;

namespace PVZRHTools.Services;

public class GameBootstrapService : IGameBootstrapService
{
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
        !File.Exists(Path.Combine(gameRootPath, "PVZRHTools.exe")) ||
        !File.Exists(Path.Combine(gameRootPath, Paths.PluginsPath, "ToolMod.dll")) ||
        !File.Exists(Path.Combine(gameRootPath, Paths.PluginsPath, "ToolData.dll"));

    public bool IsModifierInstalled(string gameRootPath) =>
        !NeedsModifierInstallation(gameRootPath);

    public bool TryInstallModifier(string gameRootPath, out string? errorMessage)
    {
        try
        {
            InstallModifier(gameRootPath);
            errorMessage = null;
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    public void InstallModifier(string gameRootPath)
    {
        try
        {
            File.Copy(Environment.ProcessPath!, Path.Combine(gameRootPath, "PVZRHTools.exe"), true);
        }
        catch
        {
        }

        var pluginsPath = Path.Combine(gameRootPath, "BepInEx", "plugins");
        using var assetStream = AssetLoader.Open(new Uri("avares://PVZRHTools/Assets/plugins.7z"));
        using var memoryStream = new MemoryStream();
        assetStream.CopyTo(memoryStream);
        memoryStream.Position = 0;
        using var archive = SevenZipArchive.OpenArchive(memoryStream);
        archive.WriteToDirectory(pluginsPath,
            new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
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

    public void WriteBootConfig(GameInstanceInfo info)
    {
        WriteBootConfig(info.GameRootPath, info.ModifierEnabled);
        ToolUtils.SetBepInExEnabled(info.GameRootPath, info.BepInExEnabled);
    }

    public bool IsModifierEnabled(string gameRootPath)
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

    public void EnableBepInEx(string gameRootPath) =>
        ToolUtils.SetBepInExEnabled(gameRootPath, true);

    public void DisableBepInEx(string gameRootPath) =>
        ToolUtils.SetBepInExEnabled(gameRootPath, false);

    public async Task ProcessOutdatedModifierAsync(string gamePath)
    {
        var dirName = new DirectoryInfo(gamePath).Name;
        var shouldUpdate = await OverlayMessageBox.ShowAsync(
            $"检测到游戏实例 \"{dirName}\" 中的修改器版本低于当前版本，是否更新？",
            "更新修改器",
            icon: MessageBoxIcon.Question,
            button: MessageBoxButton.YesNo);
        if (shouldUpdate != MessageBoxResult.Yes) return;

        if (TryInstallModifier(gamePath, out var error))
        {
            Locator.Current.GetService<INotificationService>()?.NotificationManager?.Show(
                $"\"{dirName}\" 的修改器已更新完成", NotificationType.Success);
        }
        else
        {
            await OverlayMessageBox.ShowAsync(
                $"更新修改器失败：{error}",
                "更新失败",
                icon: MessageBoxIcon.Error,
                button: MessageBoxButton.OK);
        }
    }

    public bool IsGamePathOutdated(string gamePath)
    {
        var currentVersion = Strings.ModifierVersion;

        var gameAssemblyPath = Path.Combine(gamePath, Paths.GameAssemblyName);
        if (!File.Exists(gameAssemblyPath)) return false;

        var hash = Convert.ToHexStringLower(
            SHA256.HashData(File.ReadAllBytes(gameAssemblyPath)));
        var gameVersion = Strings.GetGameVersion(hash);
        if (gameVersion != Strings.GameVersion) return false;

        // 检测 PVZRHTools.exe 版本
        var modifierExePath = Path.Combine(gamePath, Paths.ModifierExeName);
        bool exeOutdated = false;
        if (File.Exists(modifierExePath))
        {
            var exeVersion = FileVersionInfo.GetVersionInfo(modifierExePath).FileVersion;
            if (exeVersion != null &&
                Version.TryParse(exeVersion, out var exeVer) &&
                Version.TryParse(currentVersion, out var curVer) &&
                exeVer < curVer)
            {
                exeOutdated = true;
            }
        }

        // 检测 ToolMod.dll 版本
        var toolModPath = Path.Combine(gamePath, Paths.PluginsPath, "ToolMod.dll");
        bool dllOutdated = false;
        if (File.Exists(toolModPath))
        {
            var dllVersion = FileVersionInfo.GetVersionInfo(toolModPath).FileVersion;
            if (dllVersion != null &&
                Version.TryParse(dllVersion, out var dllVer) &&
                Version.TryParse(currentVersion, out var curVer2) &&
                dllVer < curVer2)
            {
                dllOutdated = true;
            }
        }

        return exeOutdated || dllOutdated;
    }

}

public interface IGameBootstrapService
{
    bool NeedsBepInExInstallation(string gameRootPath);
    bool IsBepInExInstalled(string gameRootPath);
    void InstallBepInEx(string gameRootPath);
    bool NeedsModifierInstallation(string gameRootPath);
    bool IsModifierInstalled(string gameRootPath);
    void InstallModifier(string gameRootPath);
    bool TryInstallModifier(string gameRootPath, out string? errorMessage);
    void WriteBootConfig(string gameRootPath, bool modifierEnabled);
    void WriteBootConfig(GameInstanceInfo info);
    bool IsModifierEnabled(string gameRootPath);
    bool IsGamePathOutdated(string gamePath);
    Task ProcessOutdatedModifierAsync(string gamePath);
    void EnableBepInEx(string gameRootPath);
    void DisableBepInEx(string gameRootPath);
}
