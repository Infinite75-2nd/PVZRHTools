using System;
using PVZRHTools.ViewModels;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace PVZRHTools.Services;

public partial class NavigationService : ReactiveObject, INavigationService
{
    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        var vm = Locator.Current.GetService<TViewModel>();
        CurrentViewModel =
            vm ?? throw new InvalidOperationException($"ViewModel {typeof(TViewModel)} not registered in Splat.");
    }

    public void NavigateTo(Type type)
    {
        if (!App.Bootstrap) Locator.Current.GetService<IDataSyncService>()?.Lock(true);
        var vm = Locator.Current.GetService(type) as ViewModelBase;
        CurrentViewModel =
            vm ?? throw new InvalidOperationException($"ViewModel {type.Name} not registered in Splat.");
        if (!App.Bootstrap) Locator.Current.GetService<IDataSyncService>()?.Lock(false);
    }

    [Reactive] public partial ViewModelBase CurrentViewModel { get; private set; }
}

public interface INavigationService
{
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    void NavigateTo(Type type);
    ViewModelBase CurrentViewModel { get; }
}