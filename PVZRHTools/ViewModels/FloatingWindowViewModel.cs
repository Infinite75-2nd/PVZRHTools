using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI.SourceGenerators;

namespace PVZRHTools.ViewModels;

public partial class FloatingWindowViewModel : ViewModelBase
{
    private Window? _floatingWindow;
    private Point _startPoint;
    private PixelPoint _windowStartPosition;
    
    [ReactiveCommand]
    private void StartDrag()
    {
        // 拖动逻辑由View层处理
    }

    [ReactiveCommand]
    private void BringMainWindowToFront()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var lifetime = Application.Current?.ApplicationLifetime as
                Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
            lifetime?.MainWindow?.Activate();
        });
    }

    public void SetWindow(Window window)
    {
        _floatingWindow = window;
    }
}