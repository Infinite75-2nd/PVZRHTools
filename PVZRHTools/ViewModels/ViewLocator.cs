using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using PVZRHTools.Services;
using Splat;
using StaticViewLocator;

namespace PVZRHTools.ViewModels;

[StaticViewLocator]
public partial class ViewLocator : IDataTemplate
{
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }

    public Control? Build(object? data)
    {
        var _navigationService = Locator.Current.GetService<INavigationService>()!;
        if (data is null)
        {
            return null;
        }

        var viewModel = _navigationService.CurrentViewModel;
        var viewModelType = viewModel.GetType();
        if (!s_views.TryGetValue(viewModelType, out var viewType))
            return new TextBlock { Text = $"No view mapped for {viewModelType.Name}" };
        var func = TryGetFactory(viewModelType) ?? TryGetFactoryFromInterfaces(viewModelType);
        if (func is not null)
        {
            var view = func.Invoke();
            view.DataContext = viewModel;
            return view;
        }

        var missingView = TryGetMissingView(viewModelType) ?? TryGetMissingViewFromInterfaces(viewModelType);
        return missingView is not null
            ? new TextBlock { Text = missingView }
            : throw new Exception($"Unable to create view for type: {viewModelType}");
    }
}