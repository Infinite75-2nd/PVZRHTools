using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using AlmanacData;
using BepInEx;
using BepInEx.Logging;
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

namespace ToolMod
{
    [BepInPlugin("infinite75.toolmod", "PVZRHTools", Strings.ModifierVersion)]
    public class ModCore : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = Encoding.UTF8;
            var bootConfigString = File.ReadAllText(Path.Combine(BepInEx.Paths.GameRootPath, Paths.BootConfigPath));
            BootConfig = JsonSerializer.Deserialize<BootConfig>(bootConfigString);
            if (!BootConfig.ModifierEnabled) return;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<DataProcessor>();
            ClassInjector.RegisterTypeInIl2Cpp<ToolsUpdater>();
            Instance = this;
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => Unload();
        }

        public void LateInit()
        {
            if (Inited) return;
            ModifierObject = new("PVZRHTools");
            ModifierObject.AddComponent<DataProcessor>();
            ModifierObject.AddComponent<ToolsUpdater>();
            GenerateInitData();

            // 加载并应用保存的设置
            SettingsLoader.LoadAndApplySettings();

            DataSync = new DataSync(Strings.PipeName);
            DataSync.Connected += (sender, e) => { Log.LogMessage("修改器已连接"); };
            DataSync.MessageReceived += MessageReceived;
            DataSync.Disconnected += (sender, e) =>
            {
                Log.LogMessage("修改器已断开");
                Environment.Exit(0);
            };
            DataSync.Start();
            var startInfo = new ProcessStartInfo()
            {
                FileName = BootConfig.ModifierPath,
                ArgumentList = { Strings.RunModifierArgument, BepInEx.Paths.GameRootPath },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            var process = Process.Start(startInfo);
            process?.OutputDataReceived += (sender, e) =>
            {
                // 将子进程的每一行标准输出打印到父进程的控制台
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.WriteLine($"[Info: PVZRHTools] {e.Data}");
                }
            };

            process?.ErrorDataReceived += (sender, e) =>
            {
                // 将子进程的每一行标准错误打印到父进程的控制台
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Console.WriteLine($"[Error: PVZRHTools] {e.Data}");
                }
            };
            Inited = true;
        }

        private static void MessageReceived(object? sender, string message)
        {
#if DEBUG
            Instance.Log.LogMessage($"Received Command from Modifier UI: \n{message}");
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
            if (GameAPP.config != null && GameAPP.config.gameSpeed == 0) GameAPP.config.gameSpeed = 1;
            SendCommand(new()
            {
                Command = Strings.Exit,
                Parameters = []
            });
            Thread.Sleep(100);
            DataSync.Stop();
            DataSync.Dispose();
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
                    if (TravelDictionary.advancedBuffsText is not null && TravelDictionary.advancedBuffsText.Count > 0)
                    {
                        foreach (var advBuffKey in TravelDictionary.advancedBuffsText)
                        {
                            if (advBuffKey is null) continue;
                            var id = (int)advBuffKey.Key;
                            advBuffs.Add(id, $"#{id} {advBuffKey.value}");
                            AdvBuffs.Add((AdvBuff)id, 0);
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
                        }
                    }

                    // Debuff
                    if (TravelDictionary.debuffData is not null && TravelDictionary.debuffData.Count > 0)
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
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(ex);
                }

                foreach (var t in GameAPP.resourcesManager.allBullets)
                    if (GameAPP.resourcesManager.bulletPrefabs[t] is not null)
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
                    InvestBuffs = new(investBuffs)
                };
                File.WriteAllText(Path.Combine(BepInEx.Paths.GameRootPath, Paths.InitDataPath),
                    JsonSerializer.Serialize(InitData));
#if DEBUG
                Task.Run(() =>
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
                });
#endif
            }
            catch
            {
            }
        }

        public void SendCommand(SyncData data) =>
            Task.Run(async () => await DataSync.SendAsync(JsonSerializer.Serialize(data)));

        public static ModCore Instance;

        private DataSync DataSync { get; set; }
        public GameObject ModifierObject { get; set; }
        public BootConfig BootConfig { get; set; }
        public bool Inited { get; private set; } = false;
        public InitData InitData { get; private set; }
    }
}