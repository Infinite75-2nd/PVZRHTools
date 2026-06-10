using System;
using System.Collections.ObjectModel;
using PVZRHTools.Models;
using PVZRHTools.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using ToolData;

namespace PVZRHTools.ViewModels;

public partial class SearchListViewModel : ModifierPageViewModelBase
{
    public IInitDataService InitDataService { get; }

    [Reactive] public partial string SearchText { get; set; }
    [Reactive] public partial int SoundID { get; set; }
    [Reactive] public partial int ParticleID { get; set; }

    public ObservableCollection<SearchItem> Plants { get; set; } = [];
    public ObservableCollection<SearchItem> Zombies { get; set; } = [];
    public ObservableCollection<SearchItem> FirstArmors { get; set; } = [];
    public ObservableCollection<SearchItem> SecondArmors { get; set; } = [];
    public ObservableCollection<SearchItem> Bullets { get; set; } = [];
    public ObservableCollection<SearchItem> AdvBuffs { get; set; } = [];
    public ObservableCollection<SearchItem> UltiBuffs { get; set; } = [];
    public ObservableCollection<SearchItem> Debuffs { get; set; } = [];
    public ObservableCollection<SearchItem> InvestBuffs { get; set; } = [];
    public ObservableCollection<SearchItem> Items { get; set; } = [];

    [ReactiveCommand]
    public void PlaySound()
    {
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.PlaySound,
            Parameters = [SoundID.ToString()]
        });
    }

    [ReactiveCommand]
    public void PlayParticle()
    {
        DataSyncService.SendCommand(new SyncData()
        {
            Command = Strings.PlayParticle,
            Parameters = [ParticleID.ToString()]
        });
    }

    public SearchListViewModel(IDataSyncService dataSyncService, IInitDataService initDataService) : base(
        dataSyncService)
    {
        InitDataService = initDataService;

        // 初始化所有数据
        foreach (var plant in InitDataService.InitData.Plants)
            Plants.Add(new SearchItem { ID = plant.Key, Name = plant.Value, IsVisible = true });

        foreach (var zombie in InitDataService.InitData.Zombies)
            Zombies.Add(new SearchItem { ID = zombie.Key, Name = zombie.Value, IsVisible = true });

        foreach (var firstArmor in InitDataService.InitData.FirstArmors)
            FirstArmors.Add(new SearchItem { ID = firstArmor.Key, Name = firstArmor.Value, IsVisible = true });

        foreach (var secondArmor in InitDataService.InitData.SecondArmors)
            SecondArmors.Add(new SearchItem { ID = secondArmor.Key, Name = secondArmor.Value, IsVisible = true });

        foreach (var bullet in InitDataService.InitData.Bullets)
            Bullets.Add(new SearchItem { ID = bullet.Key, Name = bullet.Value, IsVisible = true });

        foreach (var advBuff in InitDataService.InitData.AdvBuffs)
            AdvBuffs.Add(new SearchItem { ID = advBuff.Key, Name = advBuff.Value, IsVisible = true });

        foreach (var ultiBuff in InitDataService.InitData.UltiBuffs)
            UltiBuffs.Add(new SearchItem { ID = ultiBuff.Key, Name = ultiBuff.Value, IsVisible = true });

        foreach (var debuff in InitDataService.InitData.Debuffs)
            Debuffs.Add(new SearchItem { ID = debuff.Key, Name = debuff.Value, IsVisible = true });

        foreach (var investBuff in InitDataService.InitData.InvestBuffs)
            InvestBuffs.Add(new SearchItem { ID = investBuff.Key, Name = investBuff.Value, IsVisible = true });

        foreach (var item in InitDataService.Items)
            Items.Add(new SearchItem { ID = item.Key, Name = item.Value, IsVisible = true });

        // 监听搜索文本变化
        this.WhenAnyValue(x => x.SearchText)
            .Subscribe(_ => ApplyFilter());
    }

    private void ApplyFilter()
    {
        var searchText = SearchText?.ToLower() ?? string.Empty;

        ApplyFilterToCollection(Plants, searchText);
        ApplyFilterToCollection(Zombies, searchText);
        ApplyFilterToCollection(FirstArmors, searchText);
        ApplyFilterToCollection(SecondArmors, searchText);
        ApplyFilterToCollection(Bullets, searchText);
        ApplyFilterToCollection(AdvBuffs, searchText);
        ApplyFilterToCollection(UltiBuffs, searchText);
        ApplyFilterToCollection(Debuffs, searchText);
        ApplyFilterToCollection(InvestBuffs, searchText);
        ApplyFilterToCollection(Items, searchText);
    }

    private void ApplyFilterToCollection(ObservableCollection<SearchItem> collection, string searchText)
    {
        foreach (var item in collection)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                item.IsVisible = true;
            }
            else
            {
                item.IsVisible = item.Name.ToLower().Contains(searchText) ||
                                 item.ID.ToString().Contains(searchText);
            }
        }
    }
}