using System;
using System.Collections.Generic;
using PVZRHTools.Utils;
using ReactiveUI;
using ToolData;

namespace PVZRHTools.Models;

[Serializable]
public class ModifierInfo : ReactiveObject
{
    public string GameVersion { get; set; } = Strings.GameVersion;
    public List<string> GamePaths { get; set; } = [];
}