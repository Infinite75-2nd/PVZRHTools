using System.Collections.Generic;

namespace ToolData;

public class InitData
{
    public Dictionary<int, string> Plants { get; set; } = [];
    public Dictionary<int, string> Zombies { get; set; } = [];
    public Dictionary<int, string> FirstArmors { get; set; } = [];
    public Dictionary<int, string> SecondArmors { get; set; } = [];
    public Dictionary<int, string> Bullets { get; set; } = [];
    public Dictionary<int, string> AdvBuffs { get; set; } = [];
    public Dictionary<int, string> UltiBuffs { get; set; } = [];
    public Dictionary<int, string> Debuffs { get; set; } = [];
    public Dictionary<int, string> InvestBuffs { get; set; } = [];
}