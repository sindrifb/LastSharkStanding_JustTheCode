using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/HookConfig")]
public class HookConfig : ScriptableObject
{
    public float HookSpeed = 10f;
    [Tooltip("Seconds until full charge")]
    public float ChargeTime = 1f;
    public float MaxDistance = 5f;
    public float MinDistance = 1f;
}
