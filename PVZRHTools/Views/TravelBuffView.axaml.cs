using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PVZRHTools.Models;
using PVZRHTools.ViewModels;
using ToolData;
using Ursa.Controls;

namespace PVZRHTools.Views;

public partial class TravelBuffView : UserControl
{
    public TravelBuffView()
    {
        InitializeComponent();
        AddHandler(ToggleButton.IsCheckedChangedEvent, OnCheckBoxChanged, RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(NumericIntUpDown.ValueChangedEvent, OnNumericValueChanged, RoutingStrategies.Bubble, handledEventsToo: true);
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
    }

    private void OnNumericValueChanged(object? sender, ValueChangedEventArgs<int> e)
    {
        if (e.Source is not Control control || control.DataContext is not TravelBuffInfo info || VM is not { } vm) return;

        if (vm.UltiBuffs.Contains(info))
            vm.UltiBuffLevelChanged(e);
        else if (vm.InGameUltiBuffs.Contains(info))
            vm.InGameUltiBuffLevelChanged(e);
    }
}
