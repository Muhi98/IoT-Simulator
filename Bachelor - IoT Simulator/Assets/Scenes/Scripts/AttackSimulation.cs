using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.UI;
using TMPro;

public class AttackSimulation : MonoBehaviour
{
    public GameObject attackList, animText;
    public Button executeButton;
    private TextMeshProUGUI text;
    private CustomDropdown cd;
    private Button button;
    private bool inProgress;
    public static AttackSimulation instance;
    public void ExecuteAttack(){
        if(cd.selectedItemIndex == 0)
            return;

        inProgress = true;
        button.interactable = false;
        executeButton.interactable = false;
        animText.SetActive(true);
        StartCoroutine(AnimateLoading());

        switch(cd.selectedItemIndex){
            case 1:
                StartCoroutine(OpenDoor());
                break;
            case 2:
                StartCoroutine(CreateMovement());
                break;
            case 3:
                StartCoroutine(CreateSmoke());
                break;
            case 4:
                StartCoroutine(LightFlickering());
                break;
            case 5:
                StartCoroutine(LoudMusic());
                break;
            case 6:
                Debug.Log("Open Window selected");
                Controller.instance.OpenWindow();
                inProgress = false;
                break;
            case 7:
                Debug.Log("Switch Camera detected");
                Controller.instance.SwitchCamera();
                inProgress = false;
                break;
            case 8:
                Debug.Log("Switch Light detected");
                Controller.instance.SwitchLight();
                inProgress = false;
                break;
            case 9:
                //Debug.Log("Fake sensor (hot) detected");
                StartCoroutine(FakeTemp(hot: true));
                break;
            case 10:
                //Debug.Log("Fake sensor (cold) detected");
                StartCoroutine(FakeTemp(hot: false));
                break;
            case 11:
                //Debug.Log("Influence real temperature (hot) detected");
                StartCoroutine(InfluenceTemp(hot: true));
                break;
            case 12:
                //Debug.Log("Influence real temperature (cold) detected");
                StartCoroutine(InfluenceTemp(hot: false));
                break;
            case 13:
                Debug.Log("Forget window detected");
                Person.instance.willForgetWindow = true;
                inProgress = false;
                break;
        }
    }
    IEnumerator AnimateLoading(){
        int index = 0;
        while(inProgress){
            switch(index){
                case 0:
                    text.text = ".";
                    index++;
                    break;
                case 1:
                    text.text = ". .";
                    index++;
                    break;
                case 2:
                    text.text = ". . .";
                    index++;
                    break;
                case 3:
                    text.text = ". . . .";
                    index = 0;
                    break;
            }
            yield return new WaitForSeconds(0.5f);
        }
        animText.SetActive(false);
        executeButton.interactable = true;
        button.interactable = true;
    }
    IEnumerator OpenWindow(){
        Debug.Log("Open Window selected");
        Controller.instance.OpenWindow();
        var pass = TIME_CONTROL.instance.PassMin(1);
        yield return new WaitUntil(() => TIME_CONTROL.instance.WaitForTime(pass));
        inProgress = false;
    }

    IEnumerator OpenDoor(){
        Debug.Log("Open Door selected");
        Controller.instance.InteractDoor();
        var pass = TIME_CONTROL.instance.PassMin(1); 
        yield return new WaitUntil(() => TIME_CONTROL.instance.WaitForTime(pass));
        Controller.instance.InteractDoor();
        inProgress = false;
    }

    IEnumerator CreateMovement(){
        Debug.Log("Create Movement selected");
        SIMULATOR_CONTROL.instance._movementNotNormal = true;
        int[] pass = null;
        int range = Random.Range(1, 5);
        for(int i = 0; i < range; i++){
            SIMULATOR_CONTROL.instance.noiseCreated(Random.Range(10, 50), Random.Range(30, 60));
            pass = TIME_CONTROL.instance.PassSec(25);
            yield return new WaitUntil(() => TIME_CONTROL.instance.WaitForTime(pass));
        }
        SIMULATOR_CONTROL.instance._movementNotNormal = false;
        inProgress = false;
    }

    IEnumerator CreateSmoke(){
        Debug.Log("Create Smoke selected");
        SIMULATOR_CONTROL.instance._smoke = true;
        var pass = TIME_CONTROL.instance.PassMin(5);
        yield return new WaitUntil(() => TIME_CONTROL.instance.WaitForTime(pass));
        SIMULATOR_CONTROL.instance._smoke = false;
        inProgress = false;
    }

    IEnumerator LightFlickering(){
        Debug.Log("Light flickering selected");
        int[] pass = null;
        for(int i = 0; i < 5; i++){
            Controller.instance.LightOn();
            pass = TIME_CONTROL.instance.PassSec(Random.Range(5,50));
            yield return new WaitUntil(() => TIME_CONTROL.instance.WaitForTime(pass));
            Controller.instance.LightOff();
            pass = TIME_CONTROL.instance.PassSec(Random.Range(5,50));
            yield return new WaitUntil(() => TIME_CONTROL.instance.WaitForTime(pass));
        }
        inProgress = false;
    }

    IEnumerator LoudMusic(){
        Debug.Log("Loud Music selected");
        Controller.instance.TurnOnStereo();
        Controller.instance.ChangeVolume("high");
        var pass = TIME_CONTROL.instance.PassSec(Random.Range(5,50));
        yield return new WaitUntil(() => TIME_CONTROL.instance.WaitForTime(pass));
        Controller.instance.TurnOffStereo();
        inProgress = false;
    }

    IEnumerator FakeTemp(bool hot){
        Debug.Log("Fake temp selected");
        //Disable temp sensor
        var x = GameObject.Find("TemperatureSensor");
        //use sensor to send false data
        var y = x.GetComponent<Sensor>();
        y.disabled = true;
        if(hot)
            y.forceNewData(50f);
        else
            y.forceNewData(5f);
        
        y.forceSend();
        var pass = TIME_CONTROL.instance.PassMin(Random.Range(2,5));
        yield return new WaitUntil(() => TIME_CONTROL.instance.WaitForTime(pass));
        //activate sensor again
        y.disabled = false;
        inProgress = false;
    }

    IEnumerator InfluenceTemp(bool hot){
        Debug.Log("Influence temp selected");
        //Deactivate automatic regulation
        Controller.instance.isAutomaticTempRegulation = false;
        Controller.instance.CloseWindow();
        if(hot){
            Controller.instance.TurnOnHeater();
            yield return new WaitUntil(() => SIMULATOR_CONTROL.instance._temperatur >= 50f);
            Controller.instance.TurnOffHeater();
        }
        else{
            Controller.instance.TurnOnAC();
            yield return new WaitUntil(() => SIMULATOR_CONTROL.instance._temperatur <= 5f);
            Controller.instance.TurnOffAC();
        }
        //activate automatic regulation again
        Controller.instance.isAutomaticTempRegulation = true;
        inProgress = false;
    }

    


    // Start is called before the first frame update
    void Start()
    {
        cd = attackList.GetComponent<CustomDropdown>();
        button = attackList.GetComponent<Button>();
        instance = this;
        text = animText.GetComponentInChildren<TextMeshProUGUI>();
    }
}
