using System.Diagnostics;
using ReactiveUI.SourceGenerators;
using ToolData;

namespace PVZRHTools.ViewModels;

public partial class AboutViewModel : ViewModelBase
{
    public string ProjectName => "PVZRHTools";
    public string ProjectDescription => "植物大战僵尸融合版修改器";
    public string ModifierVersion => Strings.ModifierVersion;
    public string ToolModVersion => Strings.ModifierVersion;
    public string GameVersion => Strings.GameVersion;
    public string LicenseName => "MIT License";
    public ModifierAuthorsViewModel Authors { get; } = new();

    [ReactiveCommand]
    public void OpenUrl(string? url)
    {
        if (!string.IsNullOrWhiteSpace(url))
            Process.Start("explorer.exe", url);
    }
}
