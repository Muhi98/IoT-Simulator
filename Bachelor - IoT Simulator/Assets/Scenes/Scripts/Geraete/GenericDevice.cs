using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class GenericDevice : MonoBehaviour, IDevice
{
    private bool _active;
    protected TextMeshProUGUI textMeshProUGUI;
    protected Timer timer;

    public DevicePreset dp;

    public GenericDevice instance;
    
    public string GetName(){ return dp.name;}
    public virtual void Activate() { _active = true;}
    public virtual void Deactivate() { _active = false;}
    public bool GetStatus() {return _active;}

    public bool Active {
        get => _active;
        set => _active = value;
    }
    
    // Start is called before the first frame update
    virtual protected void Start()
    {
        textMeshProUGUI = SIMULATOR_CONTROL.instance.CreateOverheadText(this.transform, dp.name);
        timer = new Timer(dp.intervallForUpdatingInfo);
        instance = this;
        _active = false;
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        if(timer.UpdateTimer() && _active)
            updateData();

        if(_active)
            textMeshProUGUI.color = Color.red;
        else
            textMeshProUGUI.color = Color.black;
    }


    protected abstract void updateData();
}
