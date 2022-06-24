using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;
using System;

public class Controller : MonoBehaviour
{
    private string LOGFILE, LOGFILE_JSON, TEMP_LOGFILE_JSON, _LOGFILEJSONPATH_SET;
    struct jsonObj
    {
        public string DATE;
        public string DAY;
        public string TIME;
        public string ID;
        public string NEW_STATUS;

        public jsonObj(int _day, int[] _time, string _id, string _msg){
            //calculate correct datetime from unix
            var timezone = WeatherAPI.GetTimezone(StartScreenFunction.instance.cs[0].cityName);
            var utc = DateTimeOffset.FromUnixTimeSeconds(EXTERNAL_CONTROL.instance.CurrentWeatherData().dt);
            var realtime = TimeZoneInfo.ConvertTimeFromUtc(utc.UtcDateTime, timezone);

            TIME = TIME_CONTROL.instance.TimeToString(_time);
            DAY = realtime.ToString("dd.MM.yyyy");
            DATE = string.Format("{0}", (EXTERNAL_CONTROL.Day) ((_day - 1) % 7));
            ID = _id;
            NEW_STATUS = _msg;
        }
    }

    struct jsonForTest{
        public string TIME;
        public string OUT_TEMP;
        public string OUT_HUM;
        public string IN_TEMP;
        public string IN_HUM;

        public jsonForTest(int[] time, float out_temp, float out_hum, float in_temp, float in_hum){
            TIME = TIME_CONTROL.instance.TimeToString(time);
            OUT_TEMP = out_temp.ToString();
            OUT_HUM = out_hum.ToString();
            IN_TEMP = in_temp.ToString();
            IN_HUM = in_hum.ToString();
        }
    }
    public static int AC = 0;
    public static int DOOR = 1;
    public static int HEATER = 2;
    public static int WINDOW = 3;
    public static int LIGHT = 4;
    public static int CAMERA = 5;
    public static int STEREO = 6;

    public static Controller instance;
    public ControllerPreset preset;
    //private float timer = 0.0f;
    //private float seconds = 0.0f;
    //public bool ready = false;
    public Dictionary<string, object> sensorDict = new Dictionary<string, object>();
    public List<GenericDevice> deviceList = new List<GenericDevice>();

    private bool oldMovement;//, oldLightDetected;
    private int oldLightLevel, oldNoiseLevel;
    public bool personAtHomeThroughWifi, oldPersonAtHomeThroughWifi, isAutomaticTempRegulation;
    private bool logVisible;


    void Awake(){
        instance = this;
        LOGFILE = "REAL TIME DATE OF LOGFILE: " + DateTime.Now;
        LOGFILE_JSON = "[\n";
        TEMP_LOGFILE_JSON = "[\n";

        oldMovement = false;
        //oldLightDetected = false;
        oldLightLevel = 0;
        oldNoiseLevel = 0;
        personAtHomeThroughWifi = true;
        oldPersonAtHomeThroughWifi = true;
        isAutomaticTempRegulation = true;

        logVisible = StartScreenFunction.instance.logVisible;
        _LOGFILEJSONPATH_SET = StartScreenFunction.instance.logfilePath;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitDevice();
        var text = SIMULATOR_CONTROL.instance.CreateOverheadText(this.transform, "Contoller");
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(!ready){
            timer += TIME_CONTROL.instance.getDelta();
            seconds = timer % 60;

            if(seconds >= preset.intervallForSendRecvData){
                ready = true;
                timer = 0.0f;
                //update received data
            }
        }
        */
    }
    private void InitDevice(){
        deviceList.Add(GameObject.FindObjectOfType<ACDevice>());
        deviceList.Add(GameObject.FindObjectOfType<DoorDevice>());
        deviceList.Add(GameObject.FindObjectOfType<HeaterDevice>());
        deviceList.Add(GameObject.FindObjectOfType<WindowDevice>());
        deviceList.Add(GameObject.FindObjectOfType<LightDevice>());
        deviceList.Add(GameObject.FindObjectOfType<CameraDevice>());
        deviceList.Add(GameObject.FindObjectOfType<StereoDevice>());
        //deviceList.Add(GameObject.FindGameObjectWithTag("Device").GetComponent<IDevice>());


        Debug.Log("Device Count: " + deviceList.Count);
    }

