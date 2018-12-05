using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dotControl : MonoBehaviour {

    void Start()
    {
        GetComponent<SpriteRenderer>().color = (GameObject.Find(main.currentInstrument)).GetComponent<SpriteRenderer>().material.color; ;
        GetComponent<SpriteRenderer>().tag = (GameObject.Find(main.currentInstrument)).tag;
    }

	void Update()
	{
		
	}
}
