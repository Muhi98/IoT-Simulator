using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DoorDevice : GenericDevice
{
    private Transform gO;

    public override void Activate(){
        Active = true;
        gO.Rotate(new Vector3(0, 0, 90), Space.Self);
        SIMULATOR_CONTROL.instance.noiseCreated(Random.Range(1, 20), 1);
    }

    public override void Deactivate(){
        Active = false;
        gO.Rotate(new Vector3(0, 0, -90), Space.Self);
        SIMULATOR_CONTROL.instance.noiseCreated(Random.Range(30, 75), 1);
    }
    // Start is called before the first frame update
    override protected void Start()
    {
        //textMeshProUGUI = SIMULATOR_CONTROL.instance.CreateOverheadText(this.transform, dp.name);
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

    protected override void updateData(){
        //no updating
    }

}