    public void InitSensor(ISensor sensor){
        object data = null;
        switch(sensor.GetSensorType()){
                case SensorType.SmokeDetec:
                    data = false;
                    break;
                case SensorType.HumiditySens:
                    data = (float) 0.0f;
                    break;
                case SensorType.TemperaturSens:
                    data = (float) 0.0f;
                    break;
                case SensorType.MovementSens:
                    data = false;
                    break;
                case SensorType.LightSens:
                    data = 0;
                    break;
                case SensorType.NoiseSens:
                    data = 0;
                    break;
            }

        sensorDict.Add(sensor.GetName(), data);
    }
    public void SendData(ISensor sensor, object data){
        if(typeof(bool).IsInstanceOfType(data)){
            //Debug.Log("Received: " + sensor.GetName() + " with value: " + data);
        }
        else if(typeof(float).IsInstanceOfType(data)){
            //Debug.Log("Received: " + sensor.GetName() + " with value: " + data);
        }
        else if(typeof(int).IsInstanceOfType(data)){
            //Debug.Log("Received: " + sensor.GetName() + " with value: " + data);
        }
        else{
            Debug.Log("Data is not bool, float or int!");
        }

        sensorDict[sensor.GetName()] = data;
        UpdatePolicyEnforcement();

        //UPDATE TEXT
        SIMULATOR_CONTROL.instance.UpdateText((float) sensorDict["humidity"], (bool) sensorDict["movement"], (int) sensorDict["light"],
        (float) sensorDict["temperatur"], (int) sensorDict["noise"], (bool) sensorDict["smoke"]);
    }

    //Determines unsafe states etc.
    private void UpdatePolicyEnforcement(){
        var time = TIME_CONTROL.instance.getTime();
        //int minute = 0; //index
        //int hour = 1; //index
        //Debug.Log((float) sensorDict["temperatur"] >= 25.0f && !deviceList[AC].GetStatus() && !deviceList[WINDOW].Active);

        if((bool) sensorDict["smoke"]){
            //ALARM ON
        }
        if(isAutomaticTempRegulation){
            //Turn AC On
            if((float) sensorDict["temperatur"] >= 25.0f && !deviceList[AC].GetStatus() && !deviceList[WINDOW].Active){
                deviceList[AC].Activate();
                AddToLog("AC", "ON \t[TEMP: " + sensorDict["temperatur"] + "]");
            }
            //Turn AC Off
            if((float) sensorDict["temperatur"] <= 18.0f && deviceList[AC].GetStatus()){
                deviceList[AC].Deactivate();
                AddToLog("AC", "OFF \t[TEMP: " + sensorDict["temperatur"] + "]");
            }
            //Turn Heater On
            if((float) sensorDict["temperatur"] <= 16.0f && !deviceList[HEATER].Active && !deviceList[WINDOW].Active){
                deviceList[HEATER].Active = true;
                AddToLog("HEATER", "ON \t[TEMP: " + sensorDict["temperatur"] + "]");
            }
            //Turn Heater Off
            if((float) sensorDict["temperatur"] >= 22.0f && deviceList[HEATER].Active){
                deviceList[HEATER].Active = false;
                AddToLog("HEATER", "OFF \t[TEMP: " + sensorDict["temperatur"] + "]");
            }
        }
        //Log Movement
        if((bool) sensorDict["movement"] != oldMovement){
            oldMovement = !oldMovement;
            if((bool) sensorDict["movement"]){
                AddToLog("MOVEMENT_SENS", "ON");
            }
            else{
                AddToLog("MOVEMENT_SENS", "OFF");
            }          
        }

        //Log Light
        if((int) sensorDict["light"] != oldLightLevel){
            AddToLog("LIGHT_SENS", sensorDict["light"].ToString());
            oldLightLevel = (int) sensorDict["light"];
        }

        //Log noise
        if((int) sensorDict["noise"] != oldNoiseLevel){
            AddToLog("NOISE_SENS", sensorDict["noise"].ToString());
            oldNoiseLevel = (int) sensorDict["noise"];
        }

        //log person presence + Camera
        if(personAtHomeThroughWifi != oldPersonAtHomeThroughWifi){
            if(personAtHomeThroughWifi){
                AddToLog("OWNER_PRESENCE", "TRUE");
                deviceList[Controller.CAMERA].Deactivate();
            }
            else{
                AddToLog("OWNER_PRESENCE", "FALSE");
                deviceList[Controller.CAMERA].Activate();
            }

            oldPersonAtHomeThroughWifi = personAtHomeThroughWifi;
        }

        /*
        if(person.isAtHome){
            Controller.instance.deviceList[Controller.CAMERA].Deactivate();
            if(person.isMoving){
                _movement = true;
            } else{
                _movement = false;
            }
        } else {
            _movement = false;
            Controller.instance.deviceList[Controller.CAMERA].Activate();
        }
        */
    }

