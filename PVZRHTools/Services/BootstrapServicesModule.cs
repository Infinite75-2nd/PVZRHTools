using PVZRHTools.ViewModels;
using Splat;
using Splat.Builder;

namespace PVZRHTools.Services;

public class BootstrapServicesModule : IModule
{
    public void Configure(IMutableDependencyResolver resolver)
    {
        //Services
        resolver.RegisterLazySingleton(() => new RunGameService(Locator.Current.GetService<IGameBootstrapService>()!),
            typeof(IRunGameService));
        resolver.RegisterLazySingleton(() => new NavigationService(), typeof(INavigationService));
        resolver.RegisterLazySingleton(() => new ModsManagementService(), typeof(IModsManagementService));
        resolver.RegisterLazySingleton(
            () => new ModifierInfoService(Locator.Current.GetService<IModsManagementService>()!,
                Locator.Current.GetService<IGameBootstrapService>()!),
            typeof(IModifierInfoService));
        resolver.RegisterLazySingleton(() => new NotificationService(), typeof(INotificationService));
        resolver.RegisterLazySingleton(() => new GameBootstrapService(),
            typeof(IGameBootstrapService));

        //ViewModels
        resolver.RegisterLazySingleton(() =>
            new GameInstancesViewModel(Locator.Current.GetService<IRunGameService>()!,
                Locator.Current.GetService<IModifierInfoService>()!,
                Locator.Current.GetService<IGameBootstrapService>()!,
                Locator.Current.GetService<INotificationService>()!));
        resolver.RegisterLazySingleton(() =>
            new GameBootstrapViewModel(Locator.Current.GetService<INavigationService>()!,
                Locator.Current.GetService<IModifierInfoService>()!,
                Locator.Current.GetService<IGameBootstrapService>()!,
                Locator.Current.GetService<INotificationService>()!));
        resolver.RegisterLazySingleton(() =>
            new LinksViewModel());
        resolver.RegisterLazySingleton(() =>
            new AboutViewModel());
        resolver.Register(() =>
            new ModListViewModel(Locator.Current.GetService<IModsManagementService>()!,
                Locator.Current.GetService<INotificationService>()!));
        resolver.Register(() => new LaunchSettingsViewModel());
    }
}