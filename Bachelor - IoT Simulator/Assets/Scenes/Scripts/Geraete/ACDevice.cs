using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ACDevice : GenericDevice
{    
    protected override void updateData(){
        //SIMULATOR_CONTROL.instance._temperatur -= 0.07f;
        //SIMULATOR_CONTROL.instance._humidity -= 0.005f;
        
        timer.Reset();
    }
}
