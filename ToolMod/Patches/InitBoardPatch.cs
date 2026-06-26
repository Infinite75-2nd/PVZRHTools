using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Il2CppInterop.Runtime;
using ToolMod.Components;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;
using static ToolMod.Utils;
using Object = UnityEngine.Object;

namespace ToolMod.Patches;

[HarmonyPatch(typeof(InitBoard))]
public static class InitBoardPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(InitBoard.RightMoveCamera))]
    public static void PreRightMoveCamera(InitBoard __instance)
    {
        __instance.StartCoroutine(PostInitBoard());
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(InitBoard.HideSeedBank))]
    public static void PreHideSeedBank()
    {
        if (UnlimitedCardSlots)
        {
            try
            {
                var toRemove = new List<GameObject>();
                foreach (var card in CopiedCards)
                {
                    if (card != null)
                    {
                        var cardUI = card.GetComponent<CardUI>();
                        // 只清除未被选中的卡片
                        if (cardUI == null || !cardUI.isSelected)
                        {
                            Object.Destroy(card);
                            toRemove.Add(card!);
                        }
                    }
                    else
                    {
                        toRemove.Add(card!);
                    }
                }

                // 从列表中移除已销毁的卡片
                foreach (var card in toRemove)
                {
                    CopiedCards.Remove(card);
                }
            }
            catch
            {
            }
        }
    }

    /// <summary>
    /// 清除未被选中的复制卡片（保留已选择的卡片）
    /// </summary>
    public static void ClearUnselectedCopiedCards()
    {
        try
        {
            var toRemove = new List<GameObject>();
            foreach (var card in CopiedCards)
            {
                if (card != null)
                {
                    var cardUI = card.GetComponent<CardUI>();
                    // 只清除未被选中的卡片
                    if (cardUI == null || !cardUI.isSelected)
                    {
                        Object.Destroy(card);
                        toRemove.Add(card!);
                    }
                }
                else
                {
                    toRemove.Add(card!);
                }
            }

            // 从列表中移除已销毁的卡片
            foreach (var card in toRemove)
            {
                CopiedCards.Remove(card);
            }
        }
        catch
        {
        }
    }

    public static IEnumerator PostInitBoard()
    {
        yield return null;
        // 使用统一的 TravelMgr 获取方法
        var travelMgr = ResolveTravelMgr(autoCreate: true);
        if (travelMgr == null)
        {
            ModCore.Instance.Log?.LogWarning("[PVZRHTools] PostInitBoard: 无法找到 TravelMgr 组件");
            yield break;
        }
        yield return null;
        if (!(GameAPP.theBoardType == (LevelType)3 && Board.Instance.theCurrentSurvivalRound != 1))
        {
            yield return null;
            try
            {
                if (Board.Instance != null && GameAPP.board != null)
                {
                    var board = GameAPP.board.GetComponent<Board>();
                    if (board != null)
                    {
                        var boardTag = board.boardTag;
                        if (boardTag.isTravel)
                        {
                            boardTag.enableTravelBuff = true;
                            Board.Instance.boardTag = boardTag;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ModCore.Instance.Log?.LogError(
                    $"[PVZRHTools] PostInitBoard 设置 BoardTag 失败: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // 重置旗帜波状态检测
        // 注意：同步当前旗帜波状态，避免在 PostInitBoard 创建 TravelMgr 后导致旗帜波检测失效
        LastHugeWaveState = Board.Instance != null && Board.Instance.isHugeWave;
        FlagWaveUnlockIndex = 0;
        LastUnlockWave = -1;
        CurrentFlagWaveIndex = 0;
        if (!(GameAPP.theBoardType is LevelType.Survival && Board.Instance?.theCurrentSurvivalRound != 1))
        {
            yield return null;
            // 设置 BoardTag 标志，使游戏识别并应用词条效果
            // 注意：这里只在关卡本身就是旅行关（isTravel 为 true）时，才开启 enableTravelBuff，
            // 避免把所有普通关卡都强行当成旅行关，从而影响小推车等原版关卡行为
            try
            {
                if (Board.Instance != null && GameAPP.board != null)
                {
                    var board = GameAPP.board.GetComponent<Board>();
                    if (board != null)
                    {
                        var boardTag = board.boardTag;
                        if (boardTag.isTravel)
                        {
                            boardTag.enableTravelBuff = true;
                            Board.Instance.boardTag = boardTag;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ModCore.Instance.Log?.LogError($"PostInitBoard 设置 BoardTag 失败: {ex.Message}\n{ex.StackTrace}");
            }
        }

        yield return null;

        // 仅在“本地缓存尚未初始化”时，从游戏当前状态同步一次初始值；
        // 之后切换场景不再用游戏状态覆盖修改器里已经选好的词条。
        if (NewBoard)
        {
            foreach (var adv in InGameAdvBuffs.Keys)
            {
                try
                {
                    InGameAdvBuffs[adv] = Lawnf.TravelAdvanced(adv) ? 1 : 0;
                }
                catch
                {
                    InGameAdvBuffs[adv] = 0;
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
                    InGameUltiBuffs[ulti] = 0;
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
                    InGameDebuffs[debuff] = false;
                }
            }

            foreach (var invest in InGameInvestBuffs.Keys)
                try
                {
                    InGameInvestBuffs[invest] = Lawnf.TravelInvest(invest);
                    
                }
                catch
                {
                    InGameInvestBuffs[invest] = false;                
                }

            // 解锁植物：默认全部未解锁
            foreach (var plant in InGameUnlockedPlants.Keys)
                try
                {
                    InGameUnlockedPlants[plant] = Lawnf.TravelUnlock(plant);
                }
                catch (Exception e)
                {
                    InGameUnlockedPlants[plant] = false;
                }
        }

        yield return null;
        new Thread(SyncInGameBuffs).Start();
        yield return null;

        if (ZombieSeaData is { ZombieSeaLowEnabled: true, ZombieSeaTypes.Count: > 0 })
        {
            var i = 0;
            try
            {
                foreach (var wave in InitZombieList.zombieList)
                {
                    if (wave is null) continue;
                    wave.Clear();
                    for (var index = 0; index < 100; index++)
                    {
                        wave.Add(new ZombieSpawnData((ZombieType)ZombieSeaData.ZombieSeaTypes[i]));
                        if (++i >= ZombieSeaData.ZombieSeaTypes.Count) i = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                ModCore.Instance.Log?.LogError($"ZombieSeaLow 异常: {ex.Message}\n{ex.StackTrace}");
            }
        }
        yield return null;
        CheckLose = GameObject.Find("checklose")?.GetComponent<BoxCollider2D>();

    }
}