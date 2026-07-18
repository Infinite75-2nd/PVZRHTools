using System.Collections.Generic;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace PVZRHTools.Models;

public partial class WaveZombiesItem : ReactiveObject
{
    [Reactive] public partial int Wave { get; set; }
    [Reactive] public partial Dictionary<int, int> Zombies { get; set; } = [];
    [Reactive] public partial bool IsCurrentWave { get; set; }
}