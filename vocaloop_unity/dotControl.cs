using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dotControl : MonoBehaviour {

    
   public Transform chuckSound; 
   private IEnumerator coroutine;
    
	
	// Use this for initialization
	void Start () 
	{
		GetComponent<SpriteRenderer>().color = paintGM.currentColor;
        GetComponent<SpriteRenderer>().tag = paintGM.currentTag;
        GetComponent<Transform>().localScale = new Vector2(paintGM.currentScale,paintGM.currentScale);

        SetHaloRender(false);
	}

	void OnMouseOver()
	{
		if(paintGM.toolType == "eraser")
			Destroy(gameObject);
	}

    void OnTriggerEnter(Collider other) //turn on halo
    {	
		if (other.gameObject.CompareTag("Player"))
        {
            chuckSound.tag = gameObject.GetComponent<SpriteRenderer>().tag;
            Instantiate(chuckSound, gameObject.transform.position, gameObject.transform.rotation);
            StartCoroutine(HaloOnOff(3.0f));
        }
    }

    private IEnumerator HaloOnOff(float waitTime)
    {
        SetHaloRender(true);
		yield return new WaitForSeconds(waitTime);
        SetHaloRender(false);
    }

    void SetHaloRender(bool state)
    {
        Component halo = gameObject.GetComponent("Halo");
        halo.GetType().GetProperty("enabled").SetValue(halo, state, null);
    }
}
