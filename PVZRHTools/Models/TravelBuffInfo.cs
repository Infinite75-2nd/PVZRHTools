using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace PVZRHTools.Models;

public partial class TravelBuffInfo : ReactiveObject
{
    [Reactive] public partial int ID { get; set; }
    [Reactive] public partial int Level { get; set; }
    [Reactive] public partial string Name { get; set; }
    [Reactive] public partial bool Enabled { get; set; }
    [Reactive] public partial bool IsVisible { get; set; } = true;
}