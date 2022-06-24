using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDevice : GenericDevice
{
    public override void Activate()
    {
        base.Activate();
        SIMULATOR_CONTROL.instance._lightLevel = 8;
    }

    public override void Deactivate()
    {
        base.Deactivate();
        SIMULATOR_CONTROL.instance._lightLevel = 0;
    }

    protected override void updateData()
    {
        //no updates
    }
}