    public void AddToLog(string id, string msg){
        var time = TIME_CONTROL.instance.getTime();
        int minute = 0; //index
        int hour = 1; //index
        int day = 2; //index
        int second = 3; //index

        LOGFILE += string.Format("\n[DAY: {0} | {1:00}:{2:00}:{3:00}]\t{4} - \t\t{5}", time[day], time[hour], time[minute], time[second], id, msg);

        if(this.logVisible)
            InSimLogViewer.instance.AddLogToList(id, msg);
        //debug
        AddToLogJSON(id, msg);
    }

    private void AddToLogJSON(string id, string msg){
        var time = TIME_CONTROL.instance.getTimeNoDay();

        var obj = new jsonObj(TIME_CONTROL.instance.getDays(), time, id, msg);
        var json = JsonUtility.ToJson(obj, true);
        //Debug.Log("Wrote: " + json);
        json += ",";
        LOGFILE_JSON += json;
    }

    public void AddToTestLogJSON(float out_temp, float out_hum, float in_temp, float in_hum){
        var time = TIME_CONTROL.instance.getTimeNoDay();

        var obj = new jsonForTest(time, out_temp, out_hum, in_temp, in_hum);
        var json = JsonUtility.ToJson(obj, true);

        json += ",";
        TEMP_LOGFILE_JSON += json;
    }

