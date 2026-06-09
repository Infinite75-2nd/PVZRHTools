using System;
using PVZRHTools.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;


namespace PVZRHTools.ViewModels;

public partial class MenuItemViewModel(INavigationService _navigationService) : ViewModelBase
{
    [Reactive] public partial bool IsSeparator { get; set; }
    [Reactive] public partial string? MenuHeader { get; set; }
    [Reactive] public partial Type PageType { get; set; }

    [ReactiveCommand]
    public void Navigate() => _navigationService.NavigateTo(PageType);
}