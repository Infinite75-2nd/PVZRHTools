using System;
using System.IO;
using System.Text.Json;
using ToolData;
using PVZRHTools.Utils;
using PVZRHTools.ViewModels;
using Splat;

namespace PVZRHTools.Services;

public interface ISettingsService
{
    void SaveSettings(SettingsData settings);
    SettingsData? LoadSettings();
    void SaveAllViewModelSettings();
    void LoadAllViewModelSettings();
}

public class SettingsService : ISettingsService
{
    private readonly string _settingsPath;

    public SettingsService(string gamePath)
    {
        _settingsPath = Path.Combine(gamePath, Paths.SaveSettingsPath);
    }

    public void SaveSettings(SettingsData settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
        var json = JsonSerializer.Serialize(settings, JsonSGC.Default.SettingsData);
        File.WriteAllText(_settingsPath, json);
        System.Diagnostics.Debug.WriteLine($"设置已保存到: {_settingsPath}");
    }

    public SettingsData? LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                return JsonSerializer.Deserialize(json, JsonSGC.Default.SettingsData);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"加载设置失败: {ex.Message}");
        }

        return null;
    }

    public void SaveAllViewModelSettings()
    {
        var miscsVm = Locator.Current.GetService<MiscsViewModel>();

        if (miscsVm?.SaveAllSettings == true)
        {
            var settings = new SettingsData();

            var commonSettingsVm = Locator.Current.GetService<CommonSettingsViewModel>();
            var propertySettingsVm = Locator.Current.GetService<PropertySettingsViewModel>();
            var fieldReadWriteVm = Locator.Current.GetService<FieldReadWriteViewModel>();
            var travelBuffVm = Locator.Current.GetService<TravelBuffViewModel>();
            var flagWaveBuffsVm = Locator.Current.GetService<FlagWaveBuffsViewModel>();
            var abyssAndTreasureVm = Locator.Current.GetService<AbyssAndTreasureViewModel>();

            commonSettingsVm?.SaveSettings(settings);
            propertySettingsVm?.SaveSettings(settings);
            fieldReadWriteVm?.SaveSettings(settings);
            travelBuffVm?.SaveSettings(settings);
            flagWaveBuffsVm?.SaveSettings(settings);
            abyssAndTreasureVm?.SaveSettings(settings);
            miscsVm.SaveSettings(settings);

            SaveSettings(settings);
        }
        else
        {
            var blankSettings = new SettingsData();
            blankSettings.SaveAllSettings = false;
            SaveSettings(blankSettings);
        }
    }

    public void LoadAllViewModelSettings()
    {
        var savedSettings = LoadSettings();

        if (savedSettings is not { SaveAllSettings: true }) return;

        var miscsVm = Locator.Current.GetService<MiscsViewModel>();
        miscsVm?.LoadSettings(savedSettings);

        if (miscsVm?.SaveAllSettings != true) return;

        var commonSettingsVm = Locator.Current.GetService<CommonSettingsViewModel>();
        var propertySettingsVm = Locator.Current.GetService<PropertySettingsViewModel>();
        var fieldReadWriteVm = Locator.Current.GetService<FieldReadWriteViewModel>();
        var travelBuffVm = Locator.Current.GetService<TravelBuffViewModel>();
        var flagWaveBuffsVm = Locator.Current.GetService<FlagWaveBuffsViewModel>();
        var abyssAndTreasureVm = Locator.Current.GetService<AbyssAndTreasureViewModel>();

        commonSettingsVm?.LoadSettings(savedSettings);
        propertySettingsVm?.LoadSettings(savedSettings);
        fieldReadWriteVm?.LoadSettings(savedSettings);
        travelBuffVm?.LoadSettings(savedSettings);
        flagWaveBuffsVm?.LoadSettings(savedSettings);
        abyssAndTreasureVm?.LoadSettings(savedSettings);
    }
}