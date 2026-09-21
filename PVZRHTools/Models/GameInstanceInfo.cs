using System;
using System.Collections.ObjectModel;
using System.IO;
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
    [Reactive] public partial bool NeedsModifierInstall { get; set; }
    [Reactive] public partial ObservableCollection<ModInfo> Mods { get; set; } = [];
}

[Serializable]
public partial class ModInfo : ReactiveObject
{
    [Reactive] public partial string ModFilePath { get; set; } = "";
    [Reactive] public partial bool IsEnabled { get; set; }
    [Reactive] public partial bool IsVisible { get; set; } = true;

    public string DisplayName
    {
        get
        {
            var modPath = Path.GetFileNameWithoutExtension(ModFilePath);
            return Path.HasExtension(modPath) && Path.GetExtension(modPath) is ".dll"
                ? Path.GetFileNameWithoutExtension(modPath)
                : modPath;
        }
    }
}