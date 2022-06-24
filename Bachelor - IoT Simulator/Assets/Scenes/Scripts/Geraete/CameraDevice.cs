using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDevice : GenericDevice
{
    private bool switcher = false;
    public override void Activate()
    {
        if(Active)
            return;
        Controller.instance.AddToLog("CAMERA", "ON");
        
        base.Activate();
    }

    public override void Deactivate()
    {
        if(!Active)
            return;
        Controller.instance.AddToLog("CAMERA", "OFF");
        Debug.Log("Camera Deactivated");
        base.Deactivate();
    }
    protected override void updateData()
    {
        if(SIMULATOR_CONTROL.instance._movement && !switcher){
            Controller.instance.AddToLog("CAMERA", "ANOMALY DETECTED");
            switcher = true;
        } else if(!SIMULATOR_CONTROL.instance._movement && switcher) {
            Controller.instance.AddToLog("CAMERA", "NO ANOMALY DETECTED");
            switcher = false;
        }

        
        timer.Reset();
    }
}
