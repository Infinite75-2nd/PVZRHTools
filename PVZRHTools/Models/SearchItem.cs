using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace PVZRHTools.Models;

public partial class SearchItem : ReactiveObject
{
    [Reactive] public partial int ID { get; set; }
    [Reactive] public partial string Name { get; set; } = "";
    [Reactive] public partial bool IsVisible { get; set; }
}