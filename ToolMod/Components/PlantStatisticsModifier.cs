using System;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace ToolMod.Components;

public class PlantStatisticsModifier:MonoBehaviour
{
    public PlantStatisticsModifier() : base(ClassInjector.DerivedConstructorPointer<PlantStatisticsModifier>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public PlantStatisticsModifier(IntPtr ptr) : base(ptr)
    {
    }
    
    public static GameObject InputField{get;set;}=Resources.Load<GameObject>("ui\\prefabs\\sample\\InputField");
    public static GameObject Toggle{get;set;}=Resources.Load<GameObject>("ui\\prefabs\\sample\\Toggle");

}