using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectSelect : MonoBehaviour {


	public Transform chuckSound;
    
    // Use this for initialization
	void Start () {
        SetHaloRender(false);
	}
	
	// Update is called once per frame

	void OnMouseDown() 
	{
		string thisObject = GetComponent<SpriteRenderer>().tag;
        SetHaloRender(true);
        if(thisObject == "piano" || thisObject == "guitar" || thisObject == "violin")
        {
            main.currentInstrument = thisObject;
        }
        else if(thisObject == "stop")
        {
            Destroy(chuckSound);
        }

        if (main.currentInstrument == "piano")
        {
            Instantiate(chuckSound, gameObject.transform.position, gameObject.transform.rotation);
        }
        thisObject = "";
    }

	void OnMouseUp() 
	{
        SetHaloRender(false);

    }

	void SetHaloRender(bool state) 
	{
        Component halo = gameObject.GetComponent("Halo");
        halo.GetType().GetProperty("enabled").SetValue(halo, state, null);
        if (state)
            Debug.Log("in halo set");
	}
}
