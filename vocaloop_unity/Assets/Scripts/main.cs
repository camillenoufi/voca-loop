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

    public static bool sineFlag = false;
    public static bool sawFlag = false;
    public static bool triFlag = false;
    public static bool adcFlag = false;
    

    //PRIVATE VARIABLES
	

    // Chuck stuff
    private ChuckSubInstance myChuckTempo;
    private ChuckFloatSyncer myTempoSyncer;



    // Use this for initialization
    void Start () 
	{
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
