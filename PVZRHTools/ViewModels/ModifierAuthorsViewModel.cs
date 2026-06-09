using System.Diagnostics;
using ReactiveUI.SourceGenerators;

namespace PVZRHTools.ViewModels;

public partial class ModifierAuthorsViewModel : ViewModelBase
{
    [ReactiveCommand]
    public void OpenSpaceInf75() => Process.Start("explorer.exe", "https://space.bilibili.com/672619350");

    [ReactiveCommand]
    public void OpenSpaceMKHkro1() => Process.Start("explorer.exe", "https://space.bilibili.com/3493111820978227");

    [ReactiveCommand]
    public void OpenSpaceCarefreeSongs712() =>
        Process.Start("explorer.exe", "https://space.bilibili.com/3537110030092294");

    [ReactiveCommand]
    public void OpenGithubInf75() => Process.Start("explorer.exe", "https://github.com/Infinite75-2nd");

    [ReactiveCommand]
    public void OpenGithubMKHkro1() => Process.Start("explorer.exe", "https://github.com/MKHkro1");

    [ReactiveCommand]
    public void OpenGithubCarefreeSongs712() => Process.Start("explorer.exe", "https://github.com/CarefreeSongs712");
}