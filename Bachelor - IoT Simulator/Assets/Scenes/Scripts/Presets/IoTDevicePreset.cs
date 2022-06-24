using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IoTDevicePreset", menuName = "New IoT-Device Preset")]
public class IoTDevice : ScriptableObject
{
    public int intervallForSendRecvData;
    
    public GameObject prefab;

}
