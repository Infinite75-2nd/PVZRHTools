using ToolData;
using PVZRHTools.Services;
using PVZRHTools.Utils;
using ReactiveUI.SourceGenerators;

namespace PVZRHTools.ViewModels;

public partial class InGameHotkeysViewModel : ModifierPageViewModelBase
{
    [Reactive] public partial KeyCode KeySpeedStop { get; set; } = KeyCode.Alpha6;
    [Reactive] public partial KeyCode KeyShowGameInfo { get; set; } = KeyCode.BackQuote;
    [Reactive] public partial KeyCode KeyTopMostCardBank { get; set; } = KeyCode.Tab;
    [Reactive] public partial KeyCode KeyRandomCard { get; set; } = KeyCode.R;
    [Reactive] public partial KeyCode KeyAlmanacCreatePlant { get; set; } = KeyCode.B;
    [Reactive] public partial KeyCode KeyAlmanacCreatePlantVase { get; set; } = KeyCode.J;
    [Reactive] public partial KeyCode KeyAlmanacCreateZombie { get; set; } = KeyCode.N;
    [Reactive] public partial KeyCode KeyAlmanacCreateZombieVase { get; set; } = KeyCode.K;
    [Reactive] public partial KeyCode KeyAlmanacZombieMindCtrl { get; set; } = KeyCode.LeftControl;

    public InGameHotkeysViewModel(IDataSyncService dataSyncService) : base(dataSyncService)
    {
        this.SimpleOneWaySync(x => x.KeySpeedStop, Strings.KeySpeedStop);
        this.SimpleOneWaySync(x => x.KeyShowGameInfo, Strings.KeyShowGameInfo);
        this.SimpleOneWaySync(x => x.KeyTopMostCardBank, Strings.KeyTopMostCardBank);
        this.SimpleOneWaySync(x => x.KeyRandomCard, Strings.KeyRandomCard);
        this.SimpleOneWaySync(x => x.KeyAlmanacCreatePlant, Strings.KeyAlmanacCreatePlant);
        this.SimpleOneWaySync(x => x.KeyAlmanacCreatePlantVase, Strings.KeyAlmanacCreatePlantVase);
        this.SimpleOneWaySync(x => x.KeyAlmanacCreateZombie, Strings.KeyAlmanacCreateZombie);
        this.SimpleOneWaySync(x => x.KeyAlmanacCreateZombieVase, Strings.KeyAlmanacCreateZombieVase);
        this.SimpleOneWaySync(x => x.KeyAlmanacZombieMindCtrl, Strings.KeyAlmanacZombieMindCtrl);
    }

    public override void SaveSettings(SettingsData settings)
    {
        settings.KeySpeedStop = (int)KeySpeedStop;
        settings.KeyShowGameInfo = (int)KeyShowGameInfo;
        settings.KeyTopMostCardBank = (int)KeyTopMostCardBank;
        settings.KeyRandomCard = (int)KeyRandomCard;
        settings.KeyAlmanacCreatePlant = (int)KeyAlmanacCreatePlant;
        settings.KeyAlmanacCreatePlantVase = (int)KeyAlmanacCreatePlantVase;
        settings.KeyAlmanacCreateZombie = (int)KeyAlmanacCreateZombie;
        settings.KeyAlmanacCreateZombieVase = (int)KeyAlmanacCreateZombieVase;
        settings.KeyAlmanacZombieMindCtrl = (int)KeyAlmanacZombieMindCtrl;
    }

    public override void LoadSettings(SettingsData settings)
    {
        KeySpeedStop = (KeyCode)settings.KeySpeedStop;
        KeyShowGameInfo = (KeyCode)settings.KeyShowGameInfo;
        KeyTopMostCardBank = (KeyCode)settings.KeyTopMostCardBank;
        KeyRandomCard = (KeyCode)settings.KeyRandomCard;
        KeyAlmanacCreatePlant = (KeyCode)settings.KeyAlmanacCreatePlant;
        KeyAlmanacCreatePlantVase = (KeyCode)settings.KeyAlmanacCreatePlantVase;
        KeyAlmanacCreateZombie = (KeyCode)settings.KeyAlmanacCreateZombie;
        KeyAlmanacCreateZombieVase = (KeyCode)settings.KeyAlmanacCreateZombieVase;
        KeyAlmanacZombieMindCtrl = (KeyCode)settings.KeyAlmanacZombieMindCtrl;
    }
}