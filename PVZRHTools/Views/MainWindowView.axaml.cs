using System.ComponentModel;
using PVZRHTools.ViewModels;
using Ursa.ReactiveUIExtension;

namespace PVZRHTools.Views;

public partial class MainWindowView : ReactiveUrsaWindow<MainWindowViewModel>
{
    public MainWindowView()
    {
        InitializeComponent();
        Closing += OnClosing;
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        ViewModel?.ClosingCommand.Execute();
    }
}