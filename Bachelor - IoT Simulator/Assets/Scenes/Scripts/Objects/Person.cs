using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Michsky.UI;


//This script is for interacting with the house and simulating the persons behavior
public class Person : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI statusText, choiceText;
    [SerializeField]
    private TMP_Dropdown dropdownChoices;

    private string status, choiceTopText;

    [System.NonSerialized]
    public bool isAtHome, isMoving, isAwake, cooked, stayOut;
    public int creatingNoise;
    private Timer timer; 

    [SerializeField]
    private float atWakeUp, atTimeForWork, atDoneWork, atArriveHome, atSleepTime, genericOffsetMin, genericOffsetMax, chanceOfMoving, chanceOfOpeningWindow;
    private float wakeUpReset, timeForWorkReset, doneWorkReset, arriveHomeReset, sleepTimeReset, offsetMinReset, offsetMaxReset, movingReset, windowReset;

    [Space(10)]
    [Header("Lambdas")]
    [SerializeField]private float lambdaAtWakeUp;
    [SerializeField]private float lambdaAtTimeForWork;
    [SerializeField]private float lambdaAtDoneWork;
    [SerializeField]private float lambdaAtArriveHome;
    [SerializeField]private float lambdaAtSleepTime;

    
    public bool willForgetWindow;
    


    private List<GenericDevice> devices;

    public static Person instance;

    //automatic control
    [SerializeField]
    private bool _automatic;
    public bool Automatic{
        set => _automatic = value;
    }

    void Awake()
    {
        instance = this;

        isAtHome = true;
        isMoving = false;
        isAwake = false;
        cooked = false;
        status = "NULL";
        stayOut = false;
        _automatic = false;
        creatingNoise = 0;
        willForgetWindow = false;


        //LOAD CONFIG
        var cf = StartScreenFunction.instance.cf;

        atWakeUp = cf.WAKE_UP;
        atTimeForWork = cf.WORK_TIME;
        atDoneWork = cf.DONE_WORK;
        atArriveHome = cf.ARRIVE_HOME;
        atSleepTime = cf.SLEEP_TIME;
        chanceOfMoving = cf.CHANCE_MOVING;
        chanceOfOpeningWindow = cf.CHANCE_OPEN_WINDOW;
        _automatic = cf.AUTOMATIC;

        lambdaAtSleepTime = cf.LAMBDA_SLEEP_TIME;
        lambdaAtWakeUp = cf.LAMBDA_WAKE_UP;
        lambdaAtTimeForWork = cf.LAMBDA_WORK_TIME;
        lambdaAtDoneWork = cf.LAMBDA_DONE_WORK;
        lambdaAtArriveHome = cf.LAMBDA_ARRIVE_HOME;


        //Set resets
        wakeUpReset = atWakeUp;
        timeForWorkReset = atTimeForWork;
        doneWorkReset = atDoneWork;
        arriveHomeReset = atArriveHome;
        sleepTimeReset = atSleepTime;
        offsetMinReset = genericOffsetMin;
        offsetMaxReset = genericOffsetMax;
        movingReset = chanceOfMoving;
        windowReset = chanceOfOpeningWindow;
        Debug.Log("Loaded PERSON");
    }

    void Start(){
        //StartCoroutine(Debug_Day());
        StartCoroutine(Behaviour_A());
        devices = Controller.instance.deviceList;


        dropdownChoices.ClearOptions();
        dropdownChoices.gameObject.SetActive(false);
        choiceText.enabled = false;
    }

    void Update()
    {
        //Check for moving,etc.
        if(isMoving && isAtHome){
            SIMULATOR_CONTROL.instance._movement = true;
        } else{
            SIMULATOR_CONTROL.instance._movement = false;
        }
        

        if(!isAtHome){
            creatingNoise = 0;
            Controller.instance.personAtHomeThroughWifi = false;

        }
        if(isAtHome)
            Controller.instance.personAtHomeThroughWifi = true;
        statusText.text = string.Format("<b>Current Status</b>\n{0}", status);
        
    }

    //MAIN-CR
    IEnumerator Behaviour_A(){
        //Weekday
        for(;;){
            for(int i = 0; i < 5; i++){
                //Reset values
                ResetValues();
                //Sleeping
                yield return StartCoroutine(Sleep_GetReadyForWork_Routine());
                status = "At work";
                yield return StartCoroutine(LeaveWork_Normal_Choices());
            }

            //Weekend
            for(int i = 0; i < 2; i++){
                Debug.Log("Weekend");
            }
        }
    }

    //SUB-CR  
    IEnumerator Sleep_GetReadyForWork_Routine(){
        //Sleeping (start 6am) or outside bc of choice
        if(stayOut)
            status = "Outside";
        else
            status = "Sleeping";

        creatingNoise = 0;

        var time = RandomizeTime((int) atWakeUp, 0, RandomPoisson(lambdaAtWakeUp));
        Debug.Log("Wake up at: " + TimeToString(time));
        yield return new WaitUntil(() => WaitForTime(time));

        Debug.Log("Woke up");
        status = "Getting ready for work";

        if(stayOut){
            yield return StartCoroutine(Come_Home());
            stayOut = false; //reset
        }

        
        /*
        //how many hours till work?
        int timeLeftH = (int) atTimeForWork - time[0];
        int timeLeftM = 0;
        if(timeLeftH == 0) timeLeftM = 60 - TIME_CONTROL.instance.getMinute();
        //calculate random intervall intervals of moving
        var chunks = (int) Random.Range(5, (timeLeftH*60 + timeLeftM) * 0.5f);
        var intervall = (timeLeftH*60 + timeLeftM) / chunks;

        int[] nowPls = null;

        Debug.Log(string.Format("{0} Chunks in {1} intervalls", chunks, intervall));
        //turn on lights
        Controller.instance.LightOn();

        for(int i = 0; i < chunks - 1; i++){
            nowPls = PassMin(intervall, TIME_CONTROL.instance.getTimeNoDay());
            //Debug.Log(string.Format("Chunk {0} waits till {1}", i, TimeToString(nowPls)));

            yield return new WaitUntil(() => WaitForTime(nowPls));

            //Random movement in those chunks
            if(Random.Range(0, 99) < chanceOfMoving)
                isMoving = true;
            else
                isMoving = false;

            if(Random.Range(0, 99) < chanceOfOpeningWindow)
                Controller.instance.OpenWindow();
            else
                Controller.instance.CloseWindow();

            if(isAtHome)
                creatingNoise = Random.Range(0, 101);
            else
                creatingNoise = 0;
            
        }
        */
        //Block above is this function now
        yield return StartCoroutine(At_Home_Randomness(atTimeForWork, false, false));
        //Close light & window
        Controller.instance.LightOff();
        Controller.instance.CloseWindow();
        

        //Getting ready to leave
        isMoving = true;
        time = RandomizeTime((int) atTimeForWork, 0, RandomPoisson(lambdaAtTimeForWork));
        Debug.Log("Leaving at: " + TimeToString(time));
        yield return new WaitUntil(() => WaitForTime(time));
        //Leave house
        yield return StartCoroutine(Leave_House());
        //Debug.Log("DONE SLEEP_GETREADY ROUTINE");
    }

    IEnumerator LeaveWork_Normal_Choices(){
        isAtHome = false;
        isMoving = true;
        var time = RandomizeTime((int) atDoneWork, 0, RandomPoisson(lambdaAtDoneWork));

        Debug.Log("Done work at: " + TimeToString(time));
        yield return new WaitUntil(() => WaitForTime(time));
        status = "Done with work";


        //CHOICE [AFTER WORK]
        //GO STRAIGHT HOME (NORMAL ARRIVE TIME + RANDOM) || GO SHOPPING/OUT/ETC. (LATE ARRIVE TIME + RANDOM)
        OpenOptionDialogue(new List<string>(){"", "Normal", "Delayed (+ 1 hour)"}, "AFTER WORK");
        yield return new WaitUntil(() => CheckForOption());
        CloseOptionDialogue();
        //Switch for option
        switch(chosenOption){
            case 1:
                Debug.Log("Go home normally");
                status = "Going home";
                break;
            case 2:
                Debug.Log("Go delayed");
                status = "Going home a little later";
                atArriveHome += 1;
                break;
            default:
                Debug.Log("Something went wrong");
                break;
        }

        //Wait for home coming
        time = RandomizeTime((int) atArriveHome, 0, RandomPoisson(lambdaAtArriveHome));
        yield return new WaitUntil(() => WaitForTime(time));
        //Home now, open door
        Controller.instance.InteractDoor();
        time = PassMin(1, time);
        yield return new WaitUntil(() => WaitForTime(time));
        isMoving = true;
        isAtHome = true;
        Controller.instance.InteractDoor();


        //CHOICE [WHAT KIND OF EVENING]
        //STAY AT HOME || SHORTLY OUTSIDE || STAYS OUT FOR THE DAY
        OpenOptionDialogue(new List<string>(){"","Stay home", "Go out shortly", "Stay out for the day"}, "EVENING");
        yield return new WaitUntil(() => CheckForOption());
        CloseOptionDialogue();
        status = "At home";

        switch(chosenOption){
            case 1:
                Debug.Log("Normal Evening");
                yield return StartCoroutine(Normal_Evening(false));
                break;
            case 2:
                Debug.Log("Go outside short");
                yield return StartCoroutine(Normal_Evening(true));
                break;
            case 3:
                Debug.Log("Go outside stay out / come home during wakeup");
                yield return StartCoroutine(StayOut_Evening());
                break;
            default:
                Debug.Log("Something went wrong");
                break;
        }


        //yield return null;
    }
    
    //SUB-SUB CR'S
    IEnumerator Normal_Evening(bool goOut){
        //Randomness
        yield return StartCoroutine(At_Home_Randomness(atSleepTime, goOut, true));

        //time for bed
        status = "Getting ready for bed";
        //Time for bed -> Get ready
        isMoving = true;
        //creatingNoise = Random.Range(10, 30);
        creatingNoise = RandomPoisson(15, 30);

        var time = RandomizeTime((int) atSleepTime, 0, RandomPoisson(lambdaAtSleepTime));
        Debug.Log("Sleeping at: " + TimeToString(time));
        yield return new WaitUntil(() => WaitForTime(time));
        //DONE WITH EVENING
        Debug.Log("Sleeping now");
        status = "Sleeping";
        creatingNoise = 0;
    }

    IEnumerator StayOut_Evening(){
        //leave before bed time but after coming home
        //var timeToLeave = Random.Range(TIME_CONTROL.instance.getHour() + 1, (int) atSleepTime - 1);
        var midHour = TIME_CONTROL.instance.getHour() - atSleepTime;
        midHour = midHour / 2.0f + TIME_CONTROL.instance.getHour();
        var timeToLeave = RandomPoisson(midHour, (int) atSleepTime);

        Debug.Log("Leave at: " + timeToLeave);
        //Wait till go out
        yield return StartCoroutine(At_Home_Randomness(timeToLeave, false, true));
        //Go out
        yield return StartCoroutine(Leave_House());
        status = "Left house";
        stayOut = true;
        //Done
    }

    



    //SMALL CR'S
    IEnumerator Make_Food(){
        yield return null;
    }

    IEnumerator Leave_House(){
        var time = TIME_CONTROL.instance.getTimeNoDay();

        isMoving = true;
        //open door
        OpenDoor();
        time = PassMin(1, time);
        yield return new WaitUntil(() => WaitForTime(time));
        //close door
        CloseDoor();
        isAtHome = false;
    }

    IEnumerator Come_Home(){
        var time = TIME_CONTROL.instance.getTimeNoDay();

        isMoving = true;
        //open door
        OpenDoor();
        time = PassMin(1, time);
        yield return new WaitUntil(() => WaitForTime(time));
        //close door
        CloseDoor();
        isAtHome = true;
        //?
        creatingNoise = Random.Range(0, 25);
    }

    IEnumerator At_Home_Randomness(float till, bool leaveHouseShort, bool evening){
        //Calculate 
        var time = TIME_CONTROL.instance.getTimeNoDay();
        int chunks, intervall;
        (chunks, intervall) = Calculate_Intervall_Chunks(time, (int) till);

        int[] toNow = null;
        //late evening variables
        //float lowMovementPhaseChance = 0;
        bool lowMovementPhase = false;
        int lowMovementPhaseAt = -1;
        //for shopping
        bool wentOut = false;
        int wentOutAt = 0;
        int outForChunks = 0;
        int comeHomeAt = 0;
        //window vars
        bool openedWindowAlready = false;
        bool reasonVentilate = false;
        bool reasonTemperature = false;
        int whenClose = -1;
        float _chanceOfOpeningWindow = chanceOfOpeningWindow;


        //Poisson calculation
        //-------------------
        //Calculate shopping times
        if(leaveHouseShort){
            //in what chunk leave
            wentOutAt = RandomPoisson(chunks/3.0f, chunks); //first 1/3 of the chunks is lambda
            //in what come back
            //Average shopping time: 25min = lambda
            outForChunks = RandomPoisson(25.0f) / intervall; //-> thats how many chunks
            comeHomeAt = outForChunks + wentOutAt;
        }

        //calculate low-movement phase
        if(evening){
            //1 hour before sleep = lambda 
            //lowMovementPhaseAt = RandomPoisson(chunks - (60.0f / intervall)); //chunks - (1 hour in chunk) bsp: 24 - (60/10) = 18 = lambda = chunk
            lowMovementPhaseAt = chunks - RandomPoisson(60.0f) / intervall; //better distribution
            Debug.Log("Low Mov at: " + lowMovementPhaseAt);
        }


        Debug.Log(string.Format("{0} Chunks in {1} intervalls", chunks, intervall));



        for(int i = 0; i < chunks - 1; i++){
            toNow = PassMin(intervall, TIME_CONTROL.instance.getTimeNoDay());
            //Debug.Log(string.Format("Chunk {0} waits till {1}", i, TimeToString(nowPls)));

            yield return new WaitUntil(() => WaitForTime(toNow));

            if(leaveHouseShort && !wentOut){
                if(wentOutAt == i){
                    wentOut = true;
                    yield return StartCoroutine(Leave_House());
                    Debug.Log("Went out for shopping for " + outForChunks + " chunks; equals: " + outForChunks*intervall +" minutes");
                    status = "Went out for shopping";
                }
            }

            if(!isAtHome && comeHomeAt == i){
                yield return StartCoroutine(Come_Home());
                Debug.Log("Arrived home from shopping");
                status = "At home";
            }


            //Move or not
            if(isAtHome){
                if(lowMovementPhase)
                    chanceOfMoving *= 0.96f;

                //Random movement in those chunks
                if(Random.Range(0, 100) < chanceOfMoving)
                    isMoving = true;
                else
                    isMoving = false;

                if(lowMovementPhaseAt == i){
                    lowMovementPhase = true;
                    Debug.Log("Low Movement Phase");
                    status = "At home, chilling";
                }

                //Window calculation
                //------------------
                //
                if(!openedWindowAlready){
                    //too hot -> close when temp is below 19(?)
                    if(SIMULATOR_CONTROL.instance._temperatur > 23.0f){
                        if(Random.Range(0, 100) < _chanceOfOpeningWindow*1.5f){
                            OpenWindow();
                            openedWindowAlready = true;
                            reasonTemperature = true;
                            Debug.Log("Opened window bc too hot");
                        }
                    //ventilating -> calculate minutes here lambda = 60min
                    } else if(Random.Range(0, 100) < _chanceOfOpeningWindow){
                        OpenWindow();
                        openedWindowAlready = true;
                        reasonVentilate = true;
                        whenClose = RandomPoisson(60.0f) / intervall; //-> thats how many chunks
                        Debug.Log("Opened window bc ventilating for " + whenClose*intervall + " minutes");
                        whenClose = i + whenClose; //chunks
                    }
                //Window already open
                } else {
                    if(whenClose == i && reasonVentilate){
                        CloseWindow();
                        Debug.Log("Closed ventilation");
                    }
                    else if(SIMULATOR_CONTROL.instance._temperatur < 17.0f && reasonTemperature){
                        CloseWindow();
                        Debug.Log("Closed bc temperature low");
                    }

                }

                //No reset to high
                /*
                lowMovementPhaseChance = (TIME_CONTROL.instance.getHour() / till) * 100.0f;
                if(Random.Range(0, 99) < lowMovementPhaseChance && evening && !lowMovementPhase){
                    lowMovementPhase = true;
                    Debug.Log("Low Movement Phase");
                    status = "At home, chilling";
                }
                */
                
                creatingNoise = Random.Range(1, 101);
                

            } else
                creatingNoise = 0;
            

        }
       
    }

    //CHOICE FUNC
    bool setValue = false;
    int chosenOption = 0;
    public void SelectedOption(int i){
        this.chosenOption = i;
        setValue = true;
    }

    private bool CheckForOption(){
        return setValue;
    }

    private void OpenOptionDialogue(List<string> options, string status){
        if(_automatic){
            int i = Random.Range(1, options.Count - 1); // 0 is empty option!
            SelectedOption(i);
            Debug.Log("Chosen automatically: " +  i);
            return; 
        }

        Debug.Log("Stopped Time");
        TIME_CONTROL.instance.StopTime();
        dropdownChoices.gameObject.SetActive(true);
        choiceText.enabled = true;

        choiceText.text = status;
        dropdownChoices.AddOptions(options);
    }

    private void CloseOptionDialogue(){
        Debug.Log("Resumed Time");
        TIME_CONTROL.instance.ResumeTime();
        dropdownChoices.gameObject.SetActive(false);
        dropdownChoices.ClearOptions();
        choiceText.enabled = false;
        setValue = false; //resets the option-menu
    }



    //HELPFUNC
    private void OpenDoor(){
        Controller.instance.InteractDoor();
    }
    private void CloseDoor(){
        Controller.instance.InteractDoor();
    }

    private void OpenWindow(){
        Controller.instance.OpenWindow();
        
    }

    private void CloseWindow(){
        if(!willForgetWindow)
            Controller.instance.CloseWindow();
        else{
            Debug.Log("Forgot Window!");
            willForgetWindow = false;
        }
    }

    private void ResetValues(){
        atWakeUp = wakeUpReset;
        atTimeForWork = timeForWorkReset;
        atDoneWork = doneWorkReset;
        atArriveHome = arriveHomeReset;
        atSleepTime = sleepTimeReset;
        genericOffsetMin = offsetMinReset;
        genericOffsetMax = offsetMaxReset;
        chanceOfMoving = movingReset;
        chanceOfOpeningWindow = windowReset;
    }
    private (int, int) Calculate_Intervall_Chunks(int[] fromTime, int till){
        //how many hours till till?
        int timeLeftH = till - fromTime[0];
        int timeLeftM = 0;
        if(timeLeftH == 0) timeLeftM = 60 - TIME_CONTROL.instance.getMinute();
        if(timeLeftH < 0) {Debug.LogError("Hours is < 0 !!"); Debug.Break(); }
        //calculate random intervall intervals of moving
        var chunks = (int) Random.Range(5, Mathf.Max(5, (timeLeftH*60 + timeLeftM) * 0.25f));
        var intervall = (timeLeftH*60 + timeLeftM) / chunks;

        if(chunks <= 0 || intervall <= 0) {Debug.LogError(string.Format("Intervall or chunk is 0!\nIntervall: {0} | Chunk: {1}", intervall, chunks)); Debug.Break(); }

        return (chunks, intervall);
    }

    private float RandomGeneric(){
        return Random.Range(genericOffsetMin, genericOffsetMax);
    }

    private int RandomPoisson(float lambda){
        return PoissonRandomizer.GetPoisson(lambda);
    }

    private int RandomPoisson(float lambda, int max){
        return PoissonRandomizer.GetPoissonWMax(lambda, max);
    }

    private bool WaitForTime(int[] time){

        return TIME_CONTROL.instance.getHour() == time[0] && TIME_CONTROL.instance.getMinute() >= time[1] && (int) TIME_CONTROL.instance.getSeconds() >= time[2];
    }

    private int[] RandomizeTime(int hour, int minute, float offset){
        
        if(minute + offset >= 60){
            var min = (minute + offset) % 60;
            int hourX = (int) Mathf.Floor((minute + offset) / 60.0f);
            hour %= 24;

            hour = hourX;
            minute = (int) min;
        } else {
            minute += (int) offset;
        }

        //random sec range
        int sec = Random.Range(0,59);

        return new int[]{hour, minute, sec};
    }

    private int[] PassMin(int amountMin, int[] now){
        var _hour = now[0];
        var _minute = now[1] +  amountMin;
        

        if(_minute >= 60){
            _hour++;
            _minute %= 60;
            _hour %= 24;
        }

        now[2] = Random.Range(0,59);
        return new int[]{_hour, _minute, now[2]};
    }

    private string TimeToString(int[] x){
        return string.Format("{0:00}:{1:00}:{2:00}", x[0], x[1], x[2]);
    }


}
