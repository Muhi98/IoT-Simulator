using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SensorType{
    SmokeDetec,
    HumiditySens,
    TemperaturSens,
    LightSens,
    NoiseSens,
    MovementSens
}

[CreateAssetMenu(fileName = "SensorPreset", menuName = "New Sensor Preset")]
public class SensorPreset : ScriptableObject
{
    public int intervallForSendData;
    public new string name;
    public SensorType sensorType;
    public GameObject prefab;
}
