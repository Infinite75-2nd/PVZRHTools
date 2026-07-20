namespace ToolMod.Components;

/// <summary>
/// HotKeys.json 的数据结构，保存所有按键绑定的 KeyCode 整数值。
/// 与原有的 SettingsData.KeyXxx 字段一一对应。
/// </summary>
public class HotKeysData
{
    public int KeySpeedStop { get; set; } = 54;                 // KeyCode.Alpha6
    public int KeyShowGameInfo { get; set; } = 96;              // KeyCode.BackQuote
    public int KeyTopMostCardBank { get; set; } = 9;            // KeyCode.Tab
    public int KeyRandomCard { get; set; } = 114;               // KeyCode.R
    public int KeyAlmanacCreatePlant { get; set; } = 98;        // KeyCode.B
    public int KeyAlmanacCreatePlantVase { get; set; } = 106;   // KeyCode.J
    public int KeyAlmanacCreateZombie { get; set; } = 110;      // KeyCode.N
    public int KeyAlmanacCreateZombieVase { get; set; } = 107;  // KeyCode.K
    public int KeyAlmanacZombieMindCtrl { get; set; } = 306;    // KeyCode.LeftControl
}
