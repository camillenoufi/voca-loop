using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dotControl : MonoBehaviour {

    float loopTime;
    float gameWidth = main.xR - main.xL;
    float x, dx, y, yStart, yHeight, z;
    bool triInc;
    int boolean;
    Color thisColor = new Color();

    private TrailRenderer tr;

    void Start()
    {
        InitializeComponents();
        InitializePositionVars();
        boolean = Mathf.RoundToInt(Random.Range(1, 10)) % 2;
    }

    void InitializeComponents()
    {
        tr = GetComponent<TrailRenderer>();
        tr.time = main.currentMeter * 60 / main.currentTempo;
        tr.material = new Material(Shader.Find("Sprites/Default"));
        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        thisColor = gameObject.GetComponent<SpriteRenderer>().color;
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(thisColor, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
            );
        tr.colorGradient = gradient;
    }

    void InitializePositionVars() 
    {
        yStart = GetComponent<Transform>().position.y;
        yHeight = Random.Range(1f, 5f);
        y = yStart;
        x = GetComponent<Transform>().position.x;
        triInc = true;
    }

     
	void Update()
	{
        loopTime = main.currentMeter * 60 / main.currentTempo;
        GetComponent<TrailRenderer>().time = loopTime;
        UpdatePosition(loopTime);
        
	}

    void UpdatePosition(float loopTime)
    {
        //X
        dx = ((gameWidth / loopTime) * Time.deltaTime);
        x += dx;
        if (x > main.xR)
        {
            x = main.xL;
            GetComponent<TrailRenderer>().enabled = false;
        }
        else
            GetComponent<TrailRenderer>().enabled = true;


        //Y
        y = GetComponent<Transform>().position.y;
        if (tag == "saw")
        {
            if (y < (yStart + yHeight))
                y += IncrementSaw(dx);
            else
                y = yStart;
        }
        if (tag == "sine")
        {
            y += IncrementSine(x);
        }
        if (tag == "tri")
        {
            if (y >= (yStart + yHeight))
                triInc = false;
            if (y <= yStart)
                triInc = true;

            if (triInc)
                y += IncrementTri(dx);
            else
                y -= IncrementTri(dx);

        }

        //Z
        z += Random.Range(-0.01f, 0.01f);
        if (z >= 8f)
            z = -8f;
        else if (z <= -8)
            z = 8f;

        //update
        Vector3 posUpdate = new Vector3(x, y, z);
        GetComponent<Transform>().position = posUpdate;
    }

    float IncrementSaw(float x)
    {
        float m = 0.4f;
        return m*x;
    }

    float IncrementSine(float x)
    {
        //float T = 5; //period in number of width units
        float A = 1;//Random.Range(-1f,1f);
        float angle = Mathf.PI*x;
        if (boolean==1) 
            return A * Mathf.Sin(angle);
        else 
            return A * Mathf.Cos(angle);
        
        
    }

    float IncrementTri(float x)
    {
        float m = 0.5f;
        return m * x;
    }
    

}
