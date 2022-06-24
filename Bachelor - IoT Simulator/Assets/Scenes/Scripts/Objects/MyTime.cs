using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Obsolete("Not used any more", true)]
public class MyTime
{
    private int _hour;
    private int _minute;

    public MyTime(int hour, int minute){
        this._hour = hour;
        this._minute = minute;
    }


    public void PassMin(int amountMin){
        if(amountMin < 0){
            if(_minute < amountMin){
                var x = amountMin - _minute;
                _minute = 60 - x;
                _hour = (_hour - 1) < 0 ? 23 : _hour;
            }
        }
        else if(_minute + amountMin >= 60){
            var min = (_minute + amountMin) % 60;
            int hour = (int) Mathf.Floor((_minute + amountMin) / 60.0f);
            hour %= 24;

            _hour = hour;
            _minute = min;
        } else {
            _minute += amountMin;
        }
    }


    
}
