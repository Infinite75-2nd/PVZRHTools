using Avalonia;
using ReactiveUI.Avalonia;
using Splat.Builder;
using System;
using System.Diagnostics;
using System.Threading;
using Avalonia.Input;
using PVZRHTools.Services;
using ToolData;
using AppBuilder = Avalonia.AppBuilder;

namespace PVZRHTools;

public static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        using var mutex = new Mutex(true, Program.Mutex, out bool createdNew);
        if (createdNew)
        {
            App.Bootstrap = !(args.Length is 2 && args[0] is Strings.RunModifierArgument);
            if (!App.Bootstrap)
            {
                App.GamePath = args[1];
            }

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        else
        {
            ActivateExistingWindow();
            Environment.Exit(0);
        }
    }

    private static void ActivateExistingWindow()
    {
        using var currentProcess = Process.GetCurrentProcess();
        var processes = Process.GetProcessesByName(currentProcess.ProcessName);
        foreach (var process in processes)
        {
            if (process.Id == currentProcess.Id || process.MainWindowHandle == IntPtr.Zero) continue;
            NativeMethods.SetForegroundWindow(process.MainWindowHandle);
            if (NativeMethods.IsIconic(process.MainWindowHandle))
            {
                NativeMethods.ShowWindow(process.MainWindowHandle, 1); // 恢复窗口
            }

            break;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI(builder =>
            {
                builder.UsingSplatBuilder(appBuilder =>
                    appBuilder.UsingModule<IModule>(App.Bootstrap
                        ? new BootstrapServicesModule()
                        : new ModifierServicesModule()));
            })
            .RegisterReactiveUIViewsFromEntryAssembly();

    public const string Mutex = "Infinite75.PVZRHTools";
}