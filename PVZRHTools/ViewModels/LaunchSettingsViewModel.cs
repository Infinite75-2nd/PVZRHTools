using PVZRHTools.Models;
using ReactiveUI.SourceGenerators;

namespace PVZRHTools.ViewModels;

public partial class LaunchSettingsViewModel : ViewModelBase
{
    [Reactive] public partial GameInstanceInfo Info { get; set; }
}