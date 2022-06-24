using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISensor
{
    public string GetName();
    public SensorType GetSensorType();
}

