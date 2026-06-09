using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace PVZRHTools.Models;

[Serializable]
public partial class GameInstanceInfo : ReactiveObject
{
    [Reactive] public partial string GameRootPath { get; set; } = "";
    [Reactive] public partial string GameVersion { get; set; } = "";
    [Reactive] public partial bool BepInExEnabled { get; set; }
    [Reactive] public partial bool ModifierEnabled { get; set; }
    [Reactive] public partial ObservableCollection<ModInfo> Mods { get; set; } = [];
}

[Serializable]
public partial class ModInfo : ReactiveObject
{
    [Reactive] public partial string ModFilePath { get; set; } = "";
    [Reactive] public partial bool IsEnabled { get; set; }
}