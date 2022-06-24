using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowDevice : GenericDevice
{
    private Transform gO;

    public override void Activate()
    {
        base.Activate();
        gO.Rotate(new Vector3(0, 0, 90), Space.World);
        EXTERNAL_CONTROL.instance.influenceRateTEMP *= 100;
        EXTERNAL_CONTROL.instance.influenceRateHUM *= 100;
    }

    public override void Deactivate()
    {
        base.Deactivate();
        gO.Rotate(new Vector3(0, 0, -90), Space.World);
        EXTERNAL_CONTROL.instance.ResetInfluence();
    }

    protected override void Start()
    {
        base.Start();
        gO = transform.parent;
    }

    protected override void Update()
    {
        if(Active)
            textMeshProUGUI.color = Color.red;
        else
            textMeshProUGUI.color = Color.black;
    }

    protected override void updateData()
    {
        //No update
    }
}
