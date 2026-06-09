using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PVZRHTools.Models;
using PVZRHTools.Services;
using PVZRHTools.Utils;
using ReactiveUI.SourceGenerators;
using ToolData;
using Ursa.Controls;

namespace PVZRHTools.ViewModels;

public partial class TravelBuffViewModel : ModifierPageViewModelBase
{
    private IInitDataService InitDataService { get; }

    [Reactive] public partial ObservableCollection<TravelBuffInfo> AdvBuffs { get; set; }
    [Reactive] public partial ObservableCollection<TravelBuffInfo> UltiBuffs { get; set; }
    [Reactive] public partial ObservableCollection<TravelBuffInfo> Debuffs { get; set; }
    [Reactive] public partial ObservableCollection<TravelBuffInfo> InvestBuffs { get; set; }

    [Reactive] public partial ObservableCollection<TravelBuffInfo> InGameAdvBuffs { get; set; }
    [Reactive] public partial ObservableCollection<TravelBuffInfo> InGameUltiBuffs { get; set; }
    [Reactive] public partial ObservableCollection<TravelBuffInfo> InGameDebuffs { get; set; }
    [Reactive] public partial ObservableCollection<TravelBuffInfo> InGameInvestBuffs { get; set; }

    public void MessageReceived(object? sender, SyncData message)
    {
        if (message.Command is Strings.UpdateAllBuffs)
        {
            DataSyncService.Lock(true);
            var buffs = JsonSerializer.Deserialize(message.Parameters[0], JsonSGC.Default.SyncTravelBuffs);
            foreach (var adv in InGameAdvBuffs)
            {
                if (buffs.InGameAdvBuffs.TryGetValue(adv.ID, out var advLevel))
                {
                    adv.Level = advLevel;
                }
            }

            foreach (var ulti in InGameUltiBuffs)
            {
                if (buffs.InGameUltiBuffs.TryGetValue(ulti.ID, out var ultiLevel))
                {
                    ulti.Level = ultiLevel;
                }
            }

            foreach (var debuff in InGameDebuffs)
            {
                if (buffs.InGameDebuffs.TryGetValue(debuff.ID, out var debuffEnabled))
                {
                    debuff.Enabled = debuffEnabled;
                }
            }

            foreach (var invest in InGameInvestBuffs)
            {
                if (buffs.InGameInvestBuffs.TryGetValue(invest.ID, out var investEnabled))
                {
                    invest.Enabled = investEnabled;
                }
            }

            DataSyncService.Lock(false);
        }
    }

