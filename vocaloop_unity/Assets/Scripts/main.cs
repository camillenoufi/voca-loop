using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main : MonoBehaviour {

    // PUBLIC EDITABLE VARIABLES (in unity editor)


    //PUBLIC SCRIPTS
	public objSelect obj;

    // PUBLIC VARIABLES FOR OTHER CLASSES TO ACCESS
    public static string currentInstrument = "";
    public static float currentTempo = 80.0f;

    public static bool pianoFlag = false;
    public static bool guitarFlag = false;
    public static bool violinFlag = false;
    public static bool destroyLoop = false;
    

    //PRIVATE VARIABLES
	

    // Chuck stuff
    private ChuckSubInstance myChuckTempo;
    private ChuckFloatSyncer myTempoSyncer;



    // Use this for initialization
    void Start () 
	{
		SetUpChuck();
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
		if(pianoFlag) 
		{
			GameObject piano = GameObject.FindGameObjectWithTag("piano");
			obj.SetExternalHaloRender(piano,true);
			
		}
		else if (!pianoFlag)
		{
            GameObject piano = GameObject.FindGameObjectWithTag("piano");
            obj.SetExternalHaloRender(piano, false);
		}

        if (guitarFlag)
        {
            GameObject guitar = GameObject.FindGameObjectWithTag("guitar");
            obj.SetExternalHaloRender(guitar, true);

        }
        else if (!guitarFlag)
        {
            GameObject guitar = GameObject.FindGameObjectWithTag("guitar");
            obj.SetExternalHaloRender(guitar, false);
        }

        if (violinFlag)
        {
            GameObject guitar = GameObject.FindGameObjectWithTag("guitar");
            obj.SetExternalHaloRender(guitar, true);

        }
        else if (!violinFlag)
        {
            GameObject violin = GameObject.FindGameObjectWithTag("violin");
            obj.SetExternalHaloRender(violin, false);
        }


	}


    // ChucK pitch tracking script contained here
    void StartChuckMetronome(ChuckSubInstance myChuckTempo)
    {
        // instantiate Chuck Pitch Tracking code
        myChuckTempo.RunCode(@"

			60 => global float BEATS_PER_MIN;

			
			
			
		");
    }
}
