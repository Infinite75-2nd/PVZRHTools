using System.Text.Json.Serialization;
using PVZRHTools.Models;
using ToolData;

namespace PVZRHTools.Utils;

[JsonSerializable(typeof(BootConfig))]
[JsonSerializable(typeof(InitData))]
[JsonSerializable(typeof(ModifierInfo))]
[JsonSerializable(typeof(SyncData))]
[JsonSerializable(typeof(ZombiesListData))]
[JsonSerializable(typeof(ZombieSea))]
[JsonSerializable(typeof(SyncTravelBuffs))]
[JsonSerializable(typeof(FlagWaveBuff))]
[JsonSerializable(typeof(Snapshot))]
[JsonSerializable(typeof(SettingsData))]
[JsonSerializable(typeof(FlagWaveBuffSettings))]
[JsonSerializable(typeof(UpdateInfo))]
public partial class JsonSGC : JsonSerializerContext
{
}