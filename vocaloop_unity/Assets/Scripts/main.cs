using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main : MonoBehaviour {

    // PUBLIC EDITABLE VARIABLES (in unity editor)
    public Transform chuckSound;

    //PUBLIC SCRIPTS


    // PUBLIC VARIABLES FOR OTHER CLASSES TO ACCESS
    public static string currentInstrument = "";
    public static float currentTempo = 80.0f;
    public static int currentBeat = 0, currentMeter = 8;
    public static bool beatFlag = false, adcFlag = false;

    public static bool sineFlag = false, sawFlag = false, triFlag = false;
    public static Dictionary<string,int> instrumentDict = new Dictionary<string, int>();
    public static float xL = -50f;
    public static float xR = 50f;
    public static float yT = 25f;
    public static float yB = -25f;

    //PRIVATE VARIABLES
	

    // Chuck stuff
    private ChuckSubInstance myChuckTempo;
    private ChuckFloatSyncer myTempoSyncer;



    // Use this for initialization
    void Start () 
	{
        //create instrument list
        instrumentDict.Add("kick",1);
        instrumentDict.Add("snare",2);
        instrumentDict.Add("saw",3);
        instrumentDict.Add("sine",4);
        instrumentDict.Add("tri",5);
        
        SetUpChuck();
        Instantiate(chuckSound, gameObject.transform.position, gameObject.transform.rotation);
	}
	
	void SetUpChuck()
	{
		myChuckTempo = GetComponent<ChuckSubInstance>();
        myTempoSyncer = gameObject.AddComponent<ChuckFloatSyncer>();
        myTempoSyncer.SyncFloat(myChuckTempo, "BEATS_PER_MIN"); //current instance of chuck is determining pos value
		StartChuckMetronome(myChuckTempo);
	}

	void Update()
	{
        myTempoSyncer.SetNewValue(main.currentTempo);

	}


    // ChucK pitch tracking script contained here
    void StartChuckMetronome(ChuckSubInstance myChuckTempo)
    {
        // instantiate Chuck Pitch Tracking code
        myChuckTempo.RunCode(@"

			80 => global float BEATS_PER_MIN;
			
		");
    }
}
