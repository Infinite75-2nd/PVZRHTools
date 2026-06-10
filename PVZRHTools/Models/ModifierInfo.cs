using System;
using System.Collections.Generic;
using PVZRHTools.Utils;
using ReactiveUI;
using ToolData;

namespace PVZRHTools.Models;

[Serializable]
public class ModifierInfo : ReactiveObject
{
    public string GameVersion { get; set; } = "";
    public string ModifierVersion { get; set; } = "";
    public List<string> GamePaths { get; set; } = [];
}