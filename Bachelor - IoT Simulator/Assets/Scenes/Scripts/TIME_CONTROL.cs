using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TIME_CONTROL : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    private float counter;
    private int minutes;
    private int hours;
    private int days;
    private int timeSpeed;

    private bool stopTime;

    private float delta;

    private float fps;

    public static TIME_CONTROL instance;

    void Awake(){
        instance = this;
        delta = 0;
        counter = 0;
        minutes = 0;
        hours = StartScreenFunction.instance.cf.STARTING_HOUR;
        days = 1;
        timeSpeed = 1;
        stopTime = false;
        Debug.Log("Loaded TIME_CONTROL");
    }

    // Update is called once per frame
    void Update()
    {
        if(stopTime){
            delta = 0;
            return;
        }

        delta =  Time.deltaTime*timeSpeed;
        counter += delta;

        if(counter>= 60.0f){
           counter = 0;
           minutes++;
           if(minutes == 60){
               minutes = 0;
               hours++;
               if(hours == 24){
                   hours = 0;
                   days++;
               }
           }
        }


        
        //timeText.text = string.Format("Time:\t\t{0:00}:{1:00}:{4:00}\nDay:\t\t{2}\nMultiplier:\t{3}",hours, minutes, days, timeSpeed, counter);
    }


    public void ChangeTimeSpeed(int degree){
        timeSpeed = degree;
    }

    public void DownSpeed(){
        switch(timeSpeed){
            case 60:
                timeSpeed = 1;
                break;
            case 600:
                timeSpeed = 60;
                break;
            case 6000:
                timeSpeed = 600;
                break;
            default:
                timeSpeed = 1;
                break;
        }
    }

    public void UpSpeed(){
        switch(timeSpeed){
            case 1:
                timeSpeed = 60;
                break;
            case 60:
                timeSpeed = 600;
                break;
            case 600:
                timeSpeed = 6000;
                break;
            default:
                timeSpeed = 6000;
                break;

        }
    }

    public int[] getTime(){
        return new int[]{minutes, hours, days, (int) counter};
    }

    public int[] getTimeNoDay(){
        return new int[]{hours, minutes, (int) counter};
    }

    public int getHour(){
        return hours;
    }

    public int getMinute(){
        return minutes;
    }

    public float getSeconds(){
        return counter;
    }

    public int getDays(){
        return days;
    }
    public float getDelta(){
        return delta;
    }

    public void StopTime(){
        stopTime = true;
    }
    public void ResumeTime(){
        stopTime = false;
    }
    public bool WaitForTime(int[] time){

        return TIME_CONTROL.instance.getHour() == time[0] && TIME_CONTROL.instance.getMinute() >= time[1] && (int) TIME_CONTROL.instance.getSeconds() >= time[2];
    }

    public int[] PassMin(int amountMin){
        var now = this.getTimeNoDay();
        var _hour = now[0];
        var _minute = now[1] +  amountMin;
        

        if(_minute >= 60){
            _hour++;
            _minute %= 60;
            _hour %= 24;
        }

        now[2] = Random.Range(0,59);

        int[] x = {_hour, _minute, now[2]};
        //Debug.Log(TimeToString(x));
        return new int[]{_hour, _minute, now[2]};
    }

    public int[] PassSec(int amountSec){
        var now = this.getTimeNoDay();
        var _hour = now[0];
        var _minute = now[1];
        var counter = now[2] + amountSec;
        

        if(counter >= 60){
            _minute++;
            counter %= 60;
            _minute %= 60;
        }

        int[] x = {_hour, _minute, counter};
        //Debug.Log(TimeToString(x));
        return new int[]{_hour, _minute, counter};
    }

    public string TimeToString(int[] x){
        return string.Format("{0:00}:{1:00}:{2:00}", x[0], x[1], x[2]);
    }

}
