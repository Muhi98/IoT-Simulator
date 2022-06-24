using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using UnityEngine.Assertions;
using static WeatherAPI;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public class StartScreenFunction : MonoBehaviour
{
    public Button startButton, loadconfigButton, loadconfigSTButton, loadWeatherButton, logfileSetButton;
    public AnimatedIconHandler config;
    public AnimatedIconHandler weather, logfileIcon;
    public TextMeshProUGUI statsData, weatherStatsData;
    public TextMeshProUGUI statsErrorText;

    public bool config_set;
    public bool weatherdata_set;
    public bool logfilePath_set;

    public CONFIG_FILE cf;
    public CitySummary[] cs;
    public int startIndex;
    public bool logVisible;
    public string logfilePath;

    public static StartScreenFunction instance;

    void Awake(){
        config_set = false;
        weatherdata_set = false;
        logfilePath_set = false;
        startButton.interactable = false;
        loadconfigButton.interactable = true;
        loadconfigSTButton.interactable = true;
        loadWeatherButton.interactable = false;
        logfileSetButton.interactable = true;
        cf = null;
        cs = null;
        logfilePath = "";

        instance = this;
    }

    void Update(){
        if(config_set && weatherdata_set && cf != null && cs != null)
            startButton.interactable = true;

        if(config_set && cf != null){
            statsData.text = String.Format("{0}\n{1:00}:00\n{2}\n{3} Days\n{4:0.00}\n-------\n{5:00}:00\n{6:00}:00\n{7:00}:00\n{8:00}:00\n{9:00}:00\n{10}%\n{11}%\n{12}",
            cf.CITY, cf.STARTING_HOUR, cf.SEED, cf.RUN_SIMULATION_FOR_X_DAYS, cf.HOURS_PER_SECOND, cf.WAKE_UP, cf.WORK_TIME, cf.DONE_WORK, cf.ARRIVE_HOME, cf.SLEEP_TIME,
            cf.CHANCE_MOVING, cf.CHANCE_OPEN_WINDOW, cf.AUTOMATIC);
        } else {
            statsData.text = "";
        }

        if(!config_set){
            statsErrorText.text = "CONFIG NOT SET\nWEATHER NOT SET";
            weatherStatsData.text = "";
        } else if(!weatherdata_set){
            statsErrorText.text = "WEATHER NOT SET";
            weatherStatsData.text = "";
        } else{
            statsErrorText.text = "";
            weatherStatsData.text = String.Format("<b>Available Weather Data</b>\n{0} Days\nTemperature, Humidity per hour", this.cs.Length);
        }
    }
    public void StartSim(){
        //Change Scene
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
    }

    public void SetConfig(){
        config.ClickEvent();
        config_set = true;
        loadconfigButton.interactable = false;
        loadconfigSTButton.interactable = false;
        loadWeatherButton.interactable = true;
    }

    public void SetWeatherData(){
        weather.ClickEvent();
        weatherdata_set = true;
        loadWeatherButton.interactable = false;
    }

    public void SetLogPathData(){
        logfileIcon.ClickEvent();
        logfilePath_set = true;
        logfileSetButton.interactable = false;
    }

    public void ResetAllLoads(){
        if(weatherdata_set)
            weather.ClickEvent();
        if(config_set)
            config.ClickEvent();
        if(logfilePath_set)
            logfileIcon.ClickEvent();
        Awake();
    }

     

    public void ExitApplication(){
        Application.Quit();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void LoadConfigFile(){
        Debug.Log("Loading config-file...");
        StartCoroutine(LoadConfigRoutine());
    }

    IEnumerator LoadConfigRoutine(){
        yield return StartCoroutine(Config_Manager.instance.ShowLoadDialogCoroutine("config"));
        cf = Config_Manager.instance.config;
        Debug.Log("Success - Config");
        SetConfig();
    }

    public void LoadStandardFile(){
        Debug.Log("Loading standard-config-file...");
        cf = Config_Manager.instance.LoadStandardFile();
        Debug.Log("Success - Config");
        SetConfig();
    }

    public void LoadWeatherData(){
        Debug.Log("Loading WeatherData...");
        StartCoroutine(LoadWeatherRoutine());
    }

    IEnumerator LoadWeatherRoutine(){
        yield return Config_Manager.instance.ShowLoadDialogCoroutine("weather");
        //LOADING WEATHER FROM JSON DIRECTLY, 20 CITIES, 25 DAYS, 1 RECORD PER HOUR -> 25*24=600
        var weatherAll = Config_Manager.instance.weather;
        Debug.Log("Success - Weather");
        Debug.Log("Checking for Data validity...");
        //Check for validity
        Assert.IsNotNull(weatherAll);
        foreach(var weather in weatherAll)
            Assert.AreEqual(weather.data.Count, 20); //20 cities
        //foreach(var x in weatherAll.data)
        //    Assert.AreEqual(x.cityData.Count, 240); //25 days * 24 hours = 600 records
        Debug.Log("Amount of days found: " + weatherAll.Length);
        Debug.Log("Data OK");

        //Single out city
        List<CitySummary> CS = new List<CitySummary>();
        foreach(var weather in weatherAll){
            foreach(var x in weather.data){
                //Debug.Log(x.cityName + " == " + cf.CITY);
                if(x.cityName == cf.CITY){
                    CS.Add(x);
                    break;
                }
            }
        }
        Assert.IsNotNull(CS);
        foreach(var c in CS)
            Assert.AreEqual(c.cityName, cf.CITY);
        Debug.Log("Loaded correct City: " + CS[0].cityName);

        TimeZoneInfo timezone = GetTimezone(CS[0].cityName);
        //INIT DATA
        //-----
        //Start by finding the first hour that corresponds to "startingHour"
        //Turn unixtime into local-time of city
        int indexData = 0;
        foreach(var x in CS[0].cityData){
            var utc = DateTimeOffset.FromUnixTimeSeconds(x.dt);
            var realtime = TimeZoneInfo.ConvertTimeFromUtc(utc.UtcDateTime, timezone);
            if(realtime.Hour == cf.STARTING_HOUR)
                break;
            indexData++;
        }
        var utcCheck = DateTimeOffset.FromUnixTimeSeconds(CS[0].cityData[indexData].dt);
        var realtimeCheck = TimeZoneInfo.ConvertTimeFromUtc(utcCheck.UtcDateTime, timezone);
        Assert.AreEqual(realtimeCheck.Hour, cf.STARTING_HOUR);
        Debug.Log("Success - Weather Validation");
        this.cs = CS.ToArray();
        this.startIndex = indexData;
        SetWeatherData();
    }

    public void SetLogfilePath(){
        Debug.Log("Setting Path...");
        StartCoroutine(RoutineSetLogfilePath());
        SetLogPathData();
    }

    IEnumerator RoutineSetLogfilePath(){
        yield return StartCoroutine(Config_Manager.instance.ShowSaveDialogCoroutine());
        logfilePath = Config_Manager.instance.logfilePath;
        Debug.Log("Success - Set Path");
    }

    public void ToggleVisibleLog(){
        this.logVisible = !this.logVisible;
    }
}
