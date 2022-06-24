using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DevicePreset", menuName = "New Device Preset")]
public class DevicePreset : ScriptableObject
{
    public int intervallForUpdatingInfo;
    public new string name;
    
    public GameObject prefab;
}
