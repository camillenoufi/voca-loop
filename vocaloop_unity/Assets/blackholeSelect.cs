using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blackholeSelect : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        SetHaloRender(false);
	}

    void OnMouseDown()
    {
        Debug.Log("removing specific sound");
        GameObject[] thisSoundType = GameObject.FindGameObjectsWithTag(main.currentInstrument);
        if (thisSoundType != null && thisSoundType.Length != 0)
			DestroyInstrumentVisuals(thisSoundType);
    }

    void DestroyInstrumentVisuals(GameObject[] obj)
    {
        for (int i = 0; i < obj.Length; i++)
        {
            if(obj[i].name != main.currentInstrument)
				Destroy(obj[i]);
        }
    }

    void SetHaloRender(bool state)
    {
        Component halo = gameObject.GetComponent("Halo");
        halo.GetType().GetProperty("enabled").SetValue(halo, state, null);
    }
}
