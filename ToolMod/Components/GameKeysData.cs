namespace ToolMod.Components;

/// <summary>
/// GameKeys.json 的数据结构，保存游戏原版按键绑定的 KeyCode 整数值。
/// 字段名与 KeyCodeManager 的静态属性一一对应。
/// 默认值为 0（KeyCode.None），首次保存时会以当前 KeyCodeManager 的实际值为准整体覆盖。
/// </summary>
public class GameKeysData
{
    public int Shovel { get; set; }
    public int Glove { get; set; }
    public int SlowTrigger { get; set; }
    public int Hammer { get; set; }
    public int Wheel { get; set; }
    public int ShowPlantHealth { get; set; }
    public int ShowZombieHealth { get; set; }
    public int UseGoldBean { get; set; }
    public int CheckPlantAlmanac { get; set; }
    public int ShowBulletDamage { get; set; }
    public int ZombieGlove { get; set; }
    public int LookPlantData { get; set; }
    public int FullScreen { get; set; }
    public int NormalScreen { get; set; }
    public int Ra2Sound { get; set; }
    public int HideUI { get; set; }
}
