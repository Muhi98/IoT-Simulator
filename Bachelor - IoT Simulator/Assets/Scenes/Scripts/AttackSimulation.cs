using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.UI;
using TMPro;

public class AttackSimulation : MonoBehaviour
{
    public GameObject buttonContainer;
    public TextMeshProUGUI inProgressText, currentPushedText, currentSituationText;
    private int deviceNumberText;
    private string sensorDictText;


    public GameObject attackList, animText;
    public Button executeButton;
    private TextMeshProUGUI text;
    private CustomDropdown cd;
    private Button button;
    private bool inProgress, cancelNow;
    public static AttackSimulation instance;
    private GameObject oldButton;

    private List<Button> activeButtons;

    void Awake(){
        cancelNow = false;
        activeButtons = new List<Button>();
        foreach(Transform child in buttonContainer.transform){
            activeButtons.Add(child.GetComponent<Button>());
        }


        cd = attackList.GetComponent<CustomDropdown>();
        button = attackList.GetComponent<Button>();
        instance = this;
        text = animText.GetComponentInChildren<TextMeshProUGUI>();
        inProgressText.text = "NONE IN PROGRESS";
        currentSituationText.text = "CURRENT:\nNONE";

        deviceNumberText = -1;
        sensorDictText = "null";
    }

    private void DisableAllButton(){
        foreach(var x in activeButtons){
            x.interactable = false;
        }
    }

    private void EnableAllButtons(){
        foreach(var x in activeButtons){
            x.interactable = true;
        }
    }

    public void Cancel(){
        StartCoroutine(ClearCancel());
    }

    IEnumerator ClearCancel(){
        cancelNow = true;
        yield return new WaitForSeconds(1f);
        cancelNow = false;
    }

