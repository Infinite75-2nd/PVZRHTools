using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AlmanacData;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using TMPro;
using ToolMod.Components;
using ToolData;
using UnityEngine;
using Paths = ToolData.Paths;
using static ToolMod.Utils;
using static ToolMod.Components.PatchDataCache;

[assembly: AssemblyFileVersion(Strings.ModifierVersion)]
[assembly: AssemblyCompany("PVZRHTools")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyProduct("PVZRHTools")]
[assembly: AssemblyTitle("PVZRHTools")]
[assembly: AssemblyVersion(Strings.ModifierVersion)]

namespace ToolMod
{
    [BepInPlugin("infinite75.toolmod", "PVZRHTools", Strings.ModifierVersion)]
    public class ModCore : BasePlugin
    {
        /// <summary>True on the Android CoreCLR host; UI runs in the launcher, no external exe.</summary>
        public static bool IsAndroid => OperatingSystem.IsAndroid();

        /// <summary>
        /// BepInEx config dir shared with the launcher UI. Derived from the plugin's
        /// own location (`.../BepInEx/plugins` → `.../BepInEx/config`) because
        /// <see cref="BepInEx.Paths.GameRootPath"/> points elsewhere on this host.
        /// </summary>
        public static string SharedConfigDir => Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".",
            "..", "config"));

        /// <summary>Allows NaN / ±Infinity (some PatchDataCache floats use -inf as a sentinel).</summary>
        private static readonly JsonSerializerOptions StateJsonOptions = new()
        {
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
        };

        public override void Load()
        {
            try { Console.OutputEncoding = Encoding.UTF8; } catch { /* not supported on Android */ }

            BootConfig = ReadBootConfig();
            if (!BootConfig.ModifierEnabled) return;

            if (!IsAndroid)
            {
                // Desktop needs the Avalonia exe; Android drives everything from the launcher UI.
                if (File.Exists(Path.Combine(BepInEx.Paths.GameRootPath, Paths.ModifierExeName)))
                    ModifierPath = Path.Combine(BepInEx.Paths.GameRootPath, Paths.ModifierExeName);
                else if (File.Exists(BootConfig.ModifierPath))
                    ModifierPath = BootConfig.ModifierPath;

                if (string.IsNullOrEmpty(ModifierPath))
                {
                    Log.LogFatal("PVZRHTools.exe不存在，修改器已禁用");
                    return;
                }
            }

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Instance = this;
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => Unload();
        }

        /// <summary>Reads ModifierBootConfig.json, defaulting to enabled when absent.</summary>
        private BootConfig ReadBootConfig()
        {
            try
            {
                var path = Path.Combine(SharedConfigDir, "ModifierBootConfig.json");
                if (File.Exists(path))
                {
                    var cfg = JsonSerializer.Deserialize<BootConfig>(File.ReadAllText(path));
                    if (cfg.ModifierPath is null && cfg.GameVersion is null && !cfg.ModifierEnabled)
                        cfg.ModifierEnabled = true;
                    return cfg;
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning("读取 ModifierBootConfig.json 失败，按默认启用处理: " + ex.Message);
            }
            return new BootConfig { ModifierEnabled = true, ModifierPath = "", GameVersion = Strings.GameVersion };
        }

        public void LateInit()
        {
            if (Inited) return;

            ClassInjector.RegisterTypeInIl2Cpp<DataProcessor>();
            ClassInjector.RegisterTypeInIl2Cpp<ToolsUpdater>();
            ClassInjector.RegisterTypeInIl2Cpp<PlantStatisticsModifier>();
            if (!IsAndroid)
            {
                ClassInjector.RegisterTypeInIl2Cpp<KeyBindingButton>();
                ClassInjector.RegisterTypeInIl2Cpp<KeyBindingUI>();
                ClassInjector.RegisterTypeInIl2Cpp<GameKeyBindingUI>();
            }

            GameAPP.theGameStatus = GameStatus.OutGame;
            ModifierObject = new("PVZRHTools");
            ModifierObject.AddComponent<DataProcessor>();
            ModifierObject.AddComponent<ToolsUpdater>();
            CacheObject = new GameObject("CacheObject");
            CacheObject.SetActive(false);
            CacheObject.transform.SetParent(ModifierObject.transform);
            UnityEngine.Object.DontDestroyOnLoad(ModifierObject);
            GenerateInitData();

            // 加载并应用保存的设置
            SettingsLoader.LoadAndApplySettings();
            LoadModState();
            HotKeysLoader.Load();
            GameKeysLoader.Load();

            if (IsAndroid)
            {
                // Launcher UI shares the app sandbox: talk over files in BepInEx/config.
                DataSync = new FileDataSync(SharedConfigDir);
            }
            else
            {
                DataSync = new DataSync(Strings.PipeName);
            }
            DataSync.Connected += (sender, e) => { Log.LogMessage("修改器已连接"); };
            DataSync.MessageReceived += MessageReceived;
            DataSync.Disconnected += (sender, e) =>
            {
                Log.LogMessage("修改器已断开");
                // Desktop exits with the UI window; Android must keep the game alive.
                if (!IsAndroid) Environment.Exit(0);
            };
            DataSync.Start();
            DumpState();
            // Signal the launcher that InitData/state are ready so it can attach
            // the modifier UI only now (not during game startup).
            try
            {
                File.WriteAllText(Path.Combine(SharedConfigDir, "toolmod_ready"),
                    DateTime.UtcNow.Ticks.ToString());
                Log.LogInfo("modifier ready signal written");
            }
            catch (Exception ex) { Log.LogWarning("ready signal failed: " + ex.Message); }

            if (!IsAndroid)
            {
                var startInfo = new ProcessStartInfo()
                {
                    FileName = ModifierPath,
                    ArgumentList =
                    {
                        Strings.RunModifierArgument,
                        BepInEx.Paths.GameRootPath,
                        Environment.ProcessId.ToString()
                    },
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                };
                Process.Start(startInfo);
            }
            Inited = true;

            if (!IsAndroid)
            {
                MakeKeyBindingUI();
                MakeGameKeyBindingUI();
            }
        }

        private void MakeKeyBindingUI()
        {
            var keyBindingUI =UnityEngine.Object.Instantiate(GameAPP.UIManager.UIPrefabs[UIType.UIConfigMenu], CacheObject.transform);
            keyBindingUI.name = "KeyBindingUI";
            keyBindingUI.AddComponent<KeyBindingUI>();
            GameAPP.UIManager.UIPrefabs.Add((UIType)999, keyBindingUI);
        }

        private void MakeGameKeyBindingUI()
        {
            var gameKeyBindingUI = UnityEngine.Object.Instantiate(GameAPP.UIManager.UIPrefabs[UIType.UIConfigMenu], CacheObject.transform);
            gameKeyBindingUI.name = "GameKeyBindingUI";
            gameKeyBindingUI.AddComponent<GameKeyBindingUI>();
            GameAPP.UIManager.UIPrefabs.Add((UIType)998, gameKeyBindingUI);
        }

        private static void MessageReceived(object? sender, string message)
        {
#if DEBUG
            //Instance.Log.LogMessage($"Received Command from Modifier UI: \n{message}");
#endif
            var data = JsonSerializer.Deserialize<SyncData>(message);
            if (DataProcessor.Instance is null) return;
            lock (DataProcessor.Instance.Buffer)
            {
                DataProcessor.Instance.Buffer.Enqueue(data);
            }
        }

        public override bool Unload()
        {
            SaveModState();
            if (GameAPP.config != null && GameAPP.config.gameSpeed == 0) GameAPP.config.gameSpeed = 1;
            try
            {
                SendCommand(new()
                {
                    Command = Strings.Exit,
                    Parameters = []
                });
                Thread.Sleep(100);
            }
            catch { }
            try
            {
                DataSync?.Stop();
                DataSync?.Dispose();
            }
            catch { }
            return true;
        }

        public void GenerateInitData()
        {
            try
            {
                SortedDictionary<int, string> plants = [];
                SortedDictionary<int, string> zombies = [];
                SortedDictionary<int, string> advBuffs = [];
                SortedDictionary<int, string> ultiBuffs = [];
                SortedDictionary<int, string> debuffs = [];
                SortedDictionary<int, string> investBuffs = [];
                SortedDictionary<int, string> unlockablePlants = [];
                SortedDictionary<int, string> bullets = [];
                SortedDictionary<int, string> firsts = [];
                SortedDictionary<int, string> seconds = [];
                foreach (var pt in GameAPP.resourcesManager.allPlants)
                {
                    string displayName = "";
                    try
                    {
                        var plantInfo = AlmanacDataLoader.GetPlantData(pt);
                        if (plantInfo != null && !string.IsNullOrWhiteSpace(plantInfo.name))
                            displayName = plantInfo.name.Trim();
                    }
                    catch
                    {
                    }

                    var item = !string.IsNullOrWhiteSpace(displayName)
                        ? $"{displayName} ({(int)pt})"
                        : $"{pt} ({(int)pt})";
                    plants[(int)pt] = item;
                }

                foreach (var zt in GameAPP.resourcesManager.allZombieTypes)
                {
                    string displayName = "";
                    try
                    {
                        var zombieInfo = AlmanacDataLoader.GetZombieData(zt);
                        if (zombieInfo != null && !string.IsNullOrWhiteSpace(zombieInfo.name))
                            displayName = zombieInfo.name.Trim();
                    }
                    catch
                    {
                    }

                    var item = !string.IsNullOrWhiteSpace(displayName)
                        ? $"{displayName} ({(int)zt})"
                        : $"{zt} ({(int)zt})";
                    zombies[(int)zt] = item;
                    ZombieHP.Add(zt, -1);
                }

                try
                {
                    // Advanced
                    if (TravelDictionary.advancedBuffsText != null && TravelDictionary.advancedBuffsText.Count > 0)
                    {
                        foreach (var advBuffKey in TravelDictionary.advancedBuffsText)
                        {
                            if (advBuffKey is null) continue;
                            var id = (int)advBuffKey.Key;
                            advBuffs.Add(id, $"#{id} {advBuffKey.value}");
                            AdvBuffs.Add((AdvBuff)id, 0);
                            InGameAdvBuffs.Add((AdvBuff)id, 0);
                        }
                    }

                    // Ultimate
                    if (TravelDictionary.ultimateBuffsText != null && TravelDictionary.ultimateBuffsText.Count > 0)
                    {
                        foreach (var ultiBuffKey in TravelDictionary.ultimateBuffsText)
                        {
                            if (ultiBuffKey is null) continue;
                            var id = (int)ultiBuffKey.Key;
                            ultiBuffs.Add(id, $"#{id} {ultiBuffKey.value}");
                            UltiBuffs.Add((UltiBuff)id, 0);
                            InGameUltiBuffs.Add((UltiBuff)id, 0);
                        }
                    }

                    // Debuff
                    if (TravelDictionary.debuffData != null && TravelDictionary.debuffData.Count > 0)
                    {
                        int maxDebuffKey = -1;
                        foreach (var kvp in TravelDictionary.debuffData)
                        {
                            int key = (int)kvp.Key;
                            if (key > maxDebuffKey) maxDebuffKey = key;
                        }

                        for (int id = 0; id <= maxDebuffKey; id++)
                        {
                            if (!TravelDictionary.debuffData!.ContainsKey((TravelDebuff)id)) continue;
                            string text = "";
                            try
                            {
                                if (TravelDictionary.debuffData != null &&
                                    TravelDictionary.debuffData.ContainsKey((TravelDebuff)id))
                                {
                                    text = TravelDictionary.debuffData[(TravelDebuff)id].Item1;
                                }
                            }
                            catch
                            {
                            }

                            debuffs.Add(id,
                                $"#{id} {(string.IsNullOrEmpty(text) ? ((TravelDebuff)id).ToString() : text)}");
                            Debuffs.Add((TravelDebuff)id, false);
                            InGameDebuffs.Add((TravelDebuff)id, false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.LogWarning(ex);
                }


                // Invest：优先通过 TravelMgr.GetText 读取真实文本，不直接访问 TravelMgr.InvestBuffsData
                try
                {
                    var values = Enum.GetValues(typeof(InvestBuff));
                    int maxInvestId = -1;

                    foreach (var val in values)
                    {
                        int id = (int)val;
                        if (id > maxInvestId) maxInvestId = id;
                        string? text = TryGetTravelTextViaReflection((InvestBuff)id);
                        if (string.IsNullOrWhiteSpace(text) ||
                            text.StartsWith("EnumValue", StringComparison.OrdinalIgnoreCase))
                            text = GetInvestBuffChineseName(id);
                        if (string.IsNullOrWhiteSpace(text))
                            text = ((InvestBuff)id).ToString();
                        investBuffs.Add(id, $"#{id} {text}");
                        InvestBuffs.Add((InvestBuff)id, false);
                        InGameInvestBuffs.Add((InvestBuff)id, false);
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(ex);
                }

                // Unlockable Plants (强究解锁)
                if (TravelDictionary.unlocksText != null && TravelDictionary.unlocksText.Count > 0)
                {
                    foreach (var unlockEntry in TravelDictionary.unlocksText)
                    {
                        if (unlockEntry is null) continue;
                        var id = (int)unlockEntry.Key;
                        unlockablePlants.Add(id, $"#{id} {unlockEntry.value}");
                        UnlockedPlants[(TravelUnlocks)id] = false;
                        InGameUnlockedPlants[(TravelUnlocks)id] = false;
                    }
                }

                foreach (var t in GameAPP.resourcesManager.allBullets)
                    if (GameAPP.resourcesManager.bulletPrefabs[t] != null)
                    {
                        var text =
                            $"{GameAPP.resourcesManager.bulletPrefabs[t].name} ({(int)t})";
                        bullets.Add((int)t, text);
                        BulletDamage.Add(t, -1);
                    }

                foreach (var first in Enum.GetValues(typeof(Zombie.FirstArmorType)))
                {
                    firsts.Add((int)first, $"{first}");
                    FirstArmorHP.Add((Zombie.FirstArmorType)first, -1);
                }

                foreach (var second in Enum.GetValues(typeof(Zombie.SecondArmorType)))
                {
                    seconds.Add((int)second, $"{second}");
                    SecondArmorHP.Add((Zombie.SecondArmorType)second, -1);
                }

                // Union with the previously persisted list: the game only registers the
                // full travel buff set after it loads its own data (in-game), so without
                // this the next launch would reset InitData back to the 2 entries visible
                // at the main menu.
                try
                {
                    var initDataPath = Path.Combine(SharedConfigDir, "InitData.json");
                    if (File.Exists(initDataPath))
                    {
                        var prev = JsonSerializer.Deserialize<InitData>(File.ReadAllText(initDataPath));
                        if (prev != null)
                        {
                            foreach (var kv in prev.AdvBuffs)
                            {
                                advBuffs.TryAdd(kv.Key, kv.Value);
                                Components.PatchDataCache.AdvBuffs.TryAdd((AdvBuff)kv.Key, 0);
                                Components.PatchDataCache.InGameAdvBuffs.TryAdd((AdvBuff)kv.Key, 0);
                            }
                            foreach (var kv in prev.UltiBuffs)
                            {
                                ultiBuffs.TryAdd(kv.Key, kv.Value);
                                Components.PatchDataCache.UltiBuffs.TryAdd((UltiBuff)kv.Key, 0);
                                Components.PatchDataCache.InGameUltiBuffs.TryAdd((UltiBuff)kv.Key, 0);
                            }
                            foreach (var kv in prev.Debuffs)
                            {
                                debuffs.TryAdd(kv.Key, kv.Value);
                                Components.PatchDataCache.Debuffs.TryAdd((TravelDebuff)kv.Key, false);
                                Components.PatchDataCache.InGameDebuffs.TryAdd((TravelDebuff)kv.Key, false);
                            }
                            foreach (var kv in prev.InvestBuffs)
                            {
                                investBuffs.TryAdd(kv.Key, kv.Value);
                                Components.PatchDataCache.InvestBuffs.TryAdd((InvestBuff)kv.Key, false);
                                Components.PatchDataCache.InGameInvestBuffs.TryAdd((InvestBuff)kv.Key, false);
                            }
                            foreach (var kv in prev.UnlockablePlants)
                            {
                                unlockablePlants.TryAdd(kv.Key, kv.Value);
                                Components.PatchDataCache.UnlockedPlants.TryAdd((TravelUnlocks)kv.Key, false);
                                Components.PatchDataCache.InGameUnlockedPlants.TryAdd((TravelUnlocks)kv.Key, false);
                            }
                            foreach (var kv in prev.Plants) plants.TryAdd(kv.Key, kv.Value);
                            foreach (var kv in prev.Zombies) zombies.TryAdd(kv.Key, kv.Value);
                            foreach (var kv in prev.Bullets) bullets.TryAdd(kv.Key, kv.Value);
                        }
                    }
                }
                catch (Exception ex) { Log.LogWarning("InitData union failed: " + ex.Message); }
                Log.LogInfo($"GenerateInitData: advText={TravelDictionary.advancedBuffsText?.Count ?? -1} adv={advBuffs.Count} ulti={ultiBuffs.Count} deb={debuffs.Count} unl={unlockablePlants.Count} plants={plants.Count} zombies={zombies.Count} invest={investBuffs.Count}");
                InitData = new()
                {
                    Plants = new(plants),
                    Zombies = new(zombies),
                    AdvBuffs = new(advBuffs),
                    UltiBuffs = new(ultiBuffs),
                    Bullets = new(bullets),
                    FirstArmors = new(firsts),
                    SecondArmors = new(seconds),
                    Debuffs = new(debuffs),
                    InvestBuffs = new(investBuffs),
                    UnlockablePlants = new(unlockablePlants)
                };
                Directory.CreateDirectory(SharedConfigDir);
                File.WriteAllText(Path.Combine(SharedConfigDir, "InitData.json"),
                    JsonSerializer.Serialize(InitData));
#if DEBUG
                /*Task.Run(() =>
                {
                    foreach (var line in plants)
                        Log.LogInfo($"Dumping Plant String: {line.Value}");
                    foreach (var line in zombies)
                        Log.LogInfo($"Dumping Zombie String: {line.Value}");
                    foreach (var line in advBuffs)
                        Log.LogInfo($"Dumping Advanced Buff String: {line.Value}");
                    foreach (var line in ultiBuffs)
                        Log.LogInfo($"Dumping Ultimate Buff String: {line.Value}");
                    foreach (var line in debuffs)
                        Log.LogInfo($"Dumping Debuff String: {line.Value}");
                    foreach (var line in investBuffs)
                        Log.LogInfo($"Dumping Invest Buff String: {line.Value}");
                    foreach (var line in bullets)
                        Log.LogInfo($"Dumping Bullet String: {line.Value}");
                });*/
#endif
            }
            catch
            {
            }
        }

        /// <summary>
        /// Re-reads the travel buff text dictionaries and grows InitData when the
        /// game has loaded more entries. Safe: only reads TravelDictionary, never
        /// creates a TravelMgr (which crashed when forced at the main menu).
        /// </summary>
        public void RefreshBuffList()
        {
            try
            {
                if (InitData == null) return;
                bool changed = false;

                var advText = TravelDictionary.advancedBuffsText;
                if (advText != null && advText.Count > InitData.AdvBuffs.Count)
                {
                    var d = new SortedDictionary<int, string>();
                    foreach (var kv in advText)
                    {
                        d[(int)kv.Key] = $"#{(int)kv.Key} {kv.Value}";
                        Components.PatchDataCache.AdvBuffs.TryAdd(kv.Key, 0);
                        Components.PatchDataCache.InGameAdvBuffs.TryAdd(kv.Key, 0);
                    }
                    InitData.AdvBuffs = new(d);
                    changed = true;
                }

                var ultiText = TravelDictionary.ultimateBuffsText;
                if (ultiText != null && ultiText.Count > InitData.UltiBuffs.Count)
                {
                    var d = new SortedDictionary<int, string>();
                    foreach (var kv in ultiText)
                    {
                        d[(int)kv.Key] = $"#{(int)kv.Key} {kv.Value}";
                        Components.PatchDataCache.UltiBuffs.TryAdd(kv.Key, 0);
                        Components.PatchDataCache.InGameUltiBuffs.TryAdd(kv.Key, 0);
                    }
                    InitData.UltiBuffs = new(d);
                    changed = true;
                }

                var debData = TravelDictionary.debuffData;
                if (debData != null && debData.Count > InitData.Debuffs.Count)
                {
                    var d = new SortedDictionary<int, string>();
                    foreach (var kv in debData)
                    {
                        d[(int)kv.Key] = $"#{(int)kv.Key} {kv.Value.Item1}";
                        Components.PatchDataCache.Debuffs.TryAdd(kv.Key, false);
                        Components.PatchDataCache.InGameDebuffs.TryAdd(kv.Key, false);
                    }
                    InitData.Debuffs = new(d);
                    changed = true;
                }

                var unlText = TravelDictionary.unlocksText;
                if (unlText != null && unlText.Count > InitData.UnlockablePlants.Count)
                {
                    var d = new SortedDictionary<int, string>();
                    foreach (var kv in unlText)
                    {
                        d[(int)kv.Key] = $"#{(int)kv.Key} {kv.Value}";
                        Components.PatchDataCache.UnlockedPlants.TryAdd(kv.Key, false);
                        Components.PatchDataCache.InGameUnlockedPlants.TryAdd(kv.Key, false);
                    }
                    InitData.UnlockablePlants = new(d);
                    changed = true;
                }

                if (changed)
                {
                    Directory.CreateDirectory(SharedConfigDir);
                    File.WriteAllText(Path.Combine(SharedConfigDir, "InitData.json"),
                        JsonSerializer.Serialize(InitData));
                    DumpState();
                    Log.LogInfo($"RefreshBuffList: adv={InitData.AdvBuffs.Count} ulti={InitData.UltiBuffs.Count} deb={InitData.Debuffs.Count} unl={InitData.UnlockablePlants.Count}");
                }
            }
            catch (Exception ex) { Log.LogWarning("RefreshBuffList failed: " + ex.Message); }
        }

        public void SendCommand(SyncData data) =>
            Task.Run(async () => await DataSync.SendAsync(JsonSerializer.Serialize(data)));

        /// <summary>
        /// Writes a snapshot of the primitive PatchDataCache fields to
        /// toolmod_state.json so the launcher UI can restore switch states.
        /// Keys are lowercase-first (matching the launcher's stateKey()).
        /// </summary>
        public void DumpState()
        {
            try
            {
                Directory.CreateDirectory(SharedConfigDir);
                File.WriteAllText(Path.Combine(SharedConfigDir, "toolmod_state.json"),
                    JsonSerializer.Serialize(DumpStateDict(), StateJsonOptions));
                Log.LogInfo("DumpState: state written to " + SharedConfigDir);
                File.WriteAllText(Path.Combine(SharedConfigDir, "toolmod_buffs.json"),
                    JsonSerializer.Serialize(new
                    {
                        initial = new
                        {
                            adv = Components.PatchDataCache.AdvBuffs.Where(kv => kv.Value > 0).Select(kv => (int)kv.Key).ToArray(),
                            ulti = Components.PatchDataCache.UltiBuffs.Where(kv => kv.Value > 0).Select(kv => (int)kv.Key).ToArray(),
                            unl = Components.PatchDataCache.UnlockedPlants.Where(kv => kv.Value).Select(kv => (int)kv.Key).ToArray(),
                            deb = Components.PatchDataCache.Debuffs.Where(kv => kv.Value).Select(kv => (int)kv.Key).ToArray(),
                            inv = Components.PatchDataCache.InvestBuffs.Where(kv => kv.Value).Select(kv => (int)kv.Key).ToArray(),
                        },
                        ingame = new
                        {
                            adv = Components.PatchDataCache.InGameAdvBuffs.Where(kv => kv.Value > 0).Select(kv => (int)kv.Key).ToArray(),
                            ulti = Components.PatchDataCache.InGameUltiBuffs.Where(kv => kv.Value > 0).Select(kv => (int)kv.Key).ToArray(),
                            unl = Components.PatchDataCache.InGameUnlockedPlants.Where(kv => kv.Value).Select(kv => (int)kv.Key).ToArray(),
                            deb = Components.PatchDataCache.InGameDebuffs.Where(kv => kv.Value).Select(kv => (int)kv.Key).ToArray(),
                            inv = Components.PatchDataCache.InGameInvestBuffs.Where(kv => kv.Value).Select(kv => (int)kv.Key).ToArray(),
                        },
                    }));
            }
            catch (Exception ex) { Log.LogWarning("DumpState failed: " + ex); }
        }

        private static Dictionary<string, object> DumpStateDict()
        {
            var dict = new Dictionary<string, object>();
            foreach (var p in typeof(Components.PatchDataCache)
                         .GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                if (!p.CanRead) continue;
                var t = p.PropertyType;
                if (t != typeof(bool) && t != typeof(int) && t != typeof(float) && t != typeof(string))
                    continue;
                try
                {
                    var name = p.Name;
                    var key = char.ToLowerInvariant(name[0]) + name.Substring(1);
                    dict[key] = p.GetValue(null) ?? "";
                }
                catch { }
            }
            return dict;
        }

        /// <summary>保存全部修改条目（仅当 ModSaveEnabled 开启时）。</summary>
        public void SaveModState()
        {
            try
            {
                if (!Components.PatchDataCache.ModSaveEnabled) return;
                Directory.CreateDirectory(SharedConfigDir);
                File.WriteAllText(Path.Combine(SharedConfigDir, "ModifierSave.json"),
                    JsonSerializer.Serialize(DumpStateDict()));
                Log.LogMessage("修改条目已保存");
            }
            catch (Exception ex) { Log.LogWarning("保存修改条目失败: " + ex.Message); }
        }

        /// <summary>启动时加载修改条目。</summary>
        public void LoadModState()
        {
            try
            {
                var path = Path.Combine(SharedConfigDir, "ModifierSave.json");
                if (!File.Exists(path)) return;
                var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                    File.ReadAllText(path), StateJsonOptions);
                if (dict == null) return;
                foreach (var p in typeof(Components.PatchDataCache)
                             .GetProperties(BindingFlags.Public | BindingFlags.Static))
                {
                    if (!p.CanWrite) continue;
                    var name = p.Name;
                    var key = char.ToLowerInvariant(name[0]) + name.Substring(1);
                    if (!dict.TryGetValue(key, out var el)) continue;
                    try
                    {
                        if (p.PropertyType == typeof(bool)) p.SetValue(null, el.GetBoolean());
                        else if (p.PropertyType == typeof(int)) p.SetValue(null, el.GetInt32());
                        else if (p.PropertyType == typeof(float)) p.SetValue(null, el.GetSingle());
                        else if (p.PropertyType == typeof(string)) p.SetValue(null, el.GetString());
                    }
                    catch { }
                }
                Log.LogMessage("修改条目已加载");
            }
            catch (Exception ex) { Log.LogWarning("加载修改条目失败: " + ex.Message); }
        }

        public static ModCore Instance;

        private IToolSync DataSync { get; set; }
        public GameObject ModifierObject { get; set; }
        public GameObject CacheObject{ get; set; }
        public BootConfig BootConfig { get; set; }
        public string ModifierPath { get; set; }
        public bool Inited { get; private set; } = false;
        public InitData InitData { get; private set; }
    }
}