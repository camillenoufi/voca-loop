using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class toolSelect : MonoBehaviour {

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
		if (thisObject == "adc") 
		{
			main.adcFlag = true;
			Debug.Log(main.currentInstrument);
			Debug.Log("adc selected");
		}
        if (thisObject == "stop")
        {
            Debug.Log("destroying");
            Destroy(GameObject.FindGameObjectWithTag("chuckSound"));
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
