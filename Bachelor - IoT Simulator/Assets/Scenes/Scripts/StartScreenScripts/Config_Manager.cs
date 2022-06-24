using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using System.IO;
using static WeatherAPI;

public class Config_Manager : MonoBehaviour
{
    public static Config_Manager instance;
    public CONFIG_FILE config;
    public WeatherDataFL[] weather;
    public string logfilePath;
    void Awake(){
        instance = this;
        config = null;
        weather = null;
        logfilePath = "";
    }
    
    public WeatherDataFL[] LoadWeatherData(){
        //TIME_CONTROL.instance.StopTime();
        WeatherDataFL[] result = null;

        //FileBrowser.SetFilters(true, new FileBrowser.Filter("Json Files", ".json"));
        //FileBrowser.SetDefaultFilter(".json");

        FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        FileBrowser.ShowLoadDialog( ( paths ) => 
            { result = WeatherAPI.LoadWeatherData(paths[0]); },
			() => { Debug.Log( "Canceled" ); result = null;},
			FileBrowser.PickMode.Folders, false, null, null, "Select Weatherdata Folder", "Select" );
        return result;
    }

    public CONFIG_FILE LoadConfigFile(){
        //TIME_CONTROL.instance.StopTime();
        CONFIG_FILE cf = null;

        FileBrowser.SetFilters(true, new FileBrowser.Filter("Json Files", ".json"));
        FileBrowser.SetDefaultFilter(".json");

        FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        FileBrowser.ShowLoadDialog( ( paths ) => 
            { cf = GetJSON(paths[0]); },
			() => { Debug.Log( "Canceled" ); },
			FileBrowser.PickMode.Files, false, null, null, "Select JSON", "Select" );
		return cf;
    }

    public IEnumerator ShowLoadDialogCoroutine(string type)
	{
		// Show a load file dialog and wait for a response from user
		// Load file/folder: both, Allow multiple selection: true
		// Initial path: default (Documents), Initial filename: empty
		// Title: "Load File", Submit button text: "Load"
        if(type == "weather")
		    yield return FileBrowser.WaitForLoadDialog( FileBrowser.PickMode.Folders, false, null, null, "Select Weather folder", "Load" );
        else
            yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, null, "Select config.json", "Load");

		if( FileBrowser.Success ){
            if(type == "config")
                config = GetJSON(FileBrowser.Result[0]);
            else if(type == "weather")
                weather = WeatherAPI.LoadWeatherData(FileBrowser.Result[0]);
        }
	}

    public IEnumerator ShowSaveDialogCoroutine(){
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Folders, false, null, null, "Set logfile path", "Set");

        if(FileBrowser.Success){
            logfilePath = FileBrowser.Result[0];
        }
    }

    public CONFIG_FILE LoadStandardFile(){
        //Get path to json file
        string fullPath = Application.dataPath;
        #if UNITY_EDITOR
            Debug.Log("Unity-Editor");
            string pathJSON = Path.GetFullPath(Path.Combine(fullPath, @"..\..\CONFIG_FILES\standard_config.json"));
        #else
            Debug.Log("Unity-Exe");
            string pathJSON = this.GetType().Assembly.Location;
            pathJSON = Path.GetFullPath(Path.Combine(pathJSON, @"..\..\..\CONFIG_FILES\standard_config.json"));
            
        #endif
        
        return GetJSON(pathJSON);
    }

    private CONFIG_FILE GetJSON(string path){
        Debug.Log("Path to JSON-File: " + path);
        var jsonText = File.ReadAllText(path);

        var file = JsonUtility.FromJson<CONFIG_FILE>(jsonText);

        return file;
    }

}

[System.Serializable]
public class CONFIG_FILE{
    public int STARTING_HOUR;
    public string CITY;
    public float TEMPERATURE;
    public float HUMIDITY;
    public int  LIGHT_LEVEL;
    public bool SMOKE;
    public int NOISE;
    public bool MOVEMENT;
    //optional
    public int RUN_SIMULATION_FOR_X_DAYS;
    public float HOURS_PER_SECOND;
    public int SEED;
    //PERSON
    public float    WAKE_UP, WORK_TIME, DONE_WORK, ARRIVE_HOME, SLEEP_TIME, CHANCE_MOVING, CHANCE_OPEN_WINDOW, 
                    LAMBDA_WAKE_UP, LAMBDA_WORK_TIME, LAMBDA_DONE_WORK, LAMBDA_ARRIVE_HOME, LAMBDA_SLEEP_TIME;
    public bool AUTOMATIC;
}
