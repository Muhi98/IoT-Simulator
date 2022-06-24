using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ControllerPreset", menuName = "New Controller Preset")]
public class ControllerPreset : ScriptableObject
{
    public int intervallForSendRecvData;
    
    public GameObject prefab;
}
