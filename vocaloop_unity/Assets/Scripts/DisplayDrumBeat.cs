using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayDrumBeat : MonoBehaviour {

    void Start()
    {
        SetHaloRender(true);
        StartCoroutine(DelayDestroyObject(main.currentTempo));
    }


    void SetHaloRender(bool state)
    {
        Component halo = gameObject.GetComponent("Halo");
        halo.GetType().GetProperty("enabled").SetValue(halo, state, null);
    }

    private IEnumerator DelayDestroyObject(float tempo)
    {
        yield return new WaitForSeconds(0.5f * 60 / tempo);
        Destroy(gameObject);
    }
}
