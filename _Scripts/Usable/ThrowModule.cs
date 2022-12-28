using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public abstract class ThrowModule : MonoBehaviour 
{
    protected Coroutine ThrowCoroutine;
    [SerializeField]
    protected float ThrowDelay;
    [SerializeField]
    protected GameObject ThrowParticleSystemPrefab;
    [SerializeField]
    protected GameObject ContinousThrowParticlePrefab;
    protected Usable Usable;
    protected PlayerController OwnerPlayerController;
    [SerializeField]
    protected float ScaleOnThrow = 1;
    protected Vector3 StartLocalScale;

    protected virtual void IndependentUpdate() { }

    public virtual void Initialize(Usable pUsable)
    {
        Usable = pUsable;
        OwnerPlayerController = pUsable.Owner.GetComponent<PlayerController>();
        StartLocalScale = transform.localScale;
    }

    private void Update()
    {
        IndependentUpdate();
    }

    public virtual void Throw()
    {
        ThrowCoroutine = StartCoroutine(ThrowWithDelay());
    }

    protected IEnumerator ThrowWithDelay()
    {
        yield return new WaitForSeconds(ThrowDelay);

        if (Usable.PlayerController.CurrentState != PlayerController.State.Throwing)
        {
            yield break;
        }

        if (Usable.ParentConstraint != null)
        {
            Usable.ParentConstraint.constraintActive = false;
        }

        if (ThrowParticleSystemPrefab != null && Usable.Owner != null)
        {
            var ps = Instantiate(ThrowParticleSystemPrefab, Usable.Owner.transform.position, Usable.Owner.transform.rotation);
            Destroy(ps, 3f);
        }

        if (ContinousThrowParticlePrefab != null && Usable.Owner != null)
        {
            var ps = Instantiate(ContinousThrowParticlePrefab, Usable.Owner.transform.position, Usable.Owner.transform.rotation, Usable.transform);
        }
        //pUsable.OnThrow();

        transform.localScale = StartLocalScale * ScaleOnThrow;

        OverridableThrow();
        Usable.EffectModule.Thrown = true;

        //Usable.UsableController.PlayUsableAnimation(Constants.AnimationParameters.Throw);
        //Usable.PlayerController.PlayAnimation(Constants.AnimationParameters.Throw);

        //pUsable.PlayerController.OnThrowingDone();
    }

    protected virtual void OverridableThrow()
    {
        Usable.SetUsableRenderAndColliderActive(true);
    }

    public virtual void ThrowDone()
    {
        Usable.PlayerController.OnThrowingDone();
        transform.localScale = StartLocalScale;
    }

    public virtual void Interrupt() { }
}
