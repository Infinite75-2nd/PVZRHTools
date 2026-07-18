using PVZRHTools.ViewModels;
using Ursa.Controls;
using Ursa.ReactiveUIExtension;

namespace PVZRHTools.Views;

public partial class GameBootstrapView : ReactiveUrsaWindow<GameBootstrapViewModel>
{
    public GameBootstrapView()
    {
        InitializeComponent();
        NotificationManager = new WindowNotificationManager(this) { MaxItems = 3 };
    }

    public WindowNotificationManager NotificationManager { get; set; }
}