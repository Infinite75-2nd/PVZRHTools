using System;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;
using ToolData;

namespace PVZRHTools.ViewModels;

public partial class MiscsViewModel : ViewModelBase
{
    public MiscsViewModel()
    {
        // 监听ShowFloatingWindow属性变化,同步到MainWindowViewModel
        this.WhenAnyValue(x => x.ShowFloatingWindow)
            .Subscribe(value =>
            {
                var mainVm = Locator.Current.GetService<MainWindowViewModel>();
                if (mainVm != null) mainVm.ShowFloatingWindow = value;
            });
    }

    [Reactive] public partial bool SaveAllSettings { get; set; }
    [Reactive] public partial bool ShowFloatingWindow { get; set; }

    public override void SaveSettings(SettingsData settings)
    {
        settings.SaveAllSettings = SaveAllSettings;
        settings.ShowFloatingWindow = ShowFloatingWindow;
    }

    public override void LoadSettings(SettingsData settings)
    {
        SaveAllSettings = settings.SaveAllSettings;
        ShowFloatingWindow = settings.ShowFloatingWindow;
    }
}