using System;

namespace ToolData;

[Serializable]
public struct BootConfig
{
    public string ModifierPath { get; set; }
    public string GameVersion { get; set; }
    public bool ModifierEnabled { get; set; }
}