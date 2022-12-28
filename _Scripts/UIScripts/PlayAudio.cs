using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudio : MonoBehaviour {

    public AudioSource Whoosh;
    public AudioSource Whistle;
    public bool LastSecond;

	
    void Update()
    {
        if (LastSecond)
        {
            Whistle.enabled = true;
            Whistle.Play();
        }
    }
	void OnEnabled()
    {
        if(!LastSecond)
            Whoosh.Play();

        else 
        {
            LastSecond = false;
        }

    }
}
