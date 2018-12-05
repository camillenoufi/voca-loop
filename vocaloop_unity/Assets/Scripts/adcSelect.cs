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
            main.currentBeat = main.currentBeat % main.currentMeter + 1;
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
        SetHaloRender(true);
        main.adcFlag = true;
        Debug.Log("adc selected");
        if(main.currentInstrument == "saw" || main.currentInstrument == "sine" || main.currentInstrument == "tri") 
        {
            StartCoroutine(SyncLoopWithMetronome());
        }
        else //drums
        {
            Debug.Log("adc for drums");
        }
    }

    private IEnumerator SyncLoopWithMetronome()
    {
        yield return new WaitUntil(() => ChuckSoundController.globalBeatFlag == true);
        InstantiateLoopDot(main.currentInstrument);

    }

    void InstantiateLoopDot(string objectTag)
    {
        Vector3 position = new Vector3(main.xL, Random.Range(main.yB + 10, main.yT - 5), 1);
        loopDot.tag = objectTag;
        loopDot.GetComponent<SpriteRenderer>().color = (GameObject.Find(main.currentInstrument)).GetComponent<SpriteRenderer>().color;
        Instantiate(loopDot, position, Quaternion.identity);
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

