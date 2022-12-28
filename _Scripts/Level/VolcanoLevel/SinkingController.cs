using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkingController : MonoBehaviour 
{
    private Animator Animator;
    [SerializeField]
    private GameObject SinkParticlesParent;
    private GameObject ColliderObject;
    private Coroutine SinkCoroutine;
    [SerializeField]
    private GameObject NavMeshSurface;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        ColliderObject = GetComponentInChildren<Collider>().gameObject;
        NavMeshSurfaceEnabled(true);
    }

    private void NavMeshSurfaceEnabled(bool pValue)
    {
        NavMeshSurface.SetActive(pValue);
    }

    public void Sink()
    {
        Animator?.SetBool(Constants.AnimationParameters.PlatformShake, true);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.PlatformSink);
        NavMeshSurfaceEnabled(false);
        SinkCoroutine = StartCoroutine(DelayedSink(2));
    }

    private IEnumerator DelayedSink(float pDelay)
    {
        yield return new WaitForSeconds(pDelay);

        PlayParticles(SinkParticlesParent);
        Animator?.SetTrigger(Constants.AnimationParameters.PlatformSink);

        yield return new WaitForSeconds(2f);

        Animator?.SetBool(Constants.AnimationParameters.PlatformShake, false);
        ColliderObject.SetActive(false);
    }

    private void PlayParticles(GameObject pParent)
    {
        var systems = pParent.GetComponentsInChildren<ParticleSystem>();
        foreach (var system in systems)
        {
            system.Play();
        }
    }

    public void ResetPlatform()
    {
        StopCoroutine(SinkCoroutine);
        Animator.SetBool(Constants.AnimationParameters.PlatformShake, false);
        if (!Animator.GetCurrentAnimatorStateInfo(1).IsName("Idle"))
        {
            Animator.SetTrigger(Constants.AnimationParameters.PlatformReset);
        }
        ColliderObject.SetActive(true);
        NavMeshSurfaceEnabled(true);
    }

    public Vector3 ChildPos()
    {
        return ColliderObject.transform.position;
    }
}
