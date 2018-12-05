using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dotControl : MonoBehaviour {

    float loopTime;
    float gameWidth = main.xR - main.xL;
    float x = 0f;
    float y = 0f;

    float yStart;
    float yHeight;

    void Start()
    {
        GetComponent<SpriteRenderer>().color = (GameObject.Find(main.currentInstrument)).GetComponent<SpriteRenderer>().material.color; ;
        GetComponent<SpriteRenderer>().tag = (GameObject.Find(main.currentInstrument)).tag;
        GetComponent<TrailRenderer>().time = main.currentMeter*60/main.currentTempo;
        yStart = GetComponent<Transform>().position.y;
        Debug.Log(yStart);
        Debug.Log(GetComponent<Transform>().position.x);
        yHeight = Random.Range(3f, 10f);
    }

    /* 
	void Update()
	{
        loopTime = main.currentMeter * 60 / main.currentTempo;
        Debug.Log(loopTime);
        GetComponent<TrailRenderer>().time = loopTime;
        
        x += GetComponent<Transform>().position.x + .001f; //(gameWidth / loopTime) * Time.deltaTime;

        y = GetComponent<Transform>().position.y;
        if(main.currentInstrument == "saw") 
        {
            if (y < (yStart + yHeight))
                y += IncrementSaw(x);
            else
                y = yStart;
        }
        else if (main.currentInstrument == "sine")
        {
            y += IncrementSine(x);
        }
        else if (main.currentInstrument == "tri")
        {
            if(y < (yStart + yHeight) )
                y += IncrementTri(x);
            else
                y -= IncrementTri(x);
        }

        Vector3 posUpdate = new Vector3(x, y, 1.0f);
        GetComponent<Transform>().position = posUpdate;
	}

    float IncrementSaw(float x)
    {
        float m = 0.5f;
        return m*x;
    }

    float IncrementSine(float x)
    {
        float T = 5; //period in number of width units
        float A = T;
        float angle = Mathf.PI * x / (T/2);
        return A*Mathf.Sin(angle);
    }

    float IncrementTri(float x)
    {
        float m = 1f;
        return m * x;
    }
    */

}
