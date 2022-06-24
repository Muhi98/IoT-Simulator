using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDevice
{
    public string GetName();
    public void Activate();
    public void Deactivate();
    public bool GetStatus();
}
