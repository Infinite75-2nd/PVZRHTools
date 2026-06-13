using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Core;
using GameLevel.RogueShooting;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ToolData;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace ToolMod;

public static class Utils
{
    public static bool InGame => Board.Instance != null && !Board.Instance.IsDestroyed() &&
                                 GameAPP.theGameStatus is not GameStatus.OpenOptions;

    public static Action<List<string>> SimpleSyncEnum<T>(Expression<Func<T>> propertyExpression) where T : Enum
        => args => CreateSetter(propertyExpression)(GetEnumFromInt<T>(Convert.ToInt32(args[0])));

    public static Action<List<string>> SimpleSyncBool(Expression<Func<bool>> propertyExpression,
        Action? callBack = null) =>
        args =>
        {
            CreateSetter(propertyExpression)(Convert.ToBoolean(args[0]));
            callBack?.Invoke();
        };

    public static Action<List<string>> SimpleSyncFloat(Expression<Func<float>> propertyExpression,
        Action? callBack = null)
        => args =>
        {
            CreateSetter(propertyExpression)(Convert.ToSingle(args[0]));
            callBack?.Invoke();
        };

    public static Action<List<string>> SimpleSyncInt(Expression<Func<int>> propertyExpression, Action? callBack = null)
        => args =>
        {
            CreateSetter(propertyExpression)(Convert.ToInt32(args[0]));
            callBack?.Invoke();
        };

    public static Action<List<string>> SimpleSyncString(Expression<Func<string>> propertyExpression,
        Action? callBack = null)
        => args =>
        {
            CreateSetter(propertyExpression)(args[0]);
            callBack?.Invoke();
        };

    public static Action<List<string>> SimpleSyncKeyCode(Expression<Func<KeyCode>> propertyExpression,
        Action? callBack = null)
        => args =>
        {
            CreateSetter(propertyExpression)((KeyCode)Enum.Parse(typeof(KeyCode), args[0]));
            callBack?.Invoke();
        };

    public static T GetEnumFromInt<T>(int value) where T : Enum => (T)Enum.ToObject(typeof(T), value);
    public static T GetRandomItem<T>(this IList<T> list) => list[Random.RandomRangeInt(0, list.Count)];

    public static Action<T> CreateSetter<T>(Expression<Func<T>> propertyExpression)
    {
        if (propertyExpression == null)
            throw new ArgumentNullException(nameof(propertyExpression));

        // 处理可能的类型转换（例如将值类型转换为 object）
        var body = propertyExpression.Body;
        if (body is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unary)
        {
            body = unary.Operand;
        }

        // 确保主体是成员访问表达式
        if (body is not MemberExpression memberExpr)
            throw new ArgumentException("Lambda body must be a property or field access.", nameof(propertyExpression));

        // 验证成员是否可写
        if (memberExpr.Member is PropertyInfo propInfo)
        {
            if (!propInfo.CanWrite)
                throw new ArgumentException($"Property '{propInfo.Name}' does not have a setter.");
        }
        else if (memberExpr.Member is FieldInfo fieldInfo)
        {
            if (fieldInfo.IsInitOnly) // 只读字段（如 readonly）
                throw new ArgumentException($"Field '{fieldInfo.Name}' is read-only.");
        }
        else
        {
            throw new ArgumentException("Lambda body must be a property or field access.");
        }

        // 创建参数表达式，用于接收要设置的值
        var valueParam = Expression.Parameter(typeof(T), "value");

        // 构建赋值表达式： memberExpr = valueParam
        var assign = Expression.Assign(memberExpr, valueParam);

        // 将赋值表达式编译为 Action<T>
        var setterLambda = Expression.Lambda<Action<T>>(assign, valueParam);
        return setterLambda.Compile();
    }


