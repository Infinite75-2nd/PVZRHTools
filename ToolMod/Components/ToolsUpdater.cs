using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using TMPro;
using ToolMod.Patches;
using Unity.VisualScripting;
using UnityEngine;
using static ToolMod.Components.PatchDataCache;
using static ToolMod.Utils;
using Random = UnityEngine.Random;

namespace ToolMod.Components;

public class ToolsUpdater : MonoBehaviour
{
    public ToolsUpdater() : base(ClassInjector.DerivedConstructorPointer<ToolsUpdater>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public ToolsUpdater(IntPtr ptr) : base(ptr)
    {
    }

    public void Awake()
    {
        Instance = this;
    }

    public static ToolsUpdater Instance { get; set; }

    [HideFromIl2Cpp] public PatchDataCache DataObserver { get; set; } = new();

    public void ProcessGameSpeed()
    {
        if (Input.GetKeyDown(KeySpeedStop))
        {
            TimeStop = !TimeStop;
            TimeSlow = false;
        }

        if (Input.GetKeyDown(KeySpeedSlow))
        {
            TimeStop = false;
            TimeSlow = !TimeSlow;
        }

        if (!TimeStop && !TimeSlow)
        {
            Time.timeScale =
                GameSpeedEnabled ? GameSpeed : (GameAPP.config is not null ? GameAPP.config.gameSpeed : 1f);
        }
        else if (!TimeStop && TimeSlow)
        {
            Time.timeScale = 0.2f;
        }
        else if (TimeStop && !TimeSlow)
        {
            Time.timeScale = 0;
        }

        try
        {
            Transform slow;
            if (InGameUI.Instance is not null && !InGameUI.Instance.IsDestroyed())
            {
                slow = InGameUI.Instance.SlowTrigger.transform;
                slow.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = $"时停(x{Time.timeScale})";
                slow.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = $"时停(x{Time.timeScale})";
            }
            else if (IZBottomMenu.Instance is not null && !IZBottomMenu.Instance.IsDestroyed())
            {
                slow = GameObject.Find("SlowTrigger").transform;
                slow.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = $"时停(x{Time.timeScale})";
                slow.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = $"时停(x{Time.timeScale})";
            }
        }
        catch
        {
        }
    }

    public void ProcessHotkeys()
    {
        try
        {
            if (Input.GetKeyDown(KeyTopMostCardBank))
            {
                var canvas = GameAPP.canvas.GetComponent<Canvas>();
                if (canvas.sortingLayerName == "Default")
                    canvas.sortingLayerName = "UI";
                else
                    canvas.sortingLayerName = "Default";
            }
        }
        catch
        {
        }

        if (Input.GetKeyDown(KeyRandomCard))
            RandomCard = !RandomCard;

        if (Input.GetKeyDown(KeyAlmanacCreatePlant) && AlmanacSeedType is not PlantType.Nothing)
        {
            if (CreatePlant.Instance != null)
                CreatePlant.Instance.SetPlant(Mouse.Instance.theMouseColumn, Mouse.Instance.theMouseRow,
                    AlmanacSeedType);
        }

        // 切换魅惑僵尸模式
        if (Input.GetKeyDown(KeyAlmanacZombieMindCtrl))
            AlmanacZombieMindCtrl = !AlmanacZombieMindCtrl;

        // 放置僵尸
        if (Input.GetKeyDown(KeyAlmanacCreateZombie) &&
            AlmanacZombieType is not ZombieType.Nothing)
        {
            if (CreateZombie.Instance != null)
            {
                if (AlmanacZombieMindCtrl)
                    CreateZombie.Instance.SetZombieWithMindControl(Mouse.Instance.theMouseRow, AlmanacZombieType,
                        Mouse.Instance.mouseX);
                else
                    CreateZombie.Instance.SetZombie(Mouse.Instance.theMouseRow, AlmanacZombieType,
                        Mouse.Instance.mouseX);
            }
        }

        // 植物罐子 - 使用 ScaryPot_plant 类型
        if (Input.GetKeyDown(KeyAlmanacCreatePlantVase) && AlmanacSeedType is not PlantType.Nothing)
        {
            var gridItem = GridItem.SetGridItem(Mouse.Instance.theMouseColumn, Mouse.Instance.theMouseRow,
                GridItemType.ScaryPot_plant);
            if (gridItem != null)
            {
                var scaryPot = gridItem.GetComponent<ScaryPot>();
                if (scaryPot != null)
                {
                    scaryPot.thePlantType = (PlantType)AlmanacSeedType;
                }
            }
        }

        // 僵尸罐子 - 使用 ScaryPot_zombie 类型
        if (Input.GetKeyDown(KeyAlmanacCreateZombieVase) &&
            AlmanacZombieType is not ZombieType.Nothing)
        {
            var gridItem = GridItem.SetGridItem(Mouse.Instance.theMouseColumn, Mouse.Instance.theMouseRow,
                GridItemType.ScaryPot_zombie);
            if (gridItem != null)
            {
                var scaryPot = gridItem.GetComponent<ScaryPot>();
                if (scaryPot != null)
                {
                    scaryPot.theZombieType = AlmanacZombieType;
                }
            }
        }

        // 星辉buff功能 - 点击植物解锁星辉buff模式（如果该植物有星辉buff功能）
        try
        {
            if (StarUpBuff && Board.Instance != null && Mouse.Instance != null)
            {
                // 左键点击植物来应用星辉buff
                if (Input.GetMouseButtonDown(0))
                {
                    int column = Mouse.Instance.theMouseColumn;
                    int row = Mouse.Instance.theMouseRow;

                    // 检查点击位置是否有植物
                    var plants = Lawnf.Get1x1Plants(column, row);
                    if (plants != null && plants.Count > 0)
                    {
                        var plant = plants[0];
                        if (plant != null && !plant.isCrashed && plant.thePlantHealth > 0)
                        {
                            // 使用辅助方法给植物上星辉buff
                            ApplyStarUpBuff(plant);
                        }
                    }
                }
            }
        }
        catch
        {
        }

        // 随机升级模式 - 点击植物操控，R键切换僵尸显血
        try
        {
            if (RandomUpgradeMode && Board.Instance != null && Mouse.Instance != null)
            {
                // 左键点击植物来操控，再次点击同一植物则停止操控
                if (Input.GetMouseButtonDown(0))
                {
                    int column = Mouse.Instance.theMouseColumn;
                    int row = Mouse.Instance.theMouseRow;

                    // 先检查是否点击了当前操控的植物（根据植物当前位置）
                    var controled = Board.Instance.controledPlant;
                    if (controled != null && controled.thePlantColumn == column && controled.thePlantRow == row)
                    {
                        // 点击当前操控的植物，停止操控
                        Board.Instance.controledPlant = null;
                    }
                    else
                    {
                        // 检查点击位置是否有其他植物
                        var plants = Lawnf.Get1x1Plants(column, row);
                        if (plants != null && plants.Count > 0)
                        {
                            var plant = plants[0];
                            if (plant != null)
                            {
                                // 设置为操控植物
                                Board.Instance.controledPlant = plant;
                            }
                        }
                    }
                }

                // 方向键移动操控的植物（使用游戏内置方法）
                if (Board.Instance.controledPlant != null)
                {
                    // 使用游戏内置的 MoveControlPlant 方法
                    // index: 0=上, 1=左, 2=下, 3=右
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        Board.Instance.MoveControlPlant(0);
                    }

                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        Board.Instance.MoveControlPlant(2);
                    }

                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        Board.Instance.MoveControlPlant(1);
                    }

                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        Board.Instance.MoveControlPlant(3);
                    }
                }
            }
        }
        catch
        {
        }
    }

    public void ProcessLockData()
    {
        if (LockSun >= 0) Board.Instance.theSun = LockSun;
        if (LockMoney >= 0) Board.Instance.theMoney = LockMoney;
        if (PauseSpawn) Board.Instance!.iceDoomFreezeTime = 1;
        //if (CheckLose != null) CheckLose.enabled = !NoFail;
        if(LastNoFail!=NoFail)
            foreach (var gameLose in FindObjectsOfTypeAll(Il2CppType.Of<GameLose>()))
            {
                if (gameLose==null)
                {
                    continue;
                }
                gameLose.GetComponent<BoxCollider2D>().enabled = !NoFail;
            }
        LastNoFail=NoFail;
    }

    public void ProcessZombieSea()
    {
        if (ZombieSeaData.ZombieSeaEnabled &&
            ++SeaTime >= ZombieSeaData.ZombieSeaCD &&
            Board.Instance!.theWave is not 0 && Board.Instance!.theWave < Board.Instance!.theMaxWave &&
            GameAPP.theGameStatus is GameStatus.InGame)
        {
            foreach (var j in ZombieSeaData.ZombieSeaTypes)
            {
                if (j < 0) continue;
                for (var i = 0; i < Board.Instance!.rowNum; i++)
                    CreateZombie.Instance!.SetZombie(i, (ZombieType)j, 11f);
            }

            SeaTime = 0;
        }
    }

    public void ProcessInGameActions()
    {
        // 解锁融合植物
        try
        {
            if (Board.Instance != null)
            {
                var t = Board.Instance.boardTag;
                t.enableTravelPlant = (OriginalBoardTag?.enableTravelPlant ?? false) || RemoveFusionLimit;
                t.enableAllTravelPlant = (OriginalBoardTag?.enableAllTravelPlant ?? false) || RemoveFusionLimit;
                t.isColumn = (OriginalBoardTag?.isColumn ?? false) || ColumnPlanting;
                t.isSeedRain = (OriginalBoardTag?.isSeedRain ?? false) || SeedRain;
                Board.Instance.boardTag = t;
            }
        }
        catch
        {
        }

        if (GarlicDay && ++GarlicDayTime >= 500 && GameAPP.theGameStatus is GameStatus.InGame)
        {
            GarlicDayTime = 0;
            foreach (var zombie in Board.Instance!.zombieArray)
            {
                if (zombie != null)
                {
                    var coroutine = zombie.DeLayGarliced(0.1f, false, false);
                    if (coroutine != null) zombie.StartCoroutine_Auto(coroutine);
                }
            }

            ;
        }

        // 免疫强制扣血 - 通过缓存植物血量并在异常扣血时恢复来实现
        if (ImmuneForceDeduct)
        {
            try
            {
                var allPlants = Lawnf.GetAllPlants();
                if (allPlants != null)
                {
                    // 收集当前存活植物的ID
                    var alivePlantIds = new HashSet<int>();
                    foreach (var p in allPlants)
                    {
                        if (p != null)
                            alivePlantIds.Add(p.GetInstanceID());
                    }

                    // 清理已死亡植物的缓存
                    var deadPlantIds = PlantHealthCache.Keys.Where(id => !alivePlantIds.Contains(id)).ToList();
                    foreach (var id in deadPlantIds)
                        PlantHealthCache.Remove(id);

                    foreach (var plant in allPlants)
                    {
                        if (plant == null) continue;
                        var plantId = plant.GetInstanceID();

                        if (PlantHealthCache.TryGetValue(plantId, out var cachedHealth))
                        {
                            // 检测异常扣血：血量突然大幅下降
                            // 如果血量从正常值突然变成0或负数，或者扣血量超过5000（正常伤害很少这么高）
                            var healthDrop = cachedHealth - plant.thePlantHealth;
                            if (healthDrop > 0 && (plant.thePlantHealth <= 0 || healthDrop > 5000))
                            {
                                // 恢复血量（可能是强制扣血）
                                plant.thePlantHealth = cachedHealth;
                                plant.UpdateText();
                            }
                        }

                        // 只有当植物血量大于0时才更新缓存
                        if (plant.thePlantHealth > 0)
                        {
                            PlantHealthCache[plantId] = plant.thePlantHealth;
                        }
                    }
                }

                // 同时更新Die补丁的缓存
                PlantPatch.UpdateHealthCache();
            }
            catch
            {
            }
        }
        else
        {
            // 功能关闭时清空缓存
            if (PlantHealthCache.Count > 0)
                PlantHealthCache.Clear();
        }

        if (SuperStarNoCD)
        {
            if (Board.Instance!.bigStarActiveCountDown > 0.5f)
            {
                Board.Instance.bigStarActiveCountDown = 0.5f;
            }
        }

        // 土豆雷无CD - 使用 FindObjectsOfType 替代 Harmony patch 避免栈溢出
        if (MineNoCD)
        {
            try
            {
                var mines = FindObjectsOfType<PotatoMine>();
                foreach (var mine in mines)
                {
                    if (mine != null && mine.attributeCountdown > 0.05f)
                        mine.attributeCountdown = 0.05f;
                }
            }
            catch
            {
            }
        }

        // 大嘴花无CD - 使用 FindObjectsOfType 替代 Harmony patch 避免栈溢出
        if (ChomperNoCD)
        {
            try
            {
                var chompers = FindObjectsOfType<Chomper>();
                foreach (var chomper in chompers)
                {
                    if (chomper != null && chomper.attributeCountdown > 0.05f)
                        chomper.attributeCountdown = 0.05f;
                }
            }
            catch
            {
            }
        }

        // 植物升级功能 - 右键点击场上植物升级

        if (PlantUpgrade && Board.Instance != null && Mouse.Instance != null)
        {
            try
            {
                // 检测鼠标右键点击
                if (Input.GetMouseButtonDown(1))
                {
                    // 获取鼠标所在格子的植物
                    int column = Mouse.Instance.theMouseColumn;
                    int row = Mouse.Instance.theMouseRow;

                    // 使用 Lawnf.Get1x1Plants 获取该格子的所有植物
                    var plants = Lawnf.Get1x1Plants(column, row);
                    if (plants != null && plants.Count > 0)
                    {
                        // 遍历该格子的植物，找到可以升级的植物
                        foreach (var plant in plants)
                        {
                            if (plant != null && plant.theLevel < 3)
                            {
                                // 升级植物
                                plant.Upgrade(plant.theLevel + 1, true, false);
                                break; // 只升级一个植物
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        if (RandomCard)
        {
            Il2CppSystem.Collections.Generic.List<PlantType> randomPlant = GameAPP.resourcesManager.allPlants;
            if (InGameUI.Instance && randomPlant != null && randomPlant.Count != 0)
            {
                for (int i = 0; i < InGameUI.Instance.cards.Count; i++)
                {
                    try
                    {
                        var index = Random.RandomRangeInt(0, randomPlant.Count);
                        var card = GetInGameCards(InGameUI.Instance)[i];
                        card.thePlantType = randomPlant[index];
                        card.ChangeCardSprite();
                        card.theSeedCost = 0;
                        card.fullCD = 0;
                    }
                    catch
                    {
                    }
                }
            }
        }
    }

    public void ProcessSnapshot()
    {
        if (PendingManualSnapshotFrames > 0)
        {
            PendingManualSnapshotFrames--;
            if (IsBoardReadyForSnapshot())
            {
                TryCaptureSnapshot();
                PendingManualSnapshotFrames = 0;
                try
                {
                    InGameText.Instance?.ShowText("已保存快照", 2.0f);
                }
                catch
                {
                }
            }
            else if (PendingManualSnapshotFrames == 0)
            {
                try
                {
                    InGameText.Instance?.ShowText("当前关卡尚未初始化完成，快照失败", 2.0f);
                }
                catch
                {
                }
            }
        }
    }

    public void Update()
    {
        if (!InGame) return;
        if (GameAPP.theGameStatus is GameStatus.InGame)
        {
            ProcessGameSpeed();
            ProcessHotkeys();
            ProcessInGameActions();
            ProcessZombieSea();
        }

        if (GameAPP.theGameStatus is GameStatus.InGame or GameStatus.OpenOptions)
        {
            if (Input.GetKeyDown(KeyShowGameInfo)) ShowGameInfo = !ShowGameInfo;
        }

        ProcessLockData();
    }
}