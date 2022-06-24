using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    public Timer(int updateInterval){
        this.updateInterval = updateInterval;
    }


    
    private float timer = 0.0f;
    //private float seconds = 0.0f;
    private bool ready = false;


    private int updateInterval;

    public bool UpdateTimer(){
        if(!ready){
            timer += TIME_CONTROL.instance.getDelta();
            //seconds = timer % 60;

            if(timer >= updateInterval){
                //Debug.Log(timer-updateInterval);
                ready = true;
                timer = 0.0f;
                return true;
            }

            return false;
        }


        return true;
    }

    public void Reset(){
        ready = false;
    }

    public void newInterval(int interval){
        this.updateInterval = interval;
    }
}
