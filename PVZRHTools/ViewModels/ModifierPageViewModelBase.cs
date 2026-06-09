using PVZRHTools.Services;

namespace PVZRHTools.ViewModels;

public abstract class ModifierPageViewModelBase : ViewModelBase
{
    public readonly IDataSyncService DataSyncService;

    public ModifierPageViewModelBase(IDataSyncService dataSyncService)
    {
        DataSyncService = dataSyncService;
    }
}