using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class adcSelect : MonoBehaviour
{
    public Text beatText;
    public Transform loopDot;

    private IEnumerator coroutine;

    // Use this for initialization
    void Start()
    {
        SetHaloRender(false);
        beatText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (main.beatFlag)
        {
            DisplayBeat();
        }
    }

    void DisplayBeat()
    {
        beatText.text = main.currentBeat.ToString();
        beatText.color = (GameObject.Find(main.currentInstrument)).GetComponent<SpriteRenderer>().material.color;
        StartCoroutine(BeatOffDelay(main.currentTempo));
        main.beatFlag = false;
    }

    private IEnumerator BeatOffDelay(float tempo)
    {
        yield return new WaitForSeconds(0.5f*60/tempo);
        beatText.text = "";
    }

    void OnMouseDown()
    {
        string thisObject = GetComponent<SpriteRenderer>().tag;
        SetHaloRender(true);
        if (thisObject == "adc")
        {
            main.adcFlag = true;
            Debug.Log("adc selected");

            if(main.currentInstrument == "saw" || main.currentInstrument == "sine" || main.currentInstrument == "tri") 
            {
                Vector3 position = new Vector3(main.xL, Random.Range(main.yB+10, main.yT), -1);
                Instantiate(loopDot, position, Quaternion.identity);
            }
            else //drums
            {
                Debug.Log("adc for drums");
            }
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
    }

    public void SetExternalHaloRender(GameObject gameObject, bool state)
    {
        Component halo = gameObject.GetComponent("Halo");
        halo.GetType().GetProperty("enabled").SetValue(halo, state, null);
    }
}

