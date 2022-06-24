using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;

public static class WeatherAPI{
    public const string URL_HISTORY = "https://api.openweathermap.org/data/2.5/onecall/timemachine?lat=49.703781&lon=7.319240&appid=6efa85566aace785fe4722c9d8586913&units=metric";
    public const string URL_CITYLESS = "https://api.openweathermap.org/data/2.5/onecall/timemachine?";
    public const string URL_APPENDIX = "&appid=6efa85566aace785fe4722c9d8586913";

    public static WeatherData GetResponse(DateTimeOffset day, string optionalPar="", string city="NONE"){
        float lat, longt;
        (lat, longt) = GetLatAndLong(city);

        var correctURL = String.Format("{0}lat={1}&lon={2}{3}&dt={4}", URL_CITYLESS, lat, longt, URL_APPENDIX, day.ToUnixTimeSeconds());
        Debug.Log(correctURL);
        string result = "";

        var httpRequest = (HttpWebRequest)WebRequest.Create(correctURL);
        httpRequest.Accept = "application/json";
        
        var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream())){
            result = streamReader.ReadToEnd(); 
        }

        Debug.Log(day.ToString("dd.MM.yyyy") + ": " + httpResponse.StatusCode);


        return JsonUtility.FromJson<WeatherData>(result);
    }

    
public static WeatherDataFL[] LoadWeatherData(string path){
    //path has all the weatherdata
    string[] fileEntries = Directory.GetFiles(path); 
    List<WeatherDataFL> results = new List<WeatherDataFL>();
    foreach(var x in fileEntries){
        if(x.Contains(".py"))
            continue;

        var tempResult = System.IO.File.ReadAllText(x);
        results.Add(JsonUtility.FromJson<WeatherDataFL>(tempResult));
    }
    return results.ToArray();

    
    //string result = System.IO.File.ReadAllText(path);
    //return JsonUtility.FromJson<WeatherDataFL>(result);
}

//WeatherDataFromFile
[System.Serializable]
public class WeatherDataFL{
    public List<CitySummary> data;
}
[System.Serializable]
public class CitySummary{
    public string cityName;
    public List<CityData> cityData;
}
[System.Serializable]
public class CityData{
    public float humidity;
    public float temp;
    public string time;
    public int dt;
}

[System.Serializable]
public class WeatherData
{
    public float lat;
    public float lon;

    public long timezone;
    public long timezone_offset;
    public current current;
    public List<hourly> hourly;

}

[System.Serializable]
public class current{
    public long dt;
    public float temp;
    public long sunrise;
    public long sunset;

}

[System.Serializable]
public class hourly{
    public long dt;
    public float temp;
    public float feels_like;
    public float pressure;
    public float humidity;
    public float clouds;
}



//data from https://www.latlong.net
    public static (float, float) GetLatAndLong(string city){
        switch(city){
            case "BERLIN":
                return (52.520008f, 13.404954f);
            case "PARIS":
                return (48.864716f, 2.349014f);
            case "BUENOS AIRES":
                return (-34.603722f, -58.381592f);
            case "CANBERRA":
                return (-35.282001f, 149.128998f);
            case "BRASILIA":
                return (-15.793889f, -47.882778f);
            case "DARMSTADT":
                return (49.878708f, 8.646927f);
            case "NEW DELHI":
                return (28.644800f, 77.216721f);
            case "JAKARTA":
                return (-6.200000f, 106.816666f);
            case "ROME":
                return (41.902782f, 12.496366f);
            case "TOKYO":
                return (35.652832f, 139.839478f);
            case "OTTAWA":
                return (45.424721f, -75.695000f);
            case "MEXICO CITY":
                return (19.432608f, -99.133209f);
            case "MOSCOW":
                return (55.751244f, 37.618423f);
            case "RIYADH":
                return (24.774265f, 46.738586f);
            case "PRETORIA":
                return (-25.731340f, 28.218370f);
            case "SEOUL":
                return (37.532600f, 127.024612f);
            case "ANKARA":
                return (39.925533f, 32.866287f);
            case "LONDON":
                return (51.509865f, -0.118092f);
            case "WASHINGTON":
                return (38.900497f, -77.007507f);
            case "BEIJING":
                return (39.916668f, 116.383331f);
            default:
                return (52.520008f, 13.404954f);
        }
    }

    public static TimeZoneInfo GetTimezone(string city){
        switch(city){
            case "BERLIN":
                return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            case "PARIS":
                return TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
            case "BUENOS AIRES":
                return TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
            case "CANBERRA":
                return TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
            case "BRASILIA":
                return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            case "DARMSTADT":
                return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            case "NEW DELHI":
                return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            case "JAKARTA":
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            case "ROME":
                return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            case "TOKYO":
                return TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            case "OTTAWA":
                return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            case "MEXICO CITY":
                return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
            case "MOSCOW":
                return TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
            case "RIYADH":
                return TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
            case "PRETORIA":
                return TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");;
            case "SEOUL":
                return TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
            case "ANKARA":
                return TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            case "LONDON":
                return TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            case "WASHINGTON":
                return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            case "BEIJING":
                return TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            default:
                return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        }
    }
}

