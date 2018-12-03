using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objSelect : MonoBehaviour {
    
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
        main.currentInstrument = thisObject;
        Debug.Log(main.currentInstrument);
    }

	void OnMouseUp() 
	{
        SetHaloRender(false);

    }

	void SetHaloRender(bool state) 
	{
        Component halo = gameObject.GetComponent("Halo");
        halo.GetType().GetProperty("enabled").SetValue(halo, state, null);
        if (state) {}
	}

    public void SetExternalHaloRender(GameObject gameObject, bool state)
    {
        Component halo = gameObject.GetComponent("Halo");
        halo.GetType().GetProperty("enabled").SetValue(halo, state, null);
        if (state) {}
    }



}
