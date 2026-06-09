using ToolData;
using ReactiveUI;

namespace PVZRHTools.ViewModels;

public abstract class ViewModelBase : ReactiveObject
{
    public virtual void SaveSettings(SettingsData settings)
    {
    }

    public virtual void LoadSettings(SettingsData settings)
    {
    }
}