using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class main : MonoBehaviour {

    // PUBLIC EDITABLE VARIABLES (in unity editor)
    public Transform chuckSound, kickLight, snareLight;
    public Text bpm; 

    //PUBLIC SCRIPTS


    // PUBLIC VARIABLES FOR OTHER CLASSES TO ACCESS
    public static string currentInstrument = "";
    public static float currentTempo = 80.0f;
    public static int currentBeat = 0, currentMeter = 8;
    public static bool beatFlag = false, adcFlag = false, countdownFlag = false, recordFlag = false;
    public static bool sineFlag = false, sawFlag = false, triFlag = false, kickFlag = false, snareFlag = false;
    public static Dictionary<string,int> instrumentDict = new Dictionary<string, int>();
    public static float xL = -50f;
    public static float xR = 50f;
    public static float yT = 20f;
    public static float yB = -20f;


    // Use this for initialization
    void Start () 
	{
        //create instrument list
        instrumentDict.Add("kick",1);
        instrumentDict.Add("snare",2);
        instrumentDict.Add("saw",3);
        instrumentDict.Add("sine",4);
        instrumentDict.Add("tri",5);
        
        Instantiate(chuckSound, gameObject.transform.position, gameObject.transform.rotation);
	}


	void Update()
	{
        bpm.text = currentTempo.ToString() + " BPM";

        if (kickFlag) 
        {
            InstantiateDrumObject(kickLight);
            kickFlag = false;
        }

        if (snareFlag)
        {
            InstantiateDrumObject(snareLight);
            snareFlag = false;
        }

	}

    void InstantiateDrumObject(Transform drumPrefab)
    {
        Debug.Log("instantiating drums");
        float xpos = Random.Range(xL, xR);
        float ypos = Random.Range(yB, yT);
        Vector3 position = new Vector3(xpos, ypos, 1);
        Instantiate(drumPrefab, position, Quaternion.identity);
    }
    
}
