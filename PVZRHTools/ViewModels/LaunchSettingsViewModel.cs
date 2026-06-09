using ReactiveUI.SourceGenerators;
using PVZRHTools.Models;

namespace PVZRHTools.ViewModels;

public partial class LaunchSettingsViewModel : ViewModelBase
{
    [Reactive] public partial GameInstanceInfo Info { get; set; }
}