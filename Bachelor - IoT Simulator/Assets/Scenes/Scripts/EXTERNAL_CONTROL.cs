using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using static WeatherAPI;
using UnityEngine.Assertions;


public class EXTERNAL_CONTROL : MonoBehaviour
{
    public int updateRatePerSecond;
    public float influenceRateTEMP;
    public float influenceRateHUM;

    private float _infTemp;
    private float _infHum;

    public TextMeshProUGUI outsideDataText, whatDayText, realDateText;
    public enum Day{
        Monday = 0,
        Tuesday = 1,
        Wednesday = 2,
        Thursday = 3,
        Friday = 4,
        Saturday = 5,
        Sunday = 6

    }
    private float[] temperatur;
    private int[] lightLevel;
    private float[] humidity;
    private Timer timer;
    private int indexHour;
    private int indexDay;
    private int indexData;
    private int currentHour, currentMin;
    private int currentDay;
    private float outsideTemp;
    private int outsideLight;
    private float outsideHumidity;
    private TimeZoneInfo timezone;
    private SpriteRenderer NightSim;
    private CitySummary[] CS;
    private TimeZoneInfo tz;
    public static EXTERNAL_CONTROL instance;

    void Awake(){
        lightLevel = new int[]{0, 0, 0, 0, 1, 2, 4, 6, 8, 10,	10,	10,	10,	10,	10,	10,	9,	9,	8,	7,	5,	3,	1,	0};

        indexHour = StartScreenFunction.instance.startIndex;
        //currentHour = StartScreenFunction.instance.cf.STARTING_HOUR;
        indexData = 0; //DEPRECATED
        indexDay = 0; //DEPRECATED
        currentDay = 0;
        currentMin = 0;

        //Simulating of darkness (only visual)
        NightSim = GameObject.FindGameObjectWithTag("NightSim").GetComponent<SpriteRenderer>();
        ChangeSceneLight(outsideLight);
               
        //timer
        timer = new Timer(updateRatePerSecond);

        //influece reseters
        _infHum = influenceRateHUM;
        _infTemp = influenceRateTEMP;

        instance = this;
        CS = StartScreenFunction.instance.cs; //loaded data
        indexData = StartScreenFunction.instance.startIndex;
        tz = WeatherAPI.GetTimezone(CS[0].cityName);
        Assert.AreEqual(indexHour, indexData);

        outsideTemp = CS[currentDay].cityData[indexHour].temp;
        outsideHumidity = CS[currentDay].cityData[indexHour].humidity;
        outsideLight = lightLevel[indexHour];

        //API CALL FOR WEATHER (CURRENTLY ONLY UPTO 5 DAYS IN PAST)
        /*
        const int data = 5;
        WD = new WeatherData[data];
        for(int i = 0; i < data; i++){
            WD[i] = WeatherAPI.GetResponse(DateTimeOffset.Now.AddDays(-data+i), city:this.city);
        }
        */


        //Initialize API
        /*
        outsideTemp = WD[indexDay].hourly[indexHour].temp;
        outsideLight = lightLevel[indexHour];
        outsideHumidity = WD[indexDay].hourly[indexHour].humidity;
        */
        Debug.Log("Loaded EXTERNAL_CONTROL");
    }

