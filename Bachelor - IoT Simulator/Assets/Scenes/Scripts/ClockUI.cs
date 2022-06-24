using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ClockUI : MonoBehaviour
{
    private Transform clockHandTransform;
    private Transform clockHandHourTransform;
    private TextMeshProUGUI timeText;
    void Awake(){
        clockHandTransform = transform.Find("clockHand");
        clockHandHourTransform = transform.Find("clockHandHour");
        timeText = transform.Find("timeText").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update(){
        clockHandTransform.eulerAngles = new Vector3(0, 0, -TIME_CONTROL.instance.getMinute() * 6f + (-TIME_CONTROL.instance.getSeconds() * 0.1f));
        clockHandHourTransform.eulerAngles = new Vector3(0, 0, -TIME_CONTROL.instance.getHour() * 30f + (-TIME_CONTROL.instance.getMinute() * 0.5f));
        timeText.text = string.Format("{0:00}:{1:00}:{2:00}\nDay {3}", TIME_CONTROL.instance.getHour(), TIME_CONTROL.instance.getMinute(), (int) TIME_CONTROL.instance.getSeconds(), TIME_CONTROL.instance.getDays());
    }
}
