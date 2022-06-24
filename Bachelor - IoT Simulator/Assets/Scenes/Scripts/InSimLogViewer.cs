using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using TMPro;

public class InSimLogViewer : MonoBehaviour
{
    public  GameObject LogWindow;
    public  GameObject listItemPrefab;
    public  GameObject list;
    public static InSimLogViewer instance;
    private  int amount = 0;
    public  int max;

    void Awake(){
        instance = this;
        
    }

    void Start(){
        for(int i = 0; i < max; i++){
            var item = Instantiate(listItemPrefab);
            item.transform.SetParent(list.transform);
            item.name = "Item #" + i;
            var tm = item.GetComponentInChildren<TextMeshProUGUI>();
            tm.text = "NULL";
            item.SetActive(false);
        }
    }

    public void AddLogToList(string id, string msg){
        if(amount == max){
            //Debug.Log("Destroying: " + list.transform.GetChild(max-1).name);
            list.transform.GetChild(max-1).gameObject.SetActive(false);
            amount--;
        }
        var item = list.transform.GetChild(amount);
        var tm = item.GetComponentInChildren<TextMeshProUGUI>();
        tm.text = string.Format("{0} {1}", id, msg);
        item.gameObject.SetActive(true);
        item.SetAsFirstSibling();
        amount++;
    }

    public void ToggleWindow(){
        LogWindow.SetActive(!LogWindow.activeSelf);
    }

}
