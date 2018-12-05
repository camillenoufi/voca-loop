using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stopSelect : MonoBehaviour {

	// Use this for initialization
	void Start () {
        SetHaloRender(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnMouseDown()
    {
        Debug.Log("destroying");
        Destroy(GameObject.FindGameObjectWithTag("chuckSound"));
        GameObject[] sine = GameObject.FindGameObjectsWithTag("sine");
		DestroyInstrumentVisuals(sine);
        GameObject[] saw = GameObject.FindGameObjectsWithTag("saw");
        DestroyInstrumentVisuals(saw);
        GameObject[] tri = GameObject.FindGameObjectsWithTag("tri");
        DestroyInstrumentVisuals(tri);

    }

	void DestroyInstrumentVisuals(GameObject[] obj)
	{
        for (int i = 0; i < obj.Length; i++)
        {
            Destroy(obj[i]);
        }
	}

    void SetHaloRender(bool state)
    {
        Component halo = gameObject.GetComponent("Halo");
        halo.GetType().GetProperty("enabled").SetValue(halo, state, null);
    }

	
}
