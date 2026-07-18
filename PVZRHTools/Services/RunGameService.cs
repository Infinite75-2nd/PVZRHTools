using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using PVZRHTools.Models;
using ToolData;

namespace PVZRHTools.Services;

public class RunGameService(IModifierInfoService modifierInfoService) : IRunGameService
{
    public void RunGame(GameInstanceInfo info)
    {
        modifierInfoService.WriteBootConfig(info);
        Process.Start(new ProcessStartInfo
        {
            FileName = Path.Combine(info.GameRootPath, Paths.GameName),
            WorkingDirectory = info.GameRootPath
        });
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.TryShutdown();
    }
}

public interface IRunGameService
{
    void RunGame(GameInstanceInfo info);
}