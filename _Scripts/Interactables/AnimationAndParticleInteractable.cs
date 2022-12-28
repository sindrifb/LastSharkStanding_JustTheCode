using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationAndParticleInteractable : Interactable
{
    public Animator Animator;
    public ParticleSystem ParticleSystem;
    bool ready = false;

    private void Start()
    {
        StartCoroutine(LateSetReady());
    }

    protected override void OnGeneralActivate()
    {
        if (ready)
        {
            ready = false;
            if (ParticleSystem)
            {
                ParticleSystem.Play();
            }

            Animator.SetTrigger("play");
            StartCoroutine(LateSetReady());
        }   
    }

    IEnumerator LateSetReady()
    {
        yield return new WaitForSeconds(1f);
        ready = true;
    }
}