    void OnDestroy(){
        LOGFILE_JSON = LOGFILE_JSON.Remove(LOGFILE_JSON.Length -1) + "\n]";
        TEMP_LOGFILE_JSON = TEMP_LOGFILE_JSON.Remove(TEMP_LOGFILE_JSON.Length -1) + "\n]";

        //Get path to logfile
        string fullPath = Application.dataPath;
        #if UNITY_EDITOR
            string pathJSON = Path.GetFullPath(Path.Combine(fullPath, @"..\..\LOGFILES\JSON\"));
            string pathTXT = Path.GetFullPath(Path.Combine(fullPath, @"..\..\LOGFILES\TXT\"));
        #else
            fullPath = this.GetType().Assembly.Location;
            string pathJSON = Path.GetFullPath(Path.Combine(fullPath, @"..\..\..\LOGFILES\JSON\"));
            string pathTXT = Path.GetFullPath(Path.Combine(fullPath, @"..\..\..\LOGFILES\TXT\"));
        #endif

        if(_LOGFILEJSONPATH_SET != ""){
            File.WriteAllText(_LOGFILEJSONPATH_SET + @"\LOGFILE_JSON_"+ string.Format("{0:MM_dd_yyyy HH_mm_ss}",DateTime.Now) +".json", LOGFILE_JSON);
            File.WriteAllText(_LOGFILEJSONPATH_SET + @"\LOGFILE_JSON_FOR_TEMPERATURE"+ string.Format("{0:MM_dd_yyyy HH_mm_ss}",DateTime.Now) +".json", TEMP_LOGFILE_JSON);
            Debug.Log(_LOGFILEJSONPATH_SET);
        }
        else{
            File.WriteAllText(pathJSON + @"\LOGFILE_JSON_"+ string.Format("{0:MM_dd_yyyy HH_mm_ss}",DateTime.Now) +".json", LOGFILE_JSON);
            File.WriteAllText(_LOGFILEJSONPATH_SET + @"\LOGFILE_JSON_FOR_TEMPERATURE"+ string.Format("{0:MM_dd_yyyy HH_mm_ss}",DateTime.Now) +".json", TEMP_LOGFILE_JSON);
            Debug.Log(pathJSON);
        }
        
        
        //right enviroment
        File.WriteAllText(pathTXT + "LOGFILE_"+ string.Format("{0:MM/dd/yyyy HH_mm_ss}",DateTime.Now) +".txt", LOGFILE);
        
    }


    public void InteractDoor(){
        if(deviceList[DOOR].Active){
            deviceList[DOOR].Deactivate();
        }
        else{
            deviceList[DOOR].Activate();
        }

        string msg = deviceList[DOOR].Active ? "UNLOCKED" : "LOCKED";
        AddToLog("DOOR", msg);
        
    }

    public void InteractWindow(){
        if(deviceList[WINDOW].Active)
            deviceList[WINDOW].Deactivate();
        else
            deviceList[WINDOW].Activate();

        string msg = deviceList[WINDOW].Active ? "OPENED" : "CLOSED";
        AddToLog("WINDOW", msg);
    }

    public void OpenWindow(){
        if(deviceList[WINDOW].Active)
            return;
        deviceList[WINDOW].Activate();
        AddToLog("WINDOW", "OPENED");

        if(isAutomaticTempRegulation){
            Controller.instance.deviceList[Controller.AC].Deactivate();
            Controller.instance.deviceList[Controller.HEATER].Deactivate();
        }
    }

    public void CloseWindow(){
        if(!deviceList[WINDOW].Active)
            return;
        deviceList[WINDOW].Deactivate();
        AddToLog("WINDOW", "CLOSED");
    }

    public void LightOn(){
        deviceList[LIGHT].Activate();
        AddToLog("LIGHT", "ON");
    }

    public void LightOff(){
        deviceList[LIGHT].Deactivate();
        AddToLog("LIGHT", "OFF");
    }

    public void SwitchLight(){
        if(deviceList[LIGHT].Active)
            LightOff();
        else
            LightOn();
    }

    public void TurnOnStereo(){
        deviceList[STEREO].Activate();
        AddToLog("STEREO", "ON");
    }

    public void TurnOffStereo(){
        deviceList[STEREO].Deactivate();
        AddToLog("STEREO", "OFF");
    }

    public void ChangeVolume(string mode){
        ((StereoDevice) deviceList[STEREO]).ChangeVolume(mode);
    }

    public void SwitchCamera(){
        //Debug.Log("Camera has been switched to " + !deviceList[CAMERA].Active + " | " + deviceList[CAMERA].GetName());
        if(!deviceList[CAMERA].Active){
            deviceList[CAMERA].Activate(); //Log inside 
        }
        else{
            deviceList[CAMERA].Deactivate();
        }
    }

    public void TurnOnAC(){
        deviceList[AC].Activate();
        AddToLog("AC", "ON \t[TEMP: " + sensorDict["temperatur"] + "]");
    }

    public void TurnOnHeater(){
        deviceList[HEATER].Activate();
        AddToLog("HEATER", "ON \t[TEMP: " + sensorDict["temperatur"] + "]");
    }

    public void TurnOffAC(){
        deviceList[AC].Deactivate();
        AddToLog("AC", "OFF \t[TEMP: " + sensorDict["temperatur"] + "]");
    }

    public void TurnOffHeater(){
        deviceList[HEATER].Deactivate();
        AddToLog("HEATER", "OFF \t[TEMP: " + sensorDict["temperatur"] + "]");
    }
    
}
