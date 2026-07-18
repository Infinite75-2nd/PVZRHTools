using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PVZRHTools.Models;
using PVZRHTools.ViewModels;
using Ursa.Controls;

namespace PVZRHTools.Views;

public partial class TravelBuffView : UserControl
{
    public TravelBuffView()
    {
        InitializeComponent();
        AddHandler(ToggleButton.IsCheckedChangedEvent, OnCheckBoxChanged, RoutingStrategies.Bubble,
            handledEventsToo: true);
        AddHandler(NumericIntUpDown.ValueChangedEvent, OnNumericValueChanged, RoutingStrategies.Bubble,
            handledEventsToo: true);
    }

    private TravelBuffViewModel? VM => DataContext as TravelBuffViewModel;

    private void OnCheckBoxChanged(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not CheckBox cb || cb.DataContext is not TravelBuffInfo info || VM is not { } vm) return;

        if (vm.AdvBuffs.Contains(info)) vm.AdvBuffEnabledChanged(e);
        else if (vm.InGameAdvBuffs.Contains(info)) vm.InGameAdvBuffEnabledChanged(e);
        else if (vm.UltiBuffs.Contains(info)) vm.UltiBuffEnabledChanged(e);
        else if (vm.InGameUltiBuffs.Contains(info)) vm.InGameUltiBuffEnabledChanged(e);
        else if (vm.Debuffs.Contains(info)) vm.DebuffEnabledChanged(e);
        else if (vm.InGameDebuffs.Contains(info)) vm.InGameDebuffEnabledChanged(e);
        else if (vm.InGameInvestBuffs.Contains(info)) vm.InGameInvestBuffEnabledChanged(e);
        else if (vm.InvestBuffs.Contains(info)) vm.InvestBuffEnabledChanged(e);
        else if (vm.UnlockedPlants.Contains(info)) vm.UnlockedPlantEnabledChanged(e);
        else if (vm.InGameUnlockedPlants.Contains(info)) vm.InGameUnlockedPlantEnabledChanged(e);
    }

    private void OnNumericValueChanged(object? sender, ValueChangedEventArgs<int> e)
    {
        if (e.Source is not Control control || control.DataContext is not TravelBuffInfo info ||
            VM is not { } vm) return;

        if (vm.UltiBuffs.Contains(info))
            vm.UltiBuffLevelChanged(e);
        else if (vm.InGameUltiBuffs.Contains(info))
            vm.InGameUltiBuffLevelChanged(e);
    }

    private void ApplySearchFilter(string? text, ObservableCollection<TravelBuffInfo> collection)
    {
        var filter = string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        foreach (var item in collection)
        {
            item.IsVisible = filter == null || item.Name.Contains(filter, System.StringComparison.OrdinalIgnoreCase);
        }
    }

    private void OnAdvBuffSearchChanged(object? sender, TextChangedEventArgs e) =>
        ApplySearchFilter((sender as TextBox)?.Text, VM?.AdvBuffs!);

    private void OnInGameAdvBuffSearchChanged(object? sender, TextChangedEventArgs e) =>
        ApplySearchFilter((sender as TextBox)?.Text, VM?.InGameAdvBuffs!);

    private void OnUltiBuffSearchChanged(object? sender, TextChangedEventArgs e) =>
        ApplySearchFilter((sender as TextBox)?.Text, VM?.UltiBuffs!);

    private void OnInGameUltiBuffSearchChanged(object? sender, TextChangedEventArgs e) =>
        ApplySearchFilter((sender as TextBox)?.Text, VM?.InGameUltiBuffs!);

    private void OnDebuffSearchChanged(object? sender, TextChangedEventArgs e) =>
        ApplySearchFilter((sender as TextBox)?.Text, VM?.Debuffs!);

    private void OnInGameDebuffSearchChanged(object? sender, TextChangedEventArgs e) =>
        ApplySearchFilter((sender as TextBox)?.Text, VM?.InGameDebuffs!);

    private void OnInGameInvestBuffSearchChanged(object? sender, TextChangedEventArgs e) =>
        ApplySearchFilter((sender as TextBox)?.Text, VM?.InGameInvestBuffs!);

    private void OnInvestBuffSearchChanged(object? sender, TextChangedEventArgs e) =>
        ApplySearchFilter((sender as TextBox)?.Text, VM?.InvestBuffs!);

    private void OnUnlockedPlantSearchChanged(object? sender, TextChangedEventArgs e) =>
        ApplySearchFilter((sender as TextBox)?.Text, VM?.UnlockedPlants!);

    private void OnInGameUnlockedPlantSearchChanged(object? sender, TextChangedEventArgs e) =>
        ApplySearchFilter((sender as TextBox)?.Text, VM?.InGameUnlockedPlants!);
}