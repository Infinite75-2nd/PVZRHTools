using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
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
        Process.Start(Path.Combine(info.GameRootPath, Paths.GameName));
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.TryShutdown();
    }
}

public interface IRunGameService
{
    void RunGame(GameInstanceInfo info);
}