using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Mono.Cecil;
using PVZRHTools.Models;
using Splat;
using ToolData;

namespace PVZRHTools.Services;

public class ModsManagementService : IModsManagementService
{
    private const string BasePluginTypeName = "BepInEx.Unity.IL2CPP.BasePlugin";
    private const string LoadBaseTypeName = "HengMingModsLib.Base.LoadBase";

    public void AddMods(GameInstanceInfo info, List<string> filePaths)
    {
        var pluginsFolder = Path.Combine(info.GameRootPath, Paths.PluginsPath);
        var hengMingModsFolder = Path.Combine(pluginsFolder, "Mods - HengMing");
        if (filePaths.Count is 0) return;

        try
        {
            Directory.CreateDirectory(pluginsFolder);
            Directory.CreateDirectory(hengMingModsFolder);
        }
        catch (UnauthorizedAccessException)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
                Locator.Current.GetService<INotificationService>()?.NotificationManager
                    ?.Show("权限不足无法操作，请尝试以管理员身份启动", NotificationType.Error));
            return;
        }

        foreach (var filePath in filePaths)
        {
            if (!File.Exists(filePath)) continue;

            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            try
            {
                switch (extension)
                {
                    case ".dll":
                        ProcessDllFile(filePath, Path.GetFileName(filePath), pluginsFolder, hengMingModsFolder);
                        break;
                    case ".zip":
                        ProcessZipFile(filePath, pluginsFolder, hengMingModsFolder);
                        break;
                }
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                    Locator.Current.GetService<INotificationService>()?.NotificationManager
                        ?.Show($"处理文件失败: {Path.GetFileName(filePath)}\n{ex.Message}",
                            NotificationType.Error));
            }
        }
    }

    private void ProcessDllFile(string dllPath, string targetFileName, string pluginsFolder, string hengMingModsFolder)
    {
        AssemblyDefinition assembly;
        try
        {
            assembly = AssemblyDefinition.ReadAssembly(dllPath);
        }
        catch
        {
            // 不是有效的 .NET 程序集，跳过
            return;
        }

        using (assembly)
        {
            var hasBasePlugin = CheckTypeInheritance(assembly, BasePluginTypeName);
            var hasLoadBase = CheckTypeInheritance(assembly, LoadBaseTypeName);

            var fileName = targetFileName;

            if (hasBasePlugin)
            {
                var destPath = Path.Combine(pluginsFolder, fileName);
                File.Copy(dllPath, destPath, overwrite: true);
            }
            else if (hasLoadBase)
            {
                var destPath = Path.Combine(hengMingModsFolder, fileName);
                File.Copy(dllPath, destPath, overwrite: true);
            }
        }
    }

    private void ProcessZipFile(string zipPath, string pluginsFolder, string hengMingModsFolder)
    {
        using var archive = ZipFile.OpenRead(zipPath);
        foreach (var entry in archive.Entries)
        {
            if (!entry.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                continue;

            var tempPath = Path.GetTempFileName() + ".dll";
            try
            {
                entry.ExtractToFile(tempPath, overwrite: true);
                ProcessDllFile(tempPath, Path.GetFileName(entry.FullName), pluginsFolder, hengMingModsFolder);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }

    private bool CheckTypeInheritance(AssemblyDefinition assembly, string targetTypeName)
        => assembly.MainModule.Types.Any(type => IsTypeInheritedFrom(type, targetTypeName));

    private bool IsTypeInheritedFrom(TypeDefinition type, string targetTypeName)
    {
        if (!type.IsClass || type.IsAbstract)
            return false;

        var baseType = type.BaseType;
        while (baseType != null)
        {
            var fullName = baseType.FullName;
            if (fullName == targetTypeName)
                return true;

            try
            {
                var resolved = baseType.Resolve();
                baseType = resolved?.BaseType;
            }
            catch
            {
                break;
            }
        }

        return false;
    }

    public void ToggleMod(GameInstanceInfo info, ModInfo mod)
    {
        var oldPath = mod.ModFilePath;
        if (!File.Exists(oldPath)) return;

        var newPath = mod.IsEnabled
            ? (oldPath.EndsWith(".disabled") ? oldPath[..^".disabled".Length] : oldPath)
            : (oldPath.EndsWith(".dll") ? oldPath + ".disabled" : oldPath);

        if (oldPath != newPath)
        {
            try
            {
                File.Move(oldPath, newPath);
            }
            catch (UnauthorizedAccessException)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                    Locator.Current.GetService<INotificationService>()?.NotificationManager
                        ?.Show("权限不足无法操作，请尝试以管理员身份启动", NotificationType.Error));
            }

            mod.ModFilePath = newPath;
            SyncModsInfo(info);
        }
    }

    public GameInstanceInfo SyncModsInfo(GameInstanceInfo info)
    {
        var pluginsFolder = Path.Combine(info.GameRootPath, "BepInEx", "plugins");
        var hengMingModsFolder = Path.Combine(pluginsFolder, "Mods - HengMing");

        info.Mods.Clear();

        // 扫描 BepInEx\plugins 文件夹
        ScanModFiles(pluginsFolder, info.Mods);

        // 扫描 BepInEx\plugins\Mods - HengMing 文件夹
        ScanModFiles(hengMingModsFolder, info.Mods);

        var sorted = info.Mods.OrderBy(m => Path.GetFileName(m.ModFilePath)).ToList();
        info.Mods.Clear();
        foreach (var mod in sorted)
            info.Mods.Add(mod);

        return info;
    }

    private void ScanModFiles(string folderPath, ObservableCollection<ModInfo> modsList)
    {
        if (!Directory.Exists(folderPath))
            return;

        // 获取所有 .dll 文件
        var dllFiles = Directory.GetFiles(folderPath, "*.dll", SearchOption.TopDirectoryOnly);
        foreach (var filePath in dllFiles)
        {
            modsList.Add(new ModInfo
            {
                ModFilePath = filePath,
                IsEnabled = true
            });
        }

        // 获取所有 .disabled 文件
        var disabledFiles = Directory.GetFiles(folderPath, "*.disabled", SearchOption.TopDirectoryOnly);
        foreach (var filePath in disabledFiles)
        {
            modsList.Add(new ModInfo
            {
                ModFilePath = filePath,
                IsEnabled = false
            });
        }
    }
}

public interface IModsManagementService
{
    void AddMods(GameInstanceInfo info, List<string> filePaths);
    void ToggleMod(GameInstanceInfo info, ModInfo mod);
    GameInstanceInfo SyncModsInfo(GameInstanceInfo info);
}