    public void EnableOptions(GameObject button){
        currentPushedText.text = button.name;
        
        
        //disable old
        if(oldButton != null){
            for(int i = 1; i < oldButton.transform.childCount; i++){
                oldButton.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        oldButton = button;

        //index 0 is txt -> skip
        for(int i = 1; i < button.transform.childCount; i++){
            button.transform.GetChild(i).gameObject.SetActive(true);
        }


        switch(button.name){
            case "SensorBut":
                sensorDictText = "temperatur";
                deviceNumberText = -1;
                break;
            case "DoorBut":
                sensorDictText = "null";
                deviceNumberText = Controller.DOOR;
                break;
            case "WindowBut":
                sensorDictText = "null";
                deviceNumberText = Controller.WINDOW;
                break;
            case "MotionBut":
                sensorDictText = "movement";
                deviceNumberText = -1;
                break;
            case "SmokeBut":
                sensorDictText = "smoke";
                deviceNumberText = -1;
                break;
            case "LightBut":
                sensorDictText = "light";
                deviceNumberText = Controller.LIGHT;
                break;
            case "SoundBut":
                sensorDictText = "noise";
                deviceNumberText = Controller.STEREO;
                break;
            case "CameraBut":
                sensorDictText = "null";
                deviceNumberText = Controller.CAMERA;           
                break;
            case "HeatingBut":
                sensorDictText = "temperatur";
                deviceNumberText = Controller.HEATER;
                break;
            case "ACBut":
                sensorDictText = "temperatur";
                deviceNumberText = Controller.AC;
                break;
        }
    }

    void Update(){
        if(!inProgress){
            inProgressText.text = "NONE IN PROGRESS";

            if(!buttonContainer.GetComponentInChildren<Button>().IsInteractable()){
                foreach(Transform x in buttonContainer.transform)
                    x.GetComponent<Button>().interactable = true;
            }
        }
        else{
            //buttonContainer
            if(buttonContainer.GetComponentInChildren<Button>().IsInteractable()){
                foreach(Transform x in buttonContainer.transform)
                    x.GetComponent<Button>().interactable = false;
            }
        }
        
        object outValue;
        Controller.instance.sensorDict.TryGetValue(sensorDictText, out outValue);

        if(deviceNumberText != -1 && sensorDictText != "null")
            currentSituationText.text = "CURRENT:\n" + outValue + "\n" + Controller.instance.deviceList[deviceNumberText].Active;
        else if(deviceNumberText == -1 && sensorDictText != "null")
            currentSituationText.text = "CURRENT:\n" + outValue + "\n";
        else if(deviceNumberText != -1 && sensorDictText == "null")
            currentSituationText.text = "CURRENT:\n" + Controller.instance.deviceList[deviceNumberText].Active;
        else
            currentSituationText.text = "CURRENT:\nNONE";
    }

    public void TextAttack(GameObject button){
        var textField = button.GetComponentInChildren<TMP_InputField>();
        float value = -1;
        float.TryParse(textField.text, out value);
        Debug.Log(value);
        if(value == -1)
            return;

        switch(button.name){
            case "SensorBut":
                inProgressText.text = "CURRENT PROGRESS:\nSENSOR TO " + value;
                StartCoroutine(FakeTemp(value));
                break;
            case "HeatingBut":
                inProgressText.text = "CURRENT PROGRESS:\nHEATER TO " + value;
                StartCoroutine(InfluenceTemp(true, value));
                break;
            case "ACBut":
                inProgressText.text = "CURRENT PROGRESS:\nAC TO " + value;
                StartCoroutine(InfluenceTemp(false, value));
                break;
            default:
                Debug.Log("Error with TextButton!");
                break;
        }
    }

    public void OnAttack(GameObject button){
        string name = button.name;

        switch(name){
            case "DoorBut":
                Controller.instance.OpenDoor();
                break;
            case "WindowBut":
                Controller.instance.OpenWindow();
                break;
            case "MotionBut":
                inProgressText.text = "CURRENT PROGRESS:\nMOVEMENT";
                StartCoroutine(CreateMovement());
                break;
            case "SmokeBut":
                SIMULATOR_CONTROL.instance._smoke = true;
                break;
            case "SoundBut":
                inProgressText.text = "CURRENT PROGRESS:\nSOUND";
                StartCoroutine(LoudMusic());
                break;
            case "LightBut":
                Controller.instance.LightOn();
                break;
            case "CameraBut":
                Controller.instance.SwitchCamera();
                break;
            default:
                Debug.Log("Error with OnButton!");
                break;

            
        }
    }

    public void OffAttack(GameObject button){
        string name = button.name;

        switch(name){
            case "DoorBut":
                Controller.instance.CloseDoor();
                break;
            case "WindowBut":
                Controller.instance.CloseWindow();
                break;
            case "SmokeBut":
                SIMULATOR_CONTROL.instance._smoke = false;
                break;
            case "LightBut":
                Controller.instance.LightOff();
                break;
            case "CameraBut":
                Controller.instance.SwitchCamera();
                break;
            default:
                Debug.Log("Error with OffButton!");
                break;
        }
    }

    public void CustomAttack(GameObject button){
        string name = button.name;

        switch(name){
            case "WindowBut":
                Person.instance.willForgetWindow = true;
                break;
            case "LightBut":
                inProgressText.text = "CURRENT PROGRESS:\nLIGHT";
                StartCoroutine(LightFlickering());
                break;
            default:
                Debug.Log("Error CustomButton!");
                break;
        }
    }

    IEnumerator CreateMovement(){
        Debug.Log("Create Movement selected");
        inProgress = true;
        SIMULATOR_CONTROL.instance._movementNotNormal = true;
        int[] pass = null;
        int range = Random.Range(1, 5);
        for(int i = 0; i < range; i++){
            SIMULATOR_CONTROL.instance.noiseCreated(Random.Range(10, 50), Random.Range(30, 60));
            pass = TIME_CONTROL.instance.PassSec(25);
            yield return new WaitUntil(() => TIME_CONTROL.instance.WaitForTime(pass) || cancelNow);
        }
        SIMULATOR_CONTROL.instance._movementNotNormal = false;
        inProgress = false;
    }

    IEnumerator CreateSmoke(){
        Debug.Log("Create Smoke selected");
        inProgress = true;
        SIMULATOR_CONTROL.instance._smoke = true;

        yield return new WaitUntil(() => cancelNow);
        SIMULATOR_CONTROL.instance._smoke = false;
        inProgress = false;
    }

    IEnumerator LightFlickering(){
        Debug.Log("Light flickering selected");
        inProgress = true;
        int[] pass = null;
        for(int i = 0; i < 5; i++){
            Controller.instance.LightOn();
            pass = TIME_CONTROL.instance.PassSec(Random.Range(5,50));
            yield return new WaitUntil(() => TIME_CONTROL.instance.WaitForTime(pass) || cancelNow);
            Controller.instance.LightOff();
            pass = TIME_CONTROL.instance.PassSec(Random.Range(5,50));
            yield return new WaitUntil(() => TIME_CONTROL.instance.WaitForTime(pass) || cancelNow);
        }
        inProgress = false;
    }

    IEnumerator LoudMusic(){
        Debug.Log("Loud Music selected");
        inProgress = true;
        Controller.instance.TurnOnStereo();
        Controller.instance.ChangeVolume("high");
        var pass = TIME_CONTROL.instance.PassSec(Random.Range(5,50));
        yield return new WaitUntil(() => TIME_CONTROL.instance.WaitForTime(pass) || cancelNow);
        Controller.instance.TurnOffStereo();
        inProgress = false;
    }

    IEnumerator FakeTemp(float temp){
        Debug.Log("Fake temp selected");
        inProgress = true;
        //Disable temp sensor
        var x = GameObject.Find("TemperatureSensor");
        //use sensor to send false data
        var y = x.GetComponent<Sensor>();
        y.disabled = true;
        
        y.forceNewData(temp);
        y.forceSend();

        yield return new WaitUntil(() => cancelNow);
        //activate sensor again
        y.disabled = false;
        inProgress = false;
    }

    IEnumerator InfluenceTemp(bool hot, float value){
        Debug.Log("Influence temp selected");
        inProgress = true;
        //Deactivate automatic regulation
        Controller.instance.isAutomaticTempRegulation = false;
        Controller.instance.CloseWindow();
        if(hot){
            Controller.instance.TurnOnHeater();
            Controller.instance.TurnOffAC();
            yield return new WaitUntil(() => SIMULATOR_CONTROL.instance._temperatur >= value || cancelNow);
            Controller.instance.TurnOffHeater();
        }
        else{
            Controller.instance.TurnOnAC();
            Controller.instance.TurnOffHeater();
            yield return new WaitUntil(() => SIMULATOR_CONTROL.instance._temperatur <= value || cancelNow);
            Controller.instance.TurnOffAC();
        }
        //activate automatic regulation again
        Controller.instance.isAutomaticTempRegulation = true;
        inProgress = false;
    }

    


    // Start is called before the first frame update
    
}
