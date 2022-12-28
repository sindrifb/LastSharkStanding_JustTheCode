using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorHook : Hook
{
    //public GameObject HitParticleEffect;

    bool throwing;

    public override void UpdateThrowing()
    {
        if (TRS != null && TRS.DistanceRatio >= .98f)
        {
            OnFinishedThrow();
        }
    }

   
    protected override void IndependentUpdate()
    {
        if (throwing)
        {
            UpdateThrowing();
        }
    }

    protected override void OnTriggerEnter(Collider col)
    {
        if (col.isTrigger)
        {
            return;
        }

        var hookedObject = CheckForHit(col.gameObject);
        
        if (hookedObject != null && col.gameObject != Owner && hookedObject.IsAvailable)
        {
            Vector3 dir = (col.transform.position - transform.position);
            dir.y = 0;
            dir.Normalize();
            if (Vector3.Dot(dir, Vector3.up) < 0 && Vector3.Dot(dir,-transform.up) < 0)
            {
                dir = -transform.up;
            }

            if (HitParticleSystem != null)
            {
                var ps = Instantiate(HitParticleSystem, col.ClosestPoint(transform.position), Quaternion.LookRotation(dir));
                Destroy(ps, 3f);
            }
            
            if (hookedObject is PlayerHookable)
            {
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.HookSound.AnchorHit);
                hookedObject.Push((-transform.up + (Vector3.up * 2) + dir).normalized, 65f);
            }
            else
            {
                hookedObject.Push((-transform.up + (Vector3.up * 2) + dir).normalized, 45f);
            }
        }

        if (col.GetComponent<AnchorHook>() != null && col.GetComponent<AnchorHook>().Owner == null)
        {
            col.GetComponent<AnchorHook>().Break();
            Break();
        }
    }

    public void Break()
    {
        OnFinishedThrow();
    }

    public override void OnFinishedThrow()
    {
        //OnReelingFinished();
        //HookController.SetActiveHook(HookController.StandardHook);
        var ps = Instantiate(GameManager.Instance.DestroyParticleEffect, transform.position, Quaternion.identity);
        Destroy(ps, 3f);
        Destroy(gameObject);
    }

    public override void InitializeThrow()
    {
        //Throw ();
        if (ParentConstraint != null)
        {
            ParentConstraint.constraintActive = false;
        }
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HookSound.HookThrow);
        GameObject GO = Instantiate(gameObject,transform.position, transform.rotation);
        GO.transform.localScale = new Vector3(3f, 3f, 3f);
        AnchorHook hook = GO.GetComponent<AnchorHook>();
        hook.Curve = Curve;
        hook.TRS = TRS;
        Curve = null;
        TRS = null;
        hook.Throw();
        HookController?.PlayHookAnimation(Constants.AnimationParameters.Throw);
        StartCoroutine(LateReset());
    }

    protected override void Throw()
    {
        base.Throw();
        throwing = true;
    }

    private IEnumerator LateReset()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        ResetHook();
        OnReelingFinished();
        HookController.SetActiveHook(HookController.StandardHook);
    }

    public override void SetHookRenderAndColliderActive(bool pValue, bool pOnlyCollider = false)
    {
        // anchor is never invisible so only collider
        GetComponent<SphereCollider>().enabled = pValue;
    }
}
