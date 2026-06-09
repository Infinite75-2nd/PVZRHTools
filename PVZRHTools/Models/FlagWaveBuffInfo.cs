using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace PVZRHTools.Models;

public partial class FlagWaveBuffInfo : ReactiveObject
{
    [Reactive] public partial int Wave { get; set; }
    [Reactive] public partial ObservableCollection<KeyValuePair<int, string>> AdvBuffs { get; set; } = [];
    [Reactive] public partial ObservableCollection<KeyValuePair<int, string>> UltiBuffs { get; set; } = [];
    [Reactive] public partial ObservableCollection<KeyValuePair<int, string>> Debuffs { get; set; } = [];
    [Reactive] public partial ObservableCollection<KeyValuePair<int, string>> InvestBuffs { get; set; } = [];
    [Reactive] public partial string Description { get; set; } = "";
}