    [ReactiveCommand]
    private void AdvBuffLevelChanged(ValueChangedEventArgs<int> e)
    {
        var info = (TravelBuffInfo)((Control)e.Source!).DataContext!;
        info.Level = e.NewValue ?? 0;
        if (!((Control)e.Source!).IsVisible) return;
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAdvBuff,
            Parameters = [info.ID.ToString(), info.Level.ToString()]
        });
    }

    [ReactiveCommand]
    private void AdvBuffEnabledChanged(RoutedEventArgs e)
    {
        var info = (TravelBuffInfo)((Control)e.Source!).DataContext!;
        info.Level = ((CheckBox)e.Source).IsChecked ?? false ? 1 : 0;
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAdvBuff,
            Parameters = [info.ID.ToString(), info.Level.ToString()]
        });
    }

    [ReactiveCommand]
    private void AdvBuffChooseAll()
    {
        DataSyncService.Lock(true);
        foreach (var advBuff in AdvBuffs)
        {
            advBuff.Level = 1;
        }

        DataSyncService.Lock(false);
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAllBuffs,
            Parameters =
            [
                JsonSerializer.Serialize(new SyncTravelBuffs()
                {
                    AdvBuffs = AdvBuffs.ToDictionary(x => x.ID, x => x.Level)
                }, JsonSGC.Default.SyncTravelBuffs)
            ]
        });
    }

    [ReactiveCommand]
    private void AdvBuffDeselectAll()
    {
        DataSyncService.Lock(true);
        foreach (var advBuff in AdvBuffs)
        {
            advBuff.Level = 0;
        }

        DataSyncService.Lock(false);
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAllBuffs,
            Parameters =
            [
                JsonSerializer.Serialize(new SyncTravelBuffs()
                {
                    AdvBuffs = AdvBuffs.ToDictionary(x => x.ID, x => x.Level)
                }, JsonSGC.Default.SyncTravelBuffs)
            ]
        });
    }

    [ReactiveCommand]
    private void InGameAdvBuffLevelChanged(ValueChangedEventArgs<int> e)
    {
        var info = (TravelBuffInfo)((Control)e.Source!).DataContext!;
        info.Level = e.NewValue ?? 0;
        if (!((Control)e.Source!).IsVisible) return;
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateInGameAdvBuff,
            Parameters = [info.ID.ToString(), info.Level.ToString()]
        });
    }

    [ReactiveCommand]
    private void InGameAdvBuffEnabledChanged(RoutedEventArgs e)
    {
        var info = (TravelBuffInfo)((Control)e.Source!).DataContext!;
        info.Level = ((CheckBox)e.Source).IsChecked ?? false ? 1 : 0;
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateInGameAdvBuff,
            Parameters = [info.ID.ToString(), info.Level.ToString()]
        });
    }

    [ReactiveCommand]
    private void InGameAdvBuffChooseAll()
    {
        DataSyncService.Lock(true);
        foreach (var advBuff in InGameAdvBuffs)
        {
            advBuff.Level = 1;
        }

        DataSyncService.Lock(false);
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAllBuffs,
            Parameters =
            [
                JsonSerializer.Serialize(new SyncTravelBuffs()
                {
                    InGameAdvBuffs = InGameAdvBuffs.ToDictionary(x => x.ID, x => x.Level)
                }, JsonSGC.Default.SyncTravelBuffs)
            ]
        });
    }

    [ReactiveCommand]
    private void InGameAdvBuffDeselectAll()
    {
        DataSyncService.Lock(true);
        foreach (var advBuff in InGameAdvBuffs)
        {
            advBuff.Level = 0;
        }

        DataSyncService.Lock(false);
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAllBuffs,
            Parameters =
            [
                JsonSerializer.Serialize(new SyncTravelBuffs()
                {
                    InGameAdvBuffs = InGameAdvBuffs.ToDictionary(x => x.ID, x => x.Level)
                }, JsonSGC.Default.SyncTravelBuffs)
            ]
        });
    }

    [ReactiveCommand]
    private void UltiBuffLevelChanged(ValueChangedEventArgs<int> e)
    {
        var info = (TravelBuffInfo)((Control)e.Source!).DataContext!;
        info.Level = e.NewValue ?? 0;
        if (!((Control)e.Source!).IsVisible) return;
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateUltiBuff,
            Parameters = [info.ID.ToString(), info.Level.ToString()]
        });
    }

    [ReactiveCommand]
    private void UltiBuffEnabledChanged(RoutedEventArgs e)
    {
        var info = (TravelBuffInfo)((Control)e.Source!).DataContext!;
        info.Level = ((CheckBox)e.Source).IsChecked ?? false ? 1 : 0;
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateUltiBuff,
            Parameters = [info.ID.ToString(), info.Level.ToString()]
        });
    }

    [ReactiveCommand]
    private void UltiBuffChooseAll()
    {
        DataSyncService.Lock(true);
        foreach (var ultiBuff in UltiBuffs)
        {
            ultiBuff.Level = 1;
        }

        DataSyncService.Lock(false);
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAllBuffs,
            Parameters =
            [
                JsonSerializer.Serialize(new SyncTravelBuffs()
                {
                    UltiBuffs = UltiBuffs.ToDictionary(x => x.ID, x => x.Level)
                }, JsonSGC.Default.SyncTravelBuffs)
            ]
        });
    }

    [ReactiveCommand]
    private void UltiBuffDeselectAll()
    {
        DataSyncService.Lock(true);
        foreach (var ultiBuff in UltiBuffs)
        {
            ultiBuff.Level = 0;
        }

        DataSyncService.Lock(false);
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAllBuffs,
            Parameters =
            [
                JsonSerializer.Serialize(new SyncTravelBuffs()
                {
                    UltiBuffs = UltiBuffs.ToDictionary(x => x.ID, x => x.Level)
                }, JsonSGC.Default.SyncTravelBuffs)
            ]
        });
    }

    [ReactiveCommand]
    private void InGameUltiBuffLevelChanged(ValueChangedEventArgs<int> e)
    {
        var info = (TravelBuffInfo)((Control)e.Source!).DataContext!;
        info.Level = e.NewValue ?? 0;
        if (!((Control)e.Source!).IsVisible) return;
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateInGameUltiBuff,
            Parameters = [info.ID.ToString(), info.Level.ToString()]
        });
    }

    [ReactiveCommand]
    private void InGameUltiBuffEnabledChanged(RoutedEventArgs e)
    {
        var info = (TravelBuffInfo)((Control)e.Source!).DataContext!;
        info.Level = ((CheckBox)e.Source).IsChecked ?? false ? 1 : 0;
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateInGameUltiBuff,
            Parameters = [info.ID.ToString(), info.Level.ToString()]
        });
    }

    [ReactiveCommand]
    private void InGameUltiBuffChooseAll()
    {
        DataSyncService.Lock(true);
        foreach (var ultiBuff in InGameUltiBuffs)
        {
            ultiBuff.Level = 1;
        }

        DataSyncService.Lock(false);
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAllBuffs,
            Parameters =
            [
                JsonSerializer.Serialize(new SyncTravelBuffs()
                {
                    InGameUltiBuffs = InGameUltiBuffs.ToDictionary(x => x.ID, x => x.Level)
                }, JsonSGC.Default.SyncTravelBuffs)
            ]
        });
    }

    [ReactiveCommand]
    private void InGameUltiBuffDeselectAll()
    {
        DataSyncService.Lock(true);
        foreach (var ultiBuff in InGameUltiBuffs)
        {
            ultiBuff.Level = 0;
        }

        DataSyncService.Lock(false);
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAllBuffs,
            Parameters =
            [
                JsonSerializer.Serialize(new SyncTravelBuffs()
                {
                    InGameUltiBuffs = InGameUltiBuffs.ToDictionary(x => x.ID, x => x.Level)
                }, JsonSGC.Default.SyncTravelBuffs)
            ]
        });
    }

    [ReactiveCommand]
    private void DebuffEnabledChanged(RoutedEventArgs e)
    {
        var info = (TravelBuffInfo)((Control)e.Source!).DataContext!;
        info.Enabled = ((CheckBox)e.Source).IsChecked ?? false;
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateDebuff,
            Parameters = [info.ID.ToString(), info.Enabled.ToString()]
        });
    }

    [ReactiveCommand]
    private void DebuffChooseAll()
    {
        DataSyncService.Lock(true);
        foreach (var debuff in Debuffs)
        {
            debuff.Enabled = true;
        }

        DataSyncService.Lock(false);
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAllBuffs,
            Parameters =
            [
                JsonSerializer.Serialize(new SyncTravelBuffs()
                {
                    Debuffs = Debuffs.ToDictionary(x => x.ID, x => x.Enabled)
                }, JsonSGC.Default.SyncTravelBuffs)
            ]
        });
    }

    [ReactiveCommand]
    private void DebuffDeselectAll()
    {
        DataSyncService.Lock(true);
        foreach (var debuff in Debuffs)
        {
            debuff.Enabled = false;
        }

        DataSyncService.Lock(false);
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAllBuffs,
            Parameters =
            [
                JsonSerializer.Serialize(new SyncTravelBuffs()
                {
                    Debuffs = Debuffs.ToDictionary(x => x.ID, x => x.Enabled)
                }, JsonSGC.Default.SyncTravelBuffs)
            ]
        });
    }

    [ReactiveCommand]
    private void InGameDebuffEnabledChanged(RoutedEventArgs e)
    {
        var info = (TravelBuffInfo)((Control)e.Source!).DataContext!;
        info.Enabled = ((CheckBox)e.Source).IsChecked ?? false;
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateInGameDebuff,
            Parameters = [info.ID.ToString(), info.Enabled.ToString()]
        });
    }

    [ReactiveCommand]
    private void InGameDebuffChooseAll()
    {
        DataSyncService.Lock(true);
        foreach (var debuff in InGameDebuffs)
        {
            debuff.Enabled = true;
        }

        DataSyncService.Lock(false);
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAllBuffs,
            Parameters =
            [
                JsonSerializer.Serialize(new SyncTravelBuffs()
                {
                    InGameDebuffs = InGameDebuffs.ToDictionary(x => x.ID, x => x.Enabled)
                }, JsonSGC.Default.SyncTravelBuffs)
            ]
        });
    }

    [ReactiveCommand]
    private void InGameDebuffDeselectAll()
    {
        DataSyncService.Lock(true);
        foreach (var debuff in InGameDebuffs)
        {
            debuff.Enabled = false;
        }

        DataSyncService.Lock(false);
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAllBuffs,
            Parameters =
            [
                JsonSerializer.Serialize(new SyncTravelBuffs()
                {
                    InGameDebuffs = InGameDebuffs.ToDictionary(x => x.ID, x => x.Enabled)
                }, JsonSGC.Default.SyncTravelBuffs)
            ]
        });
    }

    [ReactiveCommand]
    private void InvestBuffEnabledChanged(RoutedEventArgs e)
    {
        var info = (TravelBuffInfo)((Control)e.Source!).DataContext!;
        info.Enabled = ((CheckBox)e.Source).IsChecked ?? false;
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateInvestBuff,
            Parameters = [info.ID.ToString(), info.Enabled.ToString()]
        });
    }

    [ReactiveCommand]
    private void InvestBuffChooseAll()
    {
        DataSyncService.Lock(true);
        foreach (var invest in InvestBuffs)
        {
            invest.Enabled = true;
        }

        DataSyncService.Lock(false);
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAllBuffs,
            Parameters =
            [
                JsonSerializer.Serialize(new SyncTravelBuffs()
                {
                    InvestBuffs = InvestBuffs.ToDictionary(x => x.ID, x => x.Enabled)
                }, JsonSGC.Default.SyncTravelBuffs)
            ]
        });
    }

    [ReactiveCommand]
    private void InvestBuffDeselectAll()
    {
        DataSyncService.Lock(true);
        foreach (var invest in InvestBuffs)
        {
            invest.Enabled = false;
        }

        DataSyncService.Lock(false);
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAllBuffs,
            Parameters =
            [
                JsonSerializer.Serialize(new SyncTravelBuffs()
                {
                    InvestBuffs = InvestBuffs.ToDictionary(x => x.ID, x => x.Enabled)
                }, JsonSGC.Default.SyncTravelBuffs)
            ]
        });
    }

    [ReactiveCommand]
    private void InGameInvestBuffEnabledChanged(RoutedEventArgs e)
    {
        var info = (TravelBuffInfo)((Control)e.Source!).DataContext!;
        info.Enabled = ((CheckBox)e.Source).IsChecked ?? false;
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateInGameInvestBuff,
            Parameters = [info.ID.ToString(), info.Enabled.ToString()]
        });
    }

    [ReactiveCommand]
    private void InGameInvestBuffChooseAll()
    {
        DataSyncService.Lock(true);
        foreach (var invest in InGameInvestBuffs)
        {
            invest.Enabled = true;
        }

        DataSyncService.Lock(false);
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAllBuffs,
            Parameters =
            [
                JsonSerializer.Serialize(new SyncTravelBuffs()
                {
                    InGameInvestBuffs = InGameInvestBuffs.ToDictionary(x => x.ID, x => x.Enabled)
                }, JsonSGC.Default.SyncTravelBuffs)
            ]
        });
    }

    [ReactiveCommand]
    private void InGameInvestBuffDeselectAll()
    {
        DataSyncService.Lock(true);
        foreach (var invest in InGameInvestBuffs)
        {
            invest.Enabled = false;
        }

        DataSyncService.Lock(false);
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAllBuffs,
            Parameters =
            [
                JsonSerializer.Serialize(new SyncTravelBuffs()
                {
                    InGameInvestBuffs = InGameInvestBuffs.ToDictionary(x => x.ID, x => x.Enabled)
                }, JsonSGC.Default.SyncTravelBuffs)
            ]
        });
    }

    public override void SaveSettings(SettingsData settings)
    {
        settings.TravelAdvBuffs = AdvBuffs.ToDictionary(x => x.ID, x => x.Level);
        settings.TravelUltiBuffs = UltiBuffs.ToDictionary(x => x.ID, x => x.Level);
        settings.TravelDebuffs = Debuffs.ToDictionary(x => x.ID, x => x.Enabled);
        settings.TravelInvestBuffs = InvestBuffs.ToDictionary(x => x.ID, x => x.Enabled);
    }

    public override void LoadSettings(SettingsData settings)
    {

            foreach (var adv in AdvBuffs)
            {
                if (settings.TravelAdvBuffs.TryGetValue(adv.ID, out var level))
                    adv.Level = level;
            }
        


            foreach (var ulti in UltiBuffs)
            {
                if (settings.TravelUltiBuffs.TryGetValue(ulti.ID, out var level))
                    ulti.Level = level;
            }
        


            foreach (var debuff in Debuffs)
            {
                if (settings.TravelDebuffs.TryGetValue(debuff.ID, out var enabled))
                    debuff.Enabled = enabled;
            }
        

            foreach (var invest in InvestBuffs)
            {
                if (settings.TravelInvestBuffs.TryGetValue(invest.ID, out var enabled))
                    invest.Enabled = enabled;
            }
        
    }

    public TravelBuffViewModel(IDataSyncService dataSyncService, IInitDataService initDataService) : base(
        dataSyncService)
    {
        InitDataService = initDataService;
        AdvBuffs = [];
        UltiBuffs = [];
        Debuffs = [];
        InvestBuffs = [];
        InGameAdvBuffs = [];
        InGameUltiBuffs = [];
        InGameDebuffs = [];
        InGameInvestBuffs = [];
        DataSyncService.MessageReceived += MessageReceived;

        foreach (var adv in InitDataService.InitData.AdvBuffs)
        {
            AdvBuffs.Add(new TravelBuffInfo { ID = adv.Key, Name = adv.Value, Level = 0 });
            InGameAdvBuffs.Add(new TravelBuffInfo { ID = adv.Key, Name = adv.Value, Level = 0 });
        }

        foreach (var ulti in InitDataService.InitData.UltiBuffs)
        {
            UltiBuffs.Add(new TravelBuffInfo { ID = ulti.Key, Name = ulti.Value, Level = 0 });
            InGameUltiBuffs.Add(new TravelBuffInfo { ID = ulti.Key, Name = ulti.Value, Level = 0 });
        }

        foreach (var debuff in InitDataService.InitData.Debuffs)
        {
            Debuffs.Add(new TravelBuffInfo { ID = debuff.Key, Name = debuff.Value, Enabled = false });
            InGameDebuffs.Add(new TravelBuffInfo { ID = debuff.Key, Name = debuff.Value, Enabled = false });
        }

        foreach (var invest in InitDataService.InitData.InvestBuffs)
        {
            InvestBuffs.Add(new TravelBuffInfo { ID = invest.Key, Name = invest.Value, Enabled = false });
            InGameInvestBuffs.Add(new TravelBuffInfo { ID = invest.Key, Name = invest.Value, Enabled = false });
        }
    }
}