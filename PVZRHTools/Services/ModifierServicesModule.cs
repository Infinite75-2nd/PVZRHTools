using PVZRHTools.ViewModels;
using Splat;
using Splat.Builder;

namespace PVZRHTools.Services;

public class ModifierServicesModule : IModule
{
    public void Configure(IMutableDependencyResolver resolver)
    {
        resolver.RegisterLazySingleton(() => new DataSyncService(), typeof(IDataSyncService));
        resolver.RegisterLazySingleton(() => new NavigationService(), typeof(INavigationService));
        resolver.RegisterLazySingleton(() => new NotificationService(), typeof(INotificationService));
        resolver.RegisterLazySingleton(() => new InitDataService(App.GamePath), typeof(IInitDataService));
        resolver.RegisterLazySingleton(() => new SettingsService(App.GamePath), typeof(ISettingsService));
        resolver.RegisterLazySingleton(
            () => new UpdateCheckService(Locator.Current.GetService<INotificationService>()!),
            typeof(UpdateCheckService));

        resolver.RegisterLazySingleton(() =>
            new MainWindowViewModel(Locator.Current.GetService<INavigationService>()!,
                Locator.Current.GetService<IDataSyncService>()!, Locator.Current.GetService<IInitDataService>()!));
        resolver.RegisterLazySingleton(() =>
            new CommonSettingsViewModel(Locator.Current.GetService<IDataSyncService>()!,
                Locator.Current.GetService<IInitDataService>()!));
        resolver.RegisterLazySingleton(() =>
            new PropertySettingsViewModel(Locator.Current.GetService<IDataSyncService>()!,
                Locator.Current.GetService<IInitDataService>()!));
        resolver.RegisterLazySingleton(() =>
            new ZombiesListViewModel(Locator.Current.GetService<IDataSyncService>()!,
                Locator.Current.GetService<IInitDataService>()!, Locator.Current.GetService<INotificationService>()!,
                Locator.Current.GetService<INavigationService>()!));
        resolver.RegisterLazySingleton(() =>
            new FieldReadWriteViewModel(Locator.Current.GetService<IDataSyncService>()!,
                Locator.Current.GetService<IInitDataService>()!));
        resolver.RegisterLazySingleton(() =>
            new TravelBuffViewModel(Locator.Current.GetService<IDataSyncService>()!,
                Locator.Current.GetService<IInitDataService>()!));
        resolver.RegisterLazySingleton(() =>
            new FlagWaveBuffsViewModel(Locator.Current.GetService<IDataSyncService>()!,
                Locator.Current.GetService<IInitDataService>()!));
        resolver.RegisterLazySingleton(() =>
            new SearchListViewModel(Locator.Current.GetService<IDataSyncService>()!,
                Locator.Current.GetService<IInitDataService>()!));
        resolver.RegisterLazySingleton(() =>
            new SnapshotViewModel(Locator.Current.GetService<IDataSyncService>()!));
        resolver.RegisterLazySingleton(() =>
            new MiscsViewModel());
        resolver.RegisterLazySingleton(() =>
            new GodEvolutionViewModel(Locator.Current.GetService<IDataSyncService>()!));
        resolver.RegisterLazySingleton(()=>new AbyssAndTreasureViewModel(Locator.Current.GetService<IDataSyncService>()!,
            Locator.Current.GetService<IInitDataService>()!));
    }
}