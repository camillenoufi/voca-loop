using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objSelect : MonoBehaviour {


	public Transform chuckSound;
    
    // Use this for initialization
	void Start () {
        SetHaloRender(false);
	}
	
	// Update is called once per frame
    void Update() 
    {

    }

	void OnMouseDown() 
	{
		string thisObject = GetComponent<SpriteRenderer>().tag;
        SetHaloRender(true);
        if(thisObject == "piano" || thisObject == "guitar" || thisObject == "violin" || thisObject == "drum")
        {
            main.currentInstrument = thisObject;
        }
        if (thisObject == "stop")
        {
            Debug.Log("destroying");
            main.destroyLoop = true;
        }
        if (main.currentInstrument == "drum")
        {
            Instantiate(chuckSound, gameObject.transform.position, gameObject.transform.rotation);
        }
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

    public void SetExternalHaloRender(GameObject gameObject, bool state)
    {
        Component halo = gameObject.GetComponent("Halo");
        halo.GetType().GetProperty("enabled").SetValue(halo, state, null);
        if (state)
            Debug.Log("in halo set");
    }



}