    // Update is called once per frame
    void Update()
    {
        if(timer.UpdateTimer()){
            //difference
            var diff = outsideTemp - SIMULATOR_CONTROL.instance._temperatur;
            
            //evaluate how much increase
            var raiseBy = diff * influenceRateTEMP;
            SIMULATOR_CONTROL.instance.changeTempBy(raiseBy);

            diff = outsideHumidity - SIMULATOR_CONTROL.instance._humidity;
            raiseBy = diff * influenceRateHUM;
            SIMULATOR_CONTROL.instance.changeHumBy(raiseBy);

            //light level
            var before = SIMULATOR_CONTROL.instance._lightLevel;
            if(Controller.instance.deviceList[4].Active){
                if(before < outsideLight)
                    SIMULATOR_CONTROL.instance._lightLevel = outsideLight;
            } 
            else{
                SIMULATOR_CONTROL.instance._lightLevel = outsideLight;
            }

            

            timer.Reset();
            //Debug.Log("Changed Temp by: " + raiseBy + " with a Diff of: " + diff);
        }

        if(TIME_CONTROL.instance.getDays()-1 != currentDay){
            indexDay = indexDay == 24 ? 0 : indexDay+1; 

            currentDay = TIME_CONTROL.instance.getDays()-1;
        }

        //Change outside temp+hum regarding json-obj
        if(TIME_CONTROL.instance.getHour() != currentHour){
            indexHour = indexHour == 23 ? 0 : indexHour+1;
            indexData++;
            outsideTemp = CS[currentDay].cityData[indexHour].temp;
            outsideHumidity = CS[currentDay].cityData[indexHour].humidity;

            

            //outsideTemp = WD[indexDay].hourly[indexHour].temp;
            //outsideHumidity = WD[indexDay].hourly[indexHour].humidity;


            //outsideTemp = temperatur[indexHour];
            outsideLight = lightLevel[indexHour];
            //outsideHumidity = humidity[indexHour];

            currentHour = TIME_CONTROL.instance.getHour();
            ChangeSceneLight(outsideLight);

            //Log data for temp
            Controller.instance.AddToTestLogJSON(outsideTemp,outsideHumidity, SIMULATOR_CONTROL.instance._temperatur, SIMULATOR_CONTROL.instance._humidity);
        }

        //Interpolation of temp & hum
        if(TIME_CONTROL.instance.getMinute() != currentMin){
            currentMin = currentMin == 59 ? 0 : currentMin+1;
            if(currentMin != 0){
                var nextHour = indexHour == 23 ? 0 : indexHour+1;
                var nextDay = nextHour == 0 ? currentDay+1 : currentDay;
                var nextTemp = CS[nextDay].cityData[nextHour].temp;
                var nextHum = CS[nextDay].cityData[nextHour].humidity;

                float percentage = (float) currentMin / 60.0f;
                float differenceTemp = -CS[currentDay].cityData[indexHour].temp + nextTemp; //10.0 °C -> 11.0°C : 10 - 11 = -1
                float differenceHum = -CS[currentDay].cityData[indexHour].humidity + nextHum;

                float addToTemp = 0.0f;
                float addToHum = 0.0f;

                addToTemp = differenceTemp * percentage; 
                addToHum = differenceHum * percentage;
                

                outsideTemp = CS[currentDay].cityData[indexHour].temp + addToTemp;
                outsideHumidity = CS[currentDay].cityData[indexHour].humidity + addToHum;
            }
        }

        //outsideDataText.text = string.Format("{0}°C | {1:0}% | Helligkeit: {2}", outsideTemp, outsideHumidity, outsideLight);
        outsideDataText.text = string.Format("{0:0.0}%\n-\n{1}\n{2:0.0}°C\n-\n-", outsideHumidity, outsideLight, outsideTemp);
        whatDayText.text = string.Format("{0}", (Day) ((TIME_CONTROL.instance.getDays() - 1) % 7));

        var utcCheck = DateTimeOffset.FromUnixTimeSeconds(CS[currentDay].cityData[indexHour].dt);
        var realtimeCheck = TimeZoneInfo.ConvertTimeFromUtc(utcCheck.UtcDateTime, tz);
        realDateText.text = realtimeCheck.ToString();
    }

    private void ChangeSceneLight(int level){
        float newAlpha = 0;

        if(level <= 2)
            newAlpha = 225;
        else if(level == 10)
            newAlpha = 0;
        else {
            newAlpha = 255.0f/(level+1);
        }

        NightSim.color = new Color(100/255, 100/255, 100/255, newAlpha/255);
    }

    public void ResetInfluence(){
        influenceRateTEMP = _infTemp;
        influenceRateHUM = _infHum;
    }

    /*
    public WeatherData CurrentWeatherData(){
        return WD[indexDay];
    }
    */

    public CityData CurrentWeatherData(){
        return CS[currentDay].cityData[indexHour];
    }
}