    /// <summary>
    /// 统一获取 TravelMgr（兼容多种场景）
    /// </summary>
    /// <param name="autoCreate">是否在找不到时自动创建 TravelMgr（仅在需要修改词条时使用）</param>
    public static TravelMgr? ResolveTravelMgr(bool autoCreate = false)
    {
        TravelMgr? travelMgr = null;
        try
        {
            travelMgr = TravelMgr.Instance;
        }
        catch
        {
        }

        if (travelMgr == null && GameAPP.Instance != null)
        {
            travelMgr = GameAPP.Instance.GetComponent<TravelMgr>();
        }

        if (travelMgr == null)
        {
            travelMgr = UnityEngine.Object.FindObjectOfType<TravelMgr>();
        }

        if (travelMgr == null && GameAPP.board != null)
        {
            travelMgr = GameAPP.board.GetComponent<TravelMgr>();
        }

        // 仅在需要修改词条时才自动创建 TravelMgr
        // GetOrAdd TravelMgr + 设置 boardTag.isTravel/enableTravelBuff
        if (travelMgr == null && autoCreate && InGame && GameAPP.Instance != null)
        {
            try
            {
                travelMgr = GameAPP.Instance.GetComponent<TravelMgr>();
                if (travelMgr == null)
                {
                    travelMgr = GameAPP.Instance.AddComponent<TravelMgr>();

                    // 关键修复：自动创建 TravelMgr 时，同步旗帜波状态，避免旗帜波检测失效
                    // 确保 _lastHugeWaveState 与当前游戏状态一致
                    if (Board.Instance != null)
                    {
                        LastHugeWaveState = Board.Instance.isHugeWave;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ModCore.Instance.Log.LogWarning($"ResolveTravelMgr: 自动创建 TravelMgr 失败: {ex.Message}");
            }
        }

        return travelMgr;
    }


    public static void SyncInGameBuffs()
    {
        if (!InGame) return;
        ModCore.Instance.SendCommand(new SyncData()
        {
            Command = Strings.UpdateAllBuffs,
            Parameters =
            [
                JsonSerializer.Serialize(new SyncTravelBuffs()
                {
                    InGameAdvBuffs = InGameAdvBuffs.ToDictionary(x => (int)x.Key, x => x.Value),
                    InGameUltiBuffs = InGameUltiBuffs.ToDictionary(x => (int)x.Key, x => x.Value),
                    InGameDebuffs = InGameDebuffs.ToDictionary(x => (int)x.Key, x => x.Value),
                    InGameInvestBuffs = InGameInvestBuffs.ToDictionary(x => (int)x.Key, x => x.Value)
                })
            ]
        });
    }

    /// <summary>
    /// 实时同步游戏词条状态到修改器
    /// 当游戏中解锁或关闭词条时调用此方法，更新 InGame*Buffs 数组并发送到UI
    /// </summary>
    public static void SyncGameBuffsToModifier()
    {
        // 从游戏状态读取所有词条，更新 InGame*Buffs 数组
        foreach (var adv in InGameAdvBuffs.Keys)
        {
            try
            {
                InGameAdvBuffs[adv] = Lawnf.TravelAdvanced(adv) ? 1 : 0;
            }
            catch
            {
            }
        }

        foreach (var ulti in InGameUltiBuffs.Keys)
        {
            try
            {
                InGameUltiBuffs[ulti] = Lawnf.TravelUltimate(ulti) ? 1 : 0;
            }
            catch
            {
            }
        }

        foreach (var debuff in InGameDebuffs.Keys)
        {
            try
            {
                InGameDebuffs[debuff] = Lawnf.TravelDebuff(debuff);
            }
            catch
            {
            }
        }

        // 发送更新后的数据到UI
        // 1. 先发送词条列表（如果需要更新词条列表）
        ReloadAndSendBuffsData();
        SyncInGameBuffs();
    }

    /// <summary>
    /// 重新读取所有词条数据（包括MOD添加的）并发送给UI
    /// 在进入游戏后调用，确保MOD词条已注册
    /// </summary>
    public static void ReloadAndSendBuffsData()
    {
        try
        {
            var travelMgr = ResolveTravelMgr(autoCreate: true);
            if (travelMgr == null)
            {
                ModCore.Instance.Log?.LogWarning("[PVZRHTools] ReloadAndSendBuffsData: 无法找到 TravelMgr 组件");
                return;
            }

            if (TravelDictionary.advancedBuffsText == null ||
                TravelDictionary.ultimateBuffsText == null ||
                TravelDictionary.debuffData == null)
            {
                ModCore.Instance.Log?.LogWarning("[PVZRHTools] ReloadAndSendBuffsData: 词条数据未初始化");
                return;
            }

            bool needRefresh = false;
            // 更新本地数组大小（如果MOD添加了新词条）
            // 直接使用 Count 作为数组大小
            if (AdvBuffs.Count < TravelDictionary.advancedBuffsText.Count)
            {
                foreach (var adv in TravelDictionary.advancedBuffsText)
                {
                    AdvBuffs.TryAdd(adv.Key, 0);
                    InGameAdvBuffs.TryAdd(adv.Key, 0);
                }

                needRefresh = true;
            }

            if (UltiBuffs.Count < TravelDictionary.ultimateBuffsText.Count)
            {
                foreach (var ulti in TravelDictionary.ultimateBuffsText)
                {
                    UltiBuffs.TryAdd(ulti.Key, 0);
                    InGameUltiBuffs.TryAdd(ulti.Key, 0);
                }

                needRefresh = true;
            }

            if (Debuffs.Count < TravelDictionary.debuffData.Count)
            {
                foreach (var debuff in TravelDictionary.debuffData)
                {
                    Debuffs.TryAdd(debuff.Key, false);
                    InGameDebuffs.TryAdd(debuff.Key, false);
                    needRefresh = true;
                }
            }

            if (!needRefresh) return;

            SortedDictionary<int, string> advBuffs = [];
            foreach (var kvp in TravelDictionary.advancedBuffsText)
            {
                advBuffs.Add((int)kvp.Key, $"#{(int)kvp.Key} {kvp.Value}");
            }

            SortedDictionary<int, string> ultiBuffs = [];
            foreach (var kvp in TravelDictionary.ultimateBuffsText)
            {
                ultiBuffs.Add((int)kvp.Key, $"#{(int)kvp.Key} {kvp.Value}");
            }

            SortedDictionary<int, string> debuffs = [];
            foreach (var kvp in TravelDictionary.debuffData)
            {
                debuffs.Add((int)kvp.Key, $"#{(int)kvp.Key} {kvp.Value}");
            }

            // 更新并保存InitData
            ModCore.Instance.InitData.AdvBuffs = new(advBuffs);
            ModCore.Instance.InitData.UltiBuffs = new(ultiBuffs);
            ModCore.Instance.InitData.Debuffs = new(debuffs);

            // 保存更新后的InitData
            File.WriteAllText(Path.Combine(BepInEx.Paths.GameRootPath, Paths.InitDataPath),
                JsonSerializer.Serialize(ModCore.Instance.InitData));
            ModCore.Instance.SendCommand(new SyncData()
            {
                Command = Strings.ReloadInitData,
                Parameters = []
            });
        }
        catch (System.Exception ex)
        {
            ModCore.Instance.Log?.LogError($"[PVZRHTools] ReloadAndSendBuffsData 异常: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public static void UpdateInGameBuffs()
    {
        try
        {
            var travelMgr = ResolveTravelMgr(autoCreate: true);
            if (travelMgr == null)
            {
                ModCore.Instance.Log?.LogWarning("UpdateInGameBuffs: 无法找到 TravelMgr，可能未进入关卡/未初始化");
                return;
            }

            var data = travelMgr.data;
            if (data == null)
            {
                ModCore.Instance.Log?.LogWarning("UpdateInGameBuffs: travelMgr.data 为空，无法同步词条状态");
                return;
            }

            // 高级词条：以游戏为主，但对“手动取消勾选”的该词条执行一次关闭
            // - 勾选为 true 且当前未解锁 -> 只补充解锁这一条
            // - 从 true 取消勾选（上一次为 true、本次为 false） -> 只对这一条从当前局移除，相当于“关闭该词条”，不动其它词条。
            if (TravelDictionary.advancedBuffsText != null)
            {
                foreach (var kvp in TravelDictionary.advancedBuffsText)
                {
                    if (!InGameAdvBuffs.ContainsKey(kvp.Key)) continue;
                    bool unlocked =
                        (data.advBuffs != null && data.advBuffs.Contains(kvp.Key));

                    if (!unlocked && InGameAdvBuffs[kvp.Key] > 0)
                    {
                        if (!InGame)
                        {
                            continue;
                        }

                        try
                        {
                            travelMgr.GetNormalBuff(kvp.Key);
                        }
                        catch (Exception ex)
                        {
                            ModCore.Instance.Log?.LogWarning(
                                $"UpdateInGameBuffs: 解锁高级词条 {kvp.Key} (id={kvp.Key}) 失败: {ex.Message}");
                        }
                    }
                    else if (InGameAdvBuffs[kvp.Key] is 0 && AllowBuffRemoval && unlocked)
                    {
                        try
                        {
                            data.advBuffs?.Remove(kvp.Key);
                            ModCore.Instance.Log?.LogInfo(
                                $"UpdateInGameBuffs: 关闭高级词条 {kvp.Key} (id={kvp.Key})，已从当前局移除");
                        }
                        catch (System.Exception ex)
                        {
                            ModCore.Instance.Log?.LogWarning(
                                $"UpdateInGameBuffs: 移除高级词条 {kvp.Key} (id={kvp.Key}) 失败: {ex.Message}");
                        }
                    }
                }
            }

            // 究极词条：以游戏为主，但对“手动取消勾选”的该词条执行一次关闭
            // - 勾选 true 且未解锁 -> 解锁
            // - 从 true 取消勾选 -> 只对这一条从当前局移除，其它究极词条不受影响。
            if (TravelDictionary.ultimateBuffsText != null)
            {
                foreach (var kvp in TravelDictionary.ultimateBuffsText)
                {
                    if (!InGameUltiBuffs.ContainsKey(kvp.Key)) continue;
                    bool unlocked =
                        (data.ultiBuffs != null && data.ultiBuffs.Contains(kvp.Key)) ||
                        (data.ultiBuffs_lv2 != null && data.ultiBuffs_lv2.Contains(kvp.Key));

                    if (!unlocked && InGameUltiBuffs[kvp.Key] > 0)
                    {
                        if (!InGame)
                        {
                            continue;
                        }

                        try
                        {
                            travelMgr.GetUltiBuff(kvp.Key, InGameUltiBuffs[kvp.Key] is 2);
                            if (InGameUltiBuffs[kvp.Key] is not 2)
                            {
                                data.ultiBuffs_lv2?.Remove(kvp.Key);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            ModCore.Instance.Log?.LogWarning(
                                $"UpdateInGameBuffs: 解锁强究词条 {kvp.Key} (id={kvp.Key}) 失败: {ex.Message}");
                        }
                    }
                    else if (InGameUltiBuffs[kvp.Key] is 0 && AllowBuffRemoval && unlocked)
                    {
                        try
                        {
                            data.ultiBuffs?.Remove(kvp.Key);
                            data.ultiBuffs_lv2?.Remove(kvp.Key);
                            ModCore.Instance.Log?.LogInfo(
                                $"UpdateInGameBuffs: 关闭强究词条 {kvp.Key} (id={kvp.Key})，已从当前局移除");
                        }
                        catch (System.Exception ex)
                        {
                            ModCore.Instance.Log?.LogWarning(
                                $"UpdateInGameBuffs: 移除强究词条 {kvp.Key} (id={kvp.Key}) 失败: {ex.Message}");
                        }
                    }
                }
            }

            // 负面词条（Debuff）：以游戏为主，但对“手动取消勾选”的该词条执行一次关闭
            // - 勾选 true 且未解锁 -> 解锁
            // - 从 true 取消勾选 -> 只对这一条从当前局移除，其它 Debuff 不受影响。
            if (TravelDictionary.debuffData != null)
            {
                foreach (var kvp in TravelDictionary.debuffData)
                {
                    if (!InGameDebuffs.ContainsKey(kvp.Key)) continue;
                    bool unlocked =
                        (data.travelDebuffs != null && data.travelDebuffs.Contains(kvp.Key));

                    if (!unlocked && InGameDebuffs[kvp.Key])
                    {
                        if (!InGame)
                        {
                            continue;
                        }

                        try
                        {
                            travelMgr.GetDebuff(kvp.Key);
                        }
                        catch (System.Exception ex)
                        {
                            ModCore.Instance.Log?.LogWarning(
                                $"UpdateInGameBuffs: 解锁僵尸词条 {kvp.Key} (id={kvp.Key}) 失败: {ex.Message}");
                        }
                    }
                    else if (!InGameDebuffs[kvp.Key] && AllowBuffRemoval && unlocked)
                    {
                        try
                        {
                            data.travelDebuffs?.Remove(kvp.Key);
                            ModCore.Instance.Log?.LogInfo(
                                $"UpdateInGameBuffs: 关闭僵尸词条 {kvp.Key} (id={kvp.Key})，已从当前局移除");
                        }
                        catch (System.Exception ex)
                        {
                            ModCore.Instance.Log?.LogWarning(
                                $"UpdateInGameBuffs: 移除僵尸词条 {kvp.Key} (id={kvp.Key}) 失败: {ex.Message}");
                        }
                    }
                }
            }

            // 投资词条（Invest）：以游戏为主，但对“手动取消勾选”的该词条执行一次关闭
            // - 勾选 true 且未解锁 -> 解锁
            // - 从 true 取消勾选 -> 只对这一条从当前局移除，其它投资词条不受影响。
            foreach (var invest in InGameInvestBuffs.Keys)
            {
                bool unlocked = data.investmentBuffs != null && data.investmentBuffs.Contains(invest);

                if (InGameInvestBuffs[invest] && !unlocked)
                {
                    try
                    {
                        travelMgr.GetInvestBuff(invest);
                    }
                    catch (System.Exception ex)
                    {
                        ModCore.Instance.Log?.LogWarning(
                            $"UpdateInGameBuffs: 解锁投资词条 {invest} (id={invest}) 失败: {ex.Message}");
                    }
                }
                // 从 true 取消勾选时，且允许移除并且当前已解锁，则关闭这一条投资词条
                else if (!InGameInvestBuffs[invest] && AllowBuffRemoval && unlocked)
                {
                    try
                    {
                        data.investmentBuffs?.Remove(invest);
                        ModCore.Instance.Log?.LogInfo($"UpdateInGameBuffs: 关闭投资词条 {invest} (id={invest})，已从当前局移除");
                    }
                    catch (System.Exception ex)
                    {
                        ModCore.Instance.Log?.LogWarning(
                            $"UpdateInGameBuffs: 移除投资词条 {invest} (id={invest}) 失败: {ex.Message}");
                    }
                }
            }

            // 关键：设置 BoardTag 标志，使游戏识别并应用词条系统
            try
            {
                if (Board.Instance != null && GameAPP.board != null)
                {
                    var board = GameAPP.board.GetComponent<Board>();
                    if (board != null)
                    {
                        var boardTag = board.boardTag;
                        boardTag.isTravel = true;
                        boardTag.enableTravelBuff = true;
                        Board.Instance.boardTag = boardTag;
                    }
                }
            }
            catch (Exception ex)
            {
                ModCore.Instance.Log?.LogError($"UpdateInGameBuffs: 设置 BoardTag 失败: {ex.Message}\n{ex.StackTrace}");
            }
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log?.LogError($"UpdateInGameBuffs: 异常: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public static string CompressString(string text)
    {
        var buffer = Encoding.UTF8.GetBytes(text);
        using var memoryStream = new MemoryStream();
        using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
        {
            gZipStream.Write(buffer, 0, buffer.Length);
        }

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public static string DecompressString(string compressedText)
    {
        var gZipBuffer = Convert.FromBase64String(compressedText);
        using var memoryStream = new MemoryStream(gZipBuffer);
        using var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        gZipStream.CopyTo(resultStream);
        var buffer = resultStream.ToArray();
        return Encoding.UTF8.GetString(buffer);
    }

    /// <summary>
    /// 给植物上星辉buff的辅助方法
    /// </summary>
    /// <param name="plant">目标植物</param>
    /// <returns>是否成功上星辉</returns>
    public static bool ApplyStarUpBuff(Plant plant)
    {
        if (plant == null)
        {
            ModCore.Instance.Log?.LogWarning("植物为 null，无法上星辉buff");
            return false;
        }

        try
        {
            // 先调用 StarUp()，然后设置属性，最后更新图标
            var starUpMethod = typeof(Plant).GetMethod("StarUp",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            // 尝试作为属性访问
            var starUpProperty = typeof(Plant).GetProperty("starUp",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            // 如果属性不存在，尝试作为字段访问
            var starUpField = typeof(Plant).GetField("starUp",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var updateStarIconMethod = typeof(Plant).GetMethod("UpdateStarIcon",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            // 步骤1：调用 StarUp 方法（如果存在）
            if (starUpMethod != null)
            {
                try
                {
                    starUpMethod.Invoke(plant, null);
                }
                catch (Exception ex)
                {
                    ModCore.Instance.Log?.LogWarning($"调用 StarUp() 方法时出错: {ex.Message}");
                }
            }

            // 步骤2：设置 starUp 属性或字段为 true
            bool setSuccess = false;
            if (starUpProperty != null)
            {
                try
                {
                    starUpProperty.SetValue(plant, true);
                    setSuccess = true;
                }
                catch (Exception ex)
                {
                    ModCore.Instance.Log?.LogWarning($"通过属性设置 starUp 时出错: {ex.Message}");
                }
            }
            else if (starUpField != null)
            {
                try
                {
                    starUpField.SetValue(plant, true);
                    setSuccess = true;
                }
                catch (Exception ex)
                {
                    ModCore.Instance.Log?.LogWarning($"通过字段设置 starUp 时出错: {ex.Message}");
                }
            }
            else
            {
                ModCore.Instance.Log?.LogError("无法找到 Plant.starUp 属性或字段");
                return false;
            }

            if (!setSuccess)
            {
                ModCore.Instance.Log?.LogError("设置 starUp 失败");
                return false;
            }

            // 步骤3：调用 UpdateStarIcon 更新UI显示
            if (updateStarIconMethod != null)
            {
                try
                {
                    updateStarIconMethod.Invoke(plant, null);
                }
                catch (Exception ex)
                {
                    ModCore.Instance.Log?.LogWarning($"调用 UpdateStarIcon() 方法时出错: {ex.Message}");
                }
            }

            // 验证是否设置成功
            bool starUpValue = false;
            if (starUpProperty != null)
            {
                starUpValue = (bool)(starUpProperty.GetValue(plant) ?? false);
            }
            else if (starUpField != null)
            {
                starUpValue = (bool)(starUpField.GetValue(plant) ?? false);
            }

            if (starUpValue)
            {
                return true;
            }
            else
            {
                ModCore.Instance.Log?.LogWarning($"设置 starUp 后验证失败，值仍为 false");
                return false;
            }
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log?.LogError($"给植物上星辉buff时发生错误: {ex.Message}\n{ex.StackTrace}");
            return false;
        }
    }


    public static string? TryGetTravelTextViaReflection(object buff)
    {
        try
        {
            var travelMgr = ResolveTravelMgr();
            if (travelMgr == null)
                return null;

            var method = typeof(TravelMgr).GetMethod("GetText", new[] { typeof(object) });
            if (method == null)
                return null;

            var result = method.Invoke(travelMgr, new[] { buff });
            var text = result?.ToString();
            if (!string.IsNullOrWhiteSpace(text))
                return text;
        }
        catch
        {
        }

        return null;
    }

    /// <summary>
    /// 获取出怪列表数据
    /// </summary>
    public static Dictionary<int, Dictionary<int, int>>? GetZombieListData()
    {
        try
        {
            // 直接访问InitZombieList.zombieList（如果是public属性）
            // 如果无法直接访问，则使用反射
            Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.List<ZombieSpawnData>>? zombieList =
                null;

            try
            {
                // 尝试直接访问
                zombieList = InitZombieList.zombieList;
            }
            catch
            {
                // 如果直接访问失败，使用反射
                var zombieListField = typeof(InitZombieList).GetField("zombieList",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                if (zombieListField != null)
                {
                    zombieList =
                        zombieListField.GetValue(null) as Il2CppSystem.Collections.Generic.List<
                            Il2CppSystem.Collections.Generic.List<ZombieSpawnData>>;
                }
            }

            if (zombieList == null)
            {
                ModCore.Instance.Log.LogWarning("GetZombieListData: InitZombieList.zombieList 为 null");
                return null;
            }

            var result = new Dictionary<int, Dictionary<int, int>>();

            // 遍历所有波次（从1开始，跳过索引0）
            for (int waveIndex = 1; waveIndex < zombieList.Count; waveIndex++)
            {
                var wave = zombieList[waveIndex];
                if (wave == null) continue;

                var zombieTypes = new Dictionary<int, int>();
                for (int i = 0; i < wave.Count; i++)
                {
                    var data = wave[i];
                    if (data == null) continue;
                    zombieTypes.Add(i, (int)data.zombieType);
                }

                if (zombieTypes.Count > 0)
                {
                    result[waveIndex] = new Dictionary<int, int>(zombieTypes);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log.LogError($"GetZombieListData 异常: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }

    public static void SetZombieList(int zombieIndex, int waveIndex, ZombieType value)
    {
        try
        {
            // 直接访问 InitZombieList.zombieList
            Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.List<ZombieSpawnData>>? zombieList =
                null;

            try
            {
                // 尝试直接访问
                zombieList = InitZombieList.zombieList;
            }
            catch
            {
                // 如果直接访问失败，使用反射
                var zombieListField = typeof(InitZombieList).GetField("zombieList",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                if (zombieListField != null)
                {
                    zombieList =
                        zombieListField.GetValue(null) as Il2CppSystem.Collections.Generic.List<
                            Il2CppSystem.Collections.Generic.List<ZombieSpawnData>>;
                }
            }

            if (zombieList == null)
            {
                ModCore.Instance.Log?.LogWarning("SetZombieList: InitZombieList.zombieList 为 null");
                return;
            }

            // 检查波次索引是否有效
            if (waveIndex < 0 || waveIndex >= zombieList.Count)
            {
                ModCore.Instance.Log?.LogWarning($"SetZombieList: 波次索引 {waveIndex} 超出范围 (0-{zombieList.Count - 1})");
                return;
            }

            var wave = zombieList[waveIndex];
            if (wave == null)
            {
                ModCore.Instance.Log?.LogWarning($"SetZombieList: 第 {waveIndex} 波为 null");
                return;
            }

            // 检查僵尸索引是否有效
            if (zombieIndex < 0 || zombieIndex >= wave.Count)
            {
                ModCore.Instance.Log?.LogWarning($"SetZombieList: 僵尸索引 {zombieIndex} 超出范围 (0-{wave.Count - 1})");
                return;
            }

            // 直接修改列表
            var data = wave[zombieIndex];
            data?.zombieType = value;
        }
        catch (Exception ex)
        {
            ModCore.Instance.Log?.LogError($"SetZombieList 异常: {ex.Message}\n{ex.StackTrace}");
        }
    }


    public static void TryCaptureSnapshot()
    {
        try
        {
            if (Board.Instance == null) return;
            // 僵尸（高数压缩）
            // 注意：魅惑僵尸在部分版本下不一定稳定出现在 Board.Instance.zombieArray，
            // 这里统一从场景对象抓取，确保快照可记录并恢复魅惑状态。
            List<string> zombieDataList = [];
            var zombiesNow = Object.FindObjectsOfTypeAll(Il2CppType.Of<Zombie>());
            foreach (var t in zombiesNow)
            {
                var zombie = t?.Cast<Zombie>();
                if (zombie is not null && zombie.gameObject is not null && zombie.gameObject.activeInHierarchy)
                {
                    int isMindControlled = zombie.isMindControlled ? 1 : 0;
                    var zombieData =
                        $"{zombie.theZombieRow},{zombie.gameObject.transform.position.x},{(int)zombie.theZombieType},{isMindControlled}";
                    zombieDataList.Add(zombieData);
                }
            }

            var zombieCode = string.Join(";", zombieDataList);
            var zombieString = CompressString(zombieCode);
            // 植物（高数压缩）
            List<string> lineupData = [];
            var allPlants = Lawnf.GetAllPlants();
            if (allPlants != null)
            {
                foreach (var plant in allPlants)
                {
                    if (plant == null) continue;
                    var plantData = $"{plant.thePlantColumn},{plant.thePlantRow},{(int)plant.thePlantType}";
                    lineupData.Add(plantData);
                }
            }

            var plantCode = string.Join(";", lineupData);
            var plantString = CompressString(plantCode);
            var snap = new Snapshot
            {
                MixCodeCompressed = plantString + "|" + zombieString,
                Sun = Board.Instance.theSun,
                Money = Board.Instance.theMoney,
                Wave = Board.Instance.theWave,
                MaxWave = Board.Instance.theMaxWave,
                IsHugeWave = SafeGetIsHugeWave(),
                BoardType = (int)GameAPP.theBoardType,
                SurvivalRound = SafeGetSurvivalRound(),
                LevelNumber = SafeGetLevelNumber(),
                TimeUntilNextWave = SafeGetTimeUntilNextWave(),
                WaveInterval = SafeGetWaveInterval(),
                CapturedAt = DateTime.UtcNow
            };
            // BoardTag
            try
            {
                var t = Board.Instance.boardTag;
                snap.BoardTag = new BoardTagSnapshot
                {
                    IsSeedRain = t.isSeedRain,
                    IsColumn = t.isColumn,
                    IsScaredyDream = t.isScaredyDream
                };
            }
            catch
            {
            }

            // 词条状态（从 PatchMgr 中读取当前局内状态）
            foreach (var adv in InGameAdvBuffs)
                if (adv.Value > 0)
                    snap.AdvOn.Add((int)adv.Key);
            foreach (var ulti in InGameUltiBuffs)
                if (ulti.Value > 0)
                    snap.UltiOn.Add((int)ulti.Key);
            foreach (var debuff in InGameDebuffs)
                if (debuff.Value)
                    snap.DebuffOn.Add((int)debuff.Key);
            foreach (var invest in InGameInvestBuffs)
                if (invest.Value)
                    snap.InvestOn.Add((int)invest.Key);
            // Vases
            try
            {
                foreach (var gi in Board.Instance.griditemArray)
                {
                    if (gi == null) continue;
                    // 记录所有格子物件（含罐子）
                    snap.GridItems.Add(new GridItemInfo
                    {
                        Row = gi.theItemRow,
                        Col = gi.theItemColumn,
                        ItemType = (int)gi.theItemType,
                        ExtraA = 0,
                        ExtraB = 0
                    });
                    if (gi.theItemType is (GridItemType)4 or (GridItemType)5 or (GridItemType)6)
                    {
                        var pot = gi.Cast<ScaryPot>();
                        snap.Vases.Add(new VaseInfo
                        {
                            Row = gi.theItemRow,
                            Col = gi.theItemColumn,
                            PlantType = (int)pot.thePlantType,
                            ZombieType = (int)pot.theZombieType
                        });
                    }
                }
            }
            catch
            {
            }

            // 小推车回溯已移除（不再捕获）
            // 卡片冷却（CardUI）
            try
            {
                var cards = Object.FindObjectsOfTypeAll(Il2CppType.Of<CardUI>());
                for (int i = 0; i < cards.Count; i++)
                {
                    var c = cards[i]?.Cast<CardUI>();
                    if (c == null) continue;
                    snap.CardCDs.Add(c.CD);
                    snap.CardFullCDs.Add(c.fullCD);
                }
            }
            catch
            {
            }

            // 物品冷却（DroppedCard）
            try
            {
                var drops = Object.FindObjectsOfTypeAll(Il2CppType.Of<DroppedCard>());
                for (int i = 0; i < drops.Count; i++)
                {
                    var d = drops[i]?.Cast<DroppedCard>();
                    if (d == null) continue;
                    snap.DroppedCDs.Add(d.CD);
                    snap.DroppedFullCDs.Add(d.fullCD);
                }
            }
            catch
            {
            }

            // 卡槽（卡库顺序与CD、消耗）
            try
            {
                if (InGameUI.Instance != null && InGameUI.Instance.cards != null)
                {
                    foreach (var card in InGameUI.Instance.cards)
                    {
                        if (card == null) continue;
                        snap.CardBank.Add(new CardSnapshot
                        {
                            PlantType = (int)card.thePlantType,
                            CD = card.CD,
                            FullCD = card.fullCD,
                            SeedCost = card.theSeedCost
                        });
                    }
                }
            }
            catch
            {
            }

            // 植物/僵尸生命
            try
            {
                var plantsNow = Lawnf.GetAllPlants();
                if (plantsNow != null)
                {
                    foreach (var pl in plantsNow)
                    {
                        if (pl == null) continue;
                        snap.PlantHealths.Add(new PlantHealthInfo
                        {
                            Row = pl.thePlantRow,
                            Col = pl.thePlantColumn,
                            PlantType = (int)pl.thePlantType,
                            Health = pl.thePlantHealth
                        });
                    }
                }
            }
            catch
            {
            }

            try
            {
                foreach (var z in Board.Instance.zombieArray)
                {
                    if (z == null) continue;
                    snap.ZombieHealths.Add(new ZombieHealthInfo
                    {
                        Row = z.theZombieRow,
                        ZombieType = (int)z.theZombieType,
                        Health = z.theHealth
                    });
                }
            }
            catch
            {
            }

            // 随机种子（用于恢复后保持一致性）
            try
            {
                snap.RandomSeed = (int)(DateTime.UtcNow.Ticks & 0x7FFFFFFF);
            }
            catch
            {
                snap.RandomSeed = 0;
            }

            Snapshots.Add(snap);
            if (Snapshots.Count > 600)
                Snapshots.RemoveRange(0, Snapshots.Count - 600);
            // 写入磁盘 LatestSnapshot.json
            var json = JsonSerializer.Serialize(
                snap,
                new JsonSerializerOptions
                {
                    IncludeFields = true,
                    WriteIndented = false
                });
            File.WriteAllText(Paths.LatestSnapshotPath, json);
        }
        catch
        {
        }
    }

    public static Snapshot? TryLoadLatestSnapshotFromDisk()
    {
        try
        {
            if (!File.Exists(Paths.LatestSnapshotPath))
                return null;

            var json = File.ReadAllText(Paths.LatestSnapshotPath);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            var snap = JsonSerializer.Deserialize<Snapshot>(
                json,
                new JsonSerializerOptions
                {
                    IncludeFields = true
                });

            return snap;
        }
        catch
        {
            return null;
        }
    }

    private static int SafeGetSurvivalRound()
    {
        try
        {
            if (Board.Instance != null)
                return Board.Instance.theCurrentSurvivalRound;
        }
        catch
        {
        }

        return 0;
    }

    private static int SafeGetLevelNumber()
    {
        try
        {
            GameLevel.LevelData levelData;
            if (GameLevel.LevelManager.TryGetLevelData(out levelData) && levelData != null)
            {
                return levelData.LevelNumber;
            }
        }
        catch
        {
        }

        return 0;
    }

    private static float SafeGetTimeUntilNextWave()
    {
        try
        {
            if (Board.Instance != null)
                return Board.Instance.timeUntilNextWave;
        }
        catch
        {
        }

        return 0f;
    }

    public static bool IsBoardReadyForSnapshot()
    {
        try
        {
            if (!InGame || Board.Instance == null) return false;
            // 基础对象与数组可用
            if (Board.Instance.zombieArray == null || Board.Instance.griditemArray == null) return false;
            // 波数与配置有效（避免 0/默认态）
            if (Board.Instance.theMaxWave <= 0) return false;
            // UI 准备就绪
            if (InGameUI.Instance == null) return false;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool SafeGetIsHugeWave()
    {
        try
        {
            if (Board.Instance != null)
                return Board.Instance.isHugeWave;
        }
        catch
        {
        }

        return false;
    }

    private static float SafeGetWaveInterval()
    {
        try
        {
            if (Board.Instance != null && Board.Instance.config != null)
                return Board.Instance.config.waveInterval;
        }
        catch
        {
        }

        return 0f;
    }

    public static void RestoreSnapshot(Snapshot snap)
    {
        if (Board.Instance == null) return;
        try
        {
            // 词条还原（优先）
            TryApplyBuffsFromSnapshot(snap);
            // 清场
            var allPlants = Lawnf.GetAllPlants();
            if (allPlants != null)
            {
                for (var i = allPlants.Count - 1; i >= 0; i--)
                    try
                    {
                        allPlants[i]?.Die();
                    }
                    catch
                    {
                    }
            }

            Il2CppReferenceArray<Object> zombies = Object.FindObjectsOfTypeAll(Il2CppType.Of<Zombie>());
            for (var i = zombies.Count - 1; i >= 0; i--)
                try
                {
                    ((Zombie)zombies[i])?.Die();
                }
                catch
                {
                }

            Board.Instance.zombieArray!.Clear();
            // 清空原有罐子
            try
            {
                for (var i = Board.Instance.griditemArray.Count - 1; i >= 0; i--)
                {
                    var gi = Board.Instance.griditemArray[i];
                    if (gi == null) continue;
                    gi.gameObject.active = false;
                    Object.Destroy(gi);
                }

                Board.Instance.griditemArray.Clear();
            }
            catch
            {
            }

            // 还原BoardTag
            try
            {
                var t = Board.Instance.boardTag;
                t.isSeedRain = snap.BoardTag.IsSeedRain;
                t.isColumn = snap.BoardTag.IsColumn;
                t.isScaredyDream = snap.BoardTag.IsScaredyDream;
                Board.Instance.boardTag = t;
            }
            catch
            {
            }

            // 回写混合阵容
            var codes = snap.MixCodeCompressed.Split('|');
            var plantCode = DecompressString(codes[0]);
            var plantEntries = plantCode.Split(';');
            foreach (var entry in plantEntries)
            {
                var plantData = entry.Split(',');
                if (plantData.Length == 3)
                    if (int.TryParse(plantData[0], out var column) &&
                        int.TryParse(plantData[1], out var row) &&
                        int.TryParse(plantData[2], out var plantType))
                        CreatePlant.Instance.SetPlant(column, row, (PlantType)plantType);
            }

            var zombieCode2 = DecompressString(codes[1]);
            var zombieEntries = zombieCode2.Split(';');
            foreach (var entry in zombieEntries)
            {
                var zombieData = entry.Split(',');
                if (zombieData.Length >= 3)
                    if (int.TryParse(zombieData[0], out var row) &&
                        float.TryParse(zombieData[1], out var x) &&
                        int.TryParse(zombieData[2], out var zombieType))
                    {
                        bool isMindControlled = zombieData.Length >= 4 &&
                                                int.TryParse(zombieData[3], out var mindFlag) &&
                                                mindFlag == 1;
                        if (isMindControlled)
                            CreateZombie.Instance.SetZombieWithMindControl(row, (ZombieType)zombieType, x);
                        else
                            CreateZombie.Instance.SetZombie(row, (ZombieType)zombieType, x);
                    }
            }

            // 恢复单位生命（基于行列/类型匹配，尽力还原）
            try
            {
                if (snap.PlantHealths.Count > 0)
                {
                    var plantsRestored = Lawnf.GetAllPlants();
                    if (plantsRestored != null)
                    {
                        foreach (var ph in snap.PlantHealths)
                        {
                            foreach (var pl in plantsRestored)
                            {
                                if (pl == null) continue;
                                if (pl.thePlantRow == ph.Row && pl.thePlantColumn == ph.Col &&
                                    (int)pl.thePlantType == ph.PlantType)
                                {
                                    try
                                    {
                                        pl.thePlantHealth = Math.Min(ph.Health, pl.thePlantMaxHealth);
                                        pl.UpdateText();
                                    }
                                    catch
                                    {
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            try
            {
                if (snap.ZombieHealths is { Count: > 0 })
                {
                    foreach (var zh in snap.ZombieHealths)
                    {
                        foreach (var z in Board.Instance.zombieArray)
                        {
                            if (z == null) continue;
                            if (z.theZombieRow == zh.Row && (int)z.theZombieType == zh.ZombieType)
                            {
                                try
                                {
                                    z.theHealth = Math.Min(zh.Health, z.theMaxHealth);
                                    z.UpdateHealthText();
                                }
                                catch
                                {
                                }

                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            // 回写格子物件（除去罐子特殊字段）
            try
            {
                foreach (var gi in snap.GridItems)
                {
                    var created = GridItem.SetGridItem(gi.Col, gi.Row, (GridItemType)gi.ItemType);
                    // 预留扩展字段（暂不设置）
                }
            }
            catch
            {
            }

            // 回写罐子内容（覆盖上一步生成的罐子，填充植物/僵尸类型）
            try
            {
                foreach (var v in snap.Vases)
                {
                    var g = GridItem.SetGridItem(v.Col, v.Row, GridItemType.ScaryPot);
                    g.Cast<ScaryPot>().thePlantType = (PlantType)v.PlantType;
                    g.Cast<ScaryPot>().theZombieType = (ZombieType)v.ZombieType;
                }
            }
            catch
            {
            }

            // 小推车回溯已移除（不再还原）
            // 恢复卡片与物品冷却（基于发现顺序，尽力匹配）
            try
            {
                // 先恢复卡槽内容与顺序
                if (InGameUI.Instance != null && InGameUI.Instance.cards != null && snap.CardBank != null &&
                    snap.CardBank.Count > 0)
                {
                    int k = Math.Min(InGameUI.Instance.cards.Count, snap.CardBank.Count);
                    for (int i = 0; i < k; i++)
                    {
                        var card = GetInGameCards(InGameUI.Instance)[i];
                        var cs = snap.CardBank[i];
                        try
                        {
                            card.thePlantType = (PlantType)cs.PlantType;
                            card.ChangeCardSprite();
                            card.theSeedCost = cs.SeedCost;
                            card.fullCD = cs.FullCD;
                            card.CD = cs.CD;
                        }
                        catch
                        {
                        }
                    }
                }

                var cards = Object.FindObjectsOfTypeAll(Il2CppType.Of<CardUI>());
                int n = Math.Min(cards.Count, snap.CardCDs?.Count ?? 0);
                for (int i = 0; i < n; i++)
                {
                    var c = cards[i]?.Cast<CardUI>();
                    if (c == null) continue;
                    try
                    {
                        // 先还原 fullCD，再还原 CD
                        if (snap.CardFullCDs != null && i < snap.CardFullCDs.Count) c.fullCD = snap.CardFullCDs[i];
                        c.CD = (snap.CardCDs != null && i < snap.CardCDs.Count) ? snap.CardCDs[i] : c.CD;
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }

            try
            {
                var drops = Object.FindObjectsOfTypeAll(Il2CppType.Of<DroppedCard>());
                var m = Math.Min(drops.Count, snap.DroppedCDs?.Count ?? 0);
                for (var i = 0; i < m; i++)
                {
                    var d = drops[i]?.Cast<DroppedCard>();
                    if (d == null) continue;
                    if (snap.DroppedFullCDs != null && i < snap.DroppedFullCDs.Count) d.fullCD = snap.DroppedFullCDs[i];
                    d.CD = (snap.DroppedCDs != null && i < snap.DroppedCDs.Count) ? snap.DroppedCDs[i] : d.CD;
                }
            }
            catch
            {
            }

            // 恢复随机种子（保证后续过程一致性）
            if (snap.RandomSeed != 0) Random.InitState(snap.RandomSeed);
            Board.Instance.theSun = snap.Sun;
            Board.Instance.theMoney = snap.Money;
            // 恢复波次计时
            try
            {
                if (snap.TimeUntilNextWave >= -10f && snap.TimeUntilNextWave <= 120f)
                {
                    Board.Instance.timeUntilNextWave = snap.TimeUntilNextWave;
                }
            }
            catch
            {
            }

            // 精确还原波次与旗帜状态（避免推进触发带来的偏差）
            try
            {
                if (snap.MaxWave > 0) Board.Instance.theMaxWave = snap.MaxWave;
                if (snap.Wave >= 0) Board.Instance.theWave = snap.Wave;
                try
                {
                    // 3.3.1 仍存在 isHugeWave
                    Board.Instance.isHugeWave = snap.IsHugeWave;
                }
                catch
                {
                }

                // 还原两波间隔，保证刷新节奏一致
                try
                {
                    if (Board.Instance.config != null && snap.WaveInterval > 0f)
                        Board.Instance.config.waveInterval = snap.WaveInterval;
                }
                catch
                {
                }
            }
            catch
            {
            }

            InGameText.Instance?.ShowText("已恢复局内存档", 2.0f);
        }
        catch
        {
        }
    }

    private static void TryApplyBuffsFromSnapshot(Snapshot snap)
    {
        try
        {
            var travelMgr = ResolveTravelMgr();
            if (travelMgr == null) return;
            // 高级词条
            foreach (var id in snap.AdvOn)
            {
                try
                {
                    travelMgr.GetNormalBuff((AdvBuff)id);
                }
                catch
                {
                }
            }

            // 究极词条
            foreach (var id in snap.UltiOn)
            {
                try
                {
                    travelMgr.GetUltiBuff((UltiBuff)id, true);
                }
                catch
                {
                }
            }

            // 负面词条
            foreach (var id in snap.DebuffOn)
            {
                try
                {
                    travelMgr.GetDebuff((TravelDebuff)id);
                }
                catch
                {
                }
            }

            // 投资词条
            foreach (var id in snap.InvestOn)
            {
                try
                {
                    travelMgr.GetInvestBuff((InvestBuff)id);
                }
                catch
                {
                }
            }
        }
        catch
        {
        }
    }

    internal static List<CardUI> GetInGameCards(InGameUI? ui)
    {
        var list = new List<CardUI>();
        if (ui?.cards == null)
            return list;

        foreach (var card in ui.cards)
        {
            if (card != null)
                list.Add(card);
        }

        return list;
    }

    internal static void ClearCursedPlants(Zombie zombie)
    {
        try
        {
            var field = typeof(Zombie).GetField(
                "cursedPlants",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            if (field?.GetValue(zombie) is Il2CppSystem.Collections.Generic.List<Plant> cursedPlants &&
                cursedPlants.Count > 0)
            {
                cursedPlants.Clear();
            }
        }
        catch
        {
            // ignored
        }
    }

    public static void ApplySettings(ShootingManager mgr)
    {
        if (mgr == null) return;
        try
        {
            if (GodEvolutionLucky >= 0)
                mgr.Lucky = (int)GodEvolutionLucky;
            if (GodEvolutionDifficulty >= 0)
                mgr.difficulty = GodEvolutionDifficulty;
            if (ShouldFixGodEvolutionRefreshButton)
                mgr.refreshCount = GetGodEvolutionMenuRefreshCount();
            if (GodEvolutionMaxPlantCount >= 0)
                mgr.maxPlantCount = GodEvolutionMaxPlantCount;
            if (GodEvolutionOptionCount >= 0)
                mgr.optionCount = GodEvolutionOptionCount;
            if (GodEvolutionUpgradeBuffChance >= 0 || GodEvolutionFreeUpgradeQuality)
                mgr.upgradeBuffChance = GodEvolutionFreeUpgradeQuality ? 999999 : GodEvolutionUpgradeBuffChance;
            if (GodEvolutionSuperUpgrade)
                mgr.superUpgrade = true;
            if (GodEvolutionForceSuperQuality)
                (_appearSuperQualitativeField ??= typeof(ShootingManager).GetField("appearSuperQualitative",
                    BindingFlags.Instance | BindingFlags.NonPublic))?.SetValue(mgr, true);
            if (GodEvolutionUncrashable)
                (_uncrashableField ??= typeof(ShootingManager).GetField("uncrashable",
                    BindingFlags.Instance | BindingFlags.NonPublic))?.SetValue(mgr, true);
        }
        catch
        {
        }
    }

    public static int GetGodEvolutionMenuRefreshCount()
    {
        if (IsRefreshUnlimited) return 9999999;
        if (GodEvolutionRefreshOverrideActive) return GodEvolutionRefreshCount;
        return 0;
    }

    public static Quality RollQuality()
    {
        float total = GodEvolutionQualityDefault + GodEvolutionQualitySilver + GodEvolutionQualityGold +
                      GodEvolutionQualityDiamond;
        if (total <= 0f) return Quality.Default;
        var r = Random.Range(0f, total);
        if (r < GodEvolutionQualityDefault) return Quality.Default;
        r -= GodEvolutionQualityDefault;
        if (r < GodEvolutionQualitySilver) return Quality.silver;
        r -= GodEvolutionQualitySilver;
        if (r < GodEvolutionQualityGold) return Quality.gold;
        return Quality.diamond;
    }

    public static string GetInvestBuffChineseName(int id)
    {
        return id switch
        {
            0 => "完美开局",
            1 => "气氛组",
            2 => "无伤通关",
            3 => "植物重组",
            4 => "究极支援",
            5 => "恢复生机",
            6 => "简单模式",
            7 => "难度修改器",
            8 => "当头一棒",
            9 => "榜样的力量",
            10 => "基层贡献",
            11 => "绝对力量奖",
            12 => "存款回报",
            13 => "免费刷新",
            1000 => "现金为王",
            1001 => "降本增效",
            1002 => "精准暴击",
            1003 => "百花齐放",
            1004 => "榜样的力量II",
            1005 => "风暴骑士",
            1006 => "绕口令",
            1007 => "创伤小组",
            1008 => "打通上下游",
            1009 => "人海战术",
            1010 => "固定理财",
            1011 => "星变",
            1012 => "沙里淘金",
            1013 => "延迟收益",
            2000 => "幸运闪避",
            2001 => "攻防一体",
            2002 => "野蛮成长",
            2003 => "鲜血阶梯",
            2004 => "超光速提拔",
            2005 => "幸运之子",
            2006 => "积分大使飘飘",
            2007 => "开源节流",
            2008 => "概率事件",
            2009 => "被动收入",
            2010 => "星辉模仿卡",
            2011 => "淘宝积分",
            2012 => "养精蓄锐",
            2013 => "藏一手",
            _ => ""
        };
    }

    public static List<GameObject> Items =>
    [
        Resources.Load<GameObject>("Items/Fertilize/Ferilize"),
        Resources.Load<GameObject>("Items/Bucket"),
        Resources.Load<GameObject>("Items/Helmet"),
        Resources.Load<GameObject>("Items/Jackbox"),
        Resources.Load<GameObject>("Items/Pickaxe"),
        Resources.Load<GameObject>("Items/Machine"),
        Resources.Load<GameObject>("Items/SuperMachine"),
        Resources.Load<GameObject>("Items/SproutPotPrize/SproutPotPrize"),
        Resources.Load<GameObject>("Items/PortalHeart")
    ];
}