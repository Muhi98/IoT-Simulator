from time import time
from types import SimpleNamespace
import requests
from datetime import datetime, timedelta
import json
from marshmallow import Schema, fields
from zoneinfo import ZoneInfo


URL_CITYLESS = "http://api.openweathermap.org/data/2.5/onecall/timemachine?"
URL_APPENDIX = "&appid=6efa85566aace785fe4722c9d8586913&units=metric"

URL_HISTORY_CITYLESS = "http://history.openweathermap.org/data/2.5/history/city?"

cities = ["BERLIN", "PARIS", "BUENOS AIRES", "CANBERRA", "BRASILIA", "DARMSTADT", "NEW DELHI", "JAKARTA", "ROME", "TOKYO", "OTTAWA", "MEXICO CITY", "MOSCOW", "RIYADH", "PRETORIA", 
"SEOUL", "ANKARA", "LONDON", "WASHINGTON", "BEIJING"]

cities2 = ["BERLIN", "PARIS"]

city = "BERLIN"

def GetLatAndLong(city):
        switcher = {
            "BERLIN": [52.520008, 13.404954],
            "PARIS":  [48.864716, 2.349014],
            "BUENOS AIRES": [-34.603722, -58.381592],
            "CANBERRA": [-35.282001, 149.128998],
            "BRASILIA": [-15.793889, -47.882778],
            "DARMSTADT": [49.878708, 8.646927],
            "NEW DELHI": [28.644800, 77.216721],
            "JAKARTA": [-6.200000, 106.816666],
            "ROME": [41.902782, 12.496366],
            "TOKYO": [35.652832, 139.839478],
            "OTTAWA": [45.424721, -75.695000],
            "MEXICO CITY": [19.432608, -99.133209],
            "MOSCOW": [55.751244, 37.618423],
            "RIYADH": [24.774265, 46.738586],
            "PRETORIA": [-25.731340, 28.218370],
            "SEOUL":  [37.532600, 127.024612],
            "ANKARA":  [39.925533, 32.866287],
            "LONDON": [51.509865, -0.118092],
            "WASHINGTON": [38.900497, -77.007507],
            "BEIJING":  [39.916668, 116.383331],
        }
        return switcher.get(city, [52.520008, 13.404954])


def Get5Days():
    class WeatherData(Schema):
        humidity = fields.List(fields.Float())
        temp = fields.List(fields.Float())
        time = fields.List(fields.Int())

    class FullData(Schema):
        cityName = fields.Str()
        cityData = fields.Nested(WeatherData)

    class AllData(Schema):
        data = fields.List(fields.Nested(FullData))

    humidity = []
    temp = []
    time = []
    allData = []

    for z in range(len(cities)):
        currentCity = cities[z]
        coords = GetLatAndLong(currentCity)
        for x in range(5):
            day = datetime.now() - timedelta(days=5-x)
            FULL_URL = URL_CITYLESS + "lat=" + str(coords[0]) + "&lon=" + str(coords[1]) + "&dt=" + str((int(day.timestamp()))) + URL_APPENDIX
            response = requests.get(FULL_URL)
            obj = json.loads(response.text, object_hook=lambda d: SimpleNamespace(**d))
            for y in obj.hourly:
                time.append(y.dt)
                temp.append(y.temp)
                humidity.append(y.humidity)
        
        wd = dict(temp=temp, humidity=humidity, time=time)
        fd = dict(cityName=currentCity, cityData=wd)
        allData.append(fd)

    ad = dict(data=allData)

    #print back to json
    schema = AllData()
    result = schema.dump(ad)

    f = open("weatherdata.json", "w")
    f.write(str(result).replace('\'', '"'))
    f.close()

def GetHistory(amountDays: int):
    class SingleDataLine(Schema):
        humidity = fields.Float()
        temp = fields.Float()
        time = fields.Str()
        dt = fields.Int()

    class FullData(Schema):
        cityName = fields.Str()
        cityData = fields.List(fields.Nested(SingleDataLine))

    class AllData(Schema):
        data = fields.List(fields.Nested(FullData))

    correctTime = datetime.now()
    correctTime = correctTime - timedelta(hours=correctTime.hour) - timedelta(minutes=correctTime.minute) - timedelta(seconds=correctTime.second)
    print("Normalized time: " + str(correctTime))
    print("Amount of days to be collected: " + str(amountDays))
    print("----------------")
    hourCounter = 0

    #By day
    for x in range(amountDays):
        fd = []
        day = correctTime - timedelta(days=amountDays-x)
        print("Getting data for day: " + str(day))
        for c in cities:
            wd = []
            obj = [] #debug var
            coords = GetLatAndLong(c)
            

            
            FULL_URL = URL_HISTORY_CITYLESS + "lat=" + str(coords[0]) + "&lon=" + str(coords[1]) + "&type=hour&start=" + str((int(day.timestamp()))) + "&cnt=24" + URL_APPENDIX
            response = requests.get(FULL_URL)
            result = json.loads(response.text, object_hook=lambda d: SimpleNamespace(**d))
            obj.append(result) #only for debug
            for y in result.list:
                try:
                    x_temp = y.main.temp
                except AttributeError:
                    x_temp = y.main.feels_like
                    
                wd.append(dict(temp=x_temp, humidity=y.main.humidity, dt=y.dt, time=str(datetime.fromtimestamp(y.dt))))
            fd.append(dict(cityName=c, cityData=wd))

        ad = dict(data=fd)
        schema = AllData()
        result = schema.dump(ad)
        fileName = "weatherdata_" + str(day.day) + "-" +str(day.month) +"-"+ str(day.year) + ".json"
        f = open(fileName, "w")
        f.write(str(result).replace('\'', '"'))
        f.close()


    '''
    #By City
    fd = []
    for c in cities:
        wd = []
        coords = GetLatAndLong(c)
        print("Getting data for: " + c + "...")


        if(amountDays < 6):
            day = datetime.now() - timedelta(days=amountDays)
            FULL_URL = URL_HISTORY_CITYLESS + "lat=" + str(coords[0]) + "&lon=" + str(coords[1]) + "&type=hour&start=" + str((int(day.timestamp()))) + "&cnt=168" + URL_APPENDIX
            response = requests.get(FULL_URL)
            return json.loads(response.text, object_hook=lambda d: SimpleNamespace(**d))


        obj = []
        for x in range(int(amountDays / 5)):
            day = correctTime - timedelta(days=amountDays-x*5)
            FULL_URL = URL_HISTORY_CITYLESS + "lat=" + str(coords[0]) + "&lon=" + str(coords[1]) + "&type=hour&start=" + str((int(day.timestamp()))) + "&cnt=120" + URL_APPENDIX
            
            response = requests.get(FULL_URL)
            result = json.loads(response.text, object_hook=lambda d: SimpleNamespace(**d))
            obj.append(result)
            for y in result.list:
                try:
                    x_temp = y.main.temp
                except AttributeError:
                    x_temp = y.main.feels_like
                    
                wd.append(dict(temp=x_temp, humidity=y.main.humidity, dt=y.dt, time=str(datetime.fromtimestamp(y.dt))))

        
        fd.append(dict(cityName=c, cityData=wd))

    ad = dict(data=fd)
    schema = AllData()
    result = schema.dump(ad)
    
    return obj, result
    '''



GetHistory(14) 

'''
#json
f = open("weatherdata_history.json", "w")
f.write(str(jsonResult).replace('\'', '"'))
f.close()
'''