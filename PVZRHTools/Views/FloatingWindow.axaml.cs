using Avalonia.Input;
using Avalonia.Threading;
using PVZRHTools.ViewModels;
using Ursa.ReactiveUIExtension;

namespace PVZRHTools.Views;

public partial class FloatingWindow : ReactiveUrsaWindow<FloatingWindowViewModel>
{
    public FloatingWindow()
    {
        InitializeComponent();

        // 使用PointerPressed事件启动系统级拖动
        PointerPressed += OnPointerPressed;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // 检查是否是左键点击
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            // 检测双击
            if (e.ClickCount == 2)
            {
                // 双击事件 - 将主窗口置顶
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var lifetime = Avalonia.Application.Current?.ApplicationLifetime as
                        Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
                    lifetime?.MainWindow?.Activate();
                });
            }
            else
            {
                // 单击 - 使用系统级拖动,和拖动标题栏完全一样
                BeginMoveDrag(e);
            }
        }
    }
}