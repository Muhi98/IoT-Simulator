using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SIMULATOR_CONTROL : MonoBehaviour
{
    public TextMeshProUGUI dataText;
    public TextMeshProUGUI actualText;



    public static SIMULATOR_CONTROL instance;
    //Umgebung
    public float _temperatur;
    public float _humidity; //in prozent
    public int _lightLevel; 
    //0 -> stockdunkel; 10 -> direkter Sonnenschein
    public bool _smoke;
    private int _noise;
    //0 -> stille; 10 -> alarmlautstärke
    public bool _movement, _movementNotNormal;

    private List<int> noiseSources;

    private GameObject textClone;

    public Dictionary<SensorType, object> data = new Dictionary<SensorType, object>();
    private Person[] allPerson;
    private Person person;
    private CONFIG_FILE cf;

    void Awake() {
        Application.targetFrameRate = 2000;
    

        instance = this;
        //Create dummy text
        textClone = new GameObject();

        //get all person (for later)
        allPerson = GameObject.FindObjectsOfType<Person>();

        //get person
        person = GameObject.FindObjectOfType<Person>();

        //init sources
        noiseSources = new List<int>();

        //init
        data = new Dictionary<SensorType, object>();
        
        cf = StartScreenFunction.instance.cf;
    }


    // Start is called before the first frame update
    void Start()
    {
        _temperatur = cf.TEMPERATURE;
        _humidity = cf.HUMIDITY;
        _lightLevel = cf.LIGHT_LEVEL;
        _smoke = cf.SMOKE;
        _noise = cf.NOISE;
        _movement = cf.MOVEMENT;
        _movementNotNormal = false; //Dieb etc.
        PoissonRandomizer.SeedRandomness(cf.SEED);

        data.Add(SensorType.SmokeDetec, _smoke);
        data.Add(SensorType.HumiditySens, _humidity);
        data.Add(SensorType.LightSens, _lightLevel);
        data.Add(SensorType.MovementSens, _movement);
        data.Add(SensorType.NoiseSens, _noise);
        data.Add(SensorType.TemperaturSens, _temperatur);

        Debug.Log("Loaded SIMULATOR_CONTROL");
    }

    // Update is called once per frame
    void Update()
    {
        //i think this should be in Controller.cs
        

        //for one person, if we have more we should use the average/highest of all things that create noise (?)
        int nTemp = 0;
        foreach(int n in noiseSources){
            nTemp = Mathf.Max(nTemp, n);
        }
        _noise = Mathf.Max(nTemp, person.creatingNoise);


        //For later when multiple people are needed
        //How this could function is that there are two lists (outside & inside)
        //Once a person leaves the house they are put into outside-list
        //So we can later program a thief maybe
        /*
        foreach(var per in allPerson){
            if(per.isAtHome){
                //camera deactivate
                if(per.isMoving)
                    _movement = true;
            }
        }*/
        _movement = _movementNotNormal || _movement;
        
        actualText.text = string.Format("{0:0.0}%\n{1}\n{2}\n{3:0.0}°C\n{4}\n{5}", _humidity, _movement, _lightLevel, _temperatur, _noise, _smoke);
        data[SensorType.SmokeDetec] = _smoke;
        data[SensorType.HumiditySens] = _humidity;
        data[SensorType.LightSens] = _lightLevel;
        data[SensorType.NoiseSens] = _noise;
        data[SensorType.TemperaturSens] = _temperatur;
        data[SensorType.MovementSens] = _movement;

        
    }

    public void UpdateText(float hum, bool mov, int light, float temp, int noi, bool smo){
        dataText.text = string.Format("{0:0.0}%\n{1}\n{2}\n{3:0.0}°C\n{4}\n{5}", hum, mov, light, temp, noi, smo);
    }


    public TextMeshProUGUI CreateOverheadText(Transform parent, string name){
        var gameObj = Instantiate(textClone, parent);
        var headText = gameObj.AddComponent<TextMeshProUGUI>();

        headText.fontSizeMax = 0.5f;
        headText.fontSizeMin = 0.1f;
        headText.enableAutoSizing = true;
        headText.text = name;
        Vector3 vec3 = parent.position;
        vec3.y = vec3.y + .5f;
        headText.rectTransform.position = vec3;
        headText.rectTransform.sizeDelta = new Vector2(3, 0.5f);
        headText.color = Color.black;
        headText.alignment = TextAlignmentOptions.Center;

        return headText;
    }


    public void changeTempBy(float f){
        this._temperatur += f;
    }

    public void changeHumBy(float f){
        this._humidity += f;
    }

    public void noiseCreated(int db, int length){
        noiseSources.Add(db);
        StartCoroutine(noiseCountdown(db, length));
    }

    IEnumerator noiseCountdown(int db, int length){
        var x = TIME_CONTROL.instance.PassSec(length);
        yield return new WaitUntil(() => TIME_CONTROL.instance.WaitForTime(x));
        noiseSources.Remove(db);
    }

    public void ExitGame(){
        Application.Quit();
    }

}
