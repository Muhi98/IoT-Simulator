using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StereoDevice : GenericDevice
{
    public enum Mode
    {
        HIGH,
        MEDIUM,
        LOW
    }

    public Mode currentMode;

    protected override void updateData(){
        int db = 0;
        switch(currentMode){
            case Mode.LOW:
                db = Random.Range(10, 30);
                break;
            case Mode.MEDIUM:
                db = Random.Range(40,60);
                break;
            case Mode.HIGH:
                db = Random.Range(70, 90);
                break;
        }


        SIMULATOR_CONTROL.instance.noiseCreated(db, dp.intervallForUpdatingInfo+1);
        
        timer.Reset();
    }

    protected override void Start(){
        base.Start();
        currentMode = Mode.LOW;
    }

    public void ChangeVolume(string mode){
        if(mode == "high")
            currentMode = Mode.HIGH;
        else if(mode == "medium")
            currentMode = Mode.MEDIUM;
        else
            currentMode = Mode.LOW;
    }
}
