using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Sensor : MonoBehaviour, ISensor
{
    public SensorPreset preset;
    private float timer = 0.0f;
    private float seconds = 0.0f;
    private bool ready = false;

    private SensorType sT;
    private object data;

    private TextMeshProUGUI headText;
    public bool disabled;

    public string GetName(){ return preset.name; }
    public SensorType GetSensorType() {return preset.sensorType; }

    void Start()
    {
        //send init data to controller
        Controller.instance.InitSensor(this) ;

        //Create text above head
        headText = SIMULATOR_CONTROL.instance.CreateOverheadText(this.gameObject.transform, this.preset.sensorType.ToString());
        disabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(!ready){
            timer += TIME_CONTROL.instance.getDelta();
            seconds = timer % 60;

            if(timer >= preset.intervallForSendData && !disabled){
                ready = true;
                timer = 0.0f;
                updateData();
            }
        }
    }

    public void updateData(){
        object newData = SIMULATOR_CONTROL.instance.data[preset.sensorType];
        if(newData != data){
            data = newData;
            sendData();
        }

        ready = false;
    }

    public void sendData(){
        if(!ready){
            Debug.Log("Not ready yet!");
            return;
        }
        ready = false;
        //SEND DATA HERE
        //----
        Controller.instance.SendData(this, data);
    }

    public void forceNewData(object newData){
        data = newData;
    }

    public void forceSend(){
        Controller.instance.SendData(this, data);
    }


    
}
