using System.Collections;
using System.Collections.Generic;
using BansheeGz.BGSpline.Curve;
using UnityEngine;

public class PlayerHookable : Hookable 
{
    public GameObject StandUpParticleEffect;
    public PlayerController PlayerController { get; private set; }
    private UsableController UsableController;
    public Transform HookedHookPosition;

    public override void Initialize()
    {
        base.Initialize();
        PlayerController = GetComponentInChildren<PlayerController>();
        UsableController = GetComponentInChildren<UsableController>();
    }

    //public override void Activate(Usable pUsable, BGCurve pCurve = null)
    //{
    //    IsAvailable = PlayerController.IsAvailable;
    //    base.Activate(pUsable, pCurve);
    //    if (IsAvailable)
    //    {
    //        PlayerController.OnBeingHit();
    //        //***** not sure why this is needed****
    //        //Usable.transform.SetParent(HookedHookPosition);
    //    }
    //}

    public override void Push(Vector3 pDir, float pForce)
    {
        PlayerController.Push(pDir, pForce);
        base.Push(pDir, pForce);

        //PlayerController.ChangeState(PlayerController.State.none);
        //StartCoroutine(PlayerController.LateSet(PlayerController.State.Ragdoll, 0.1f));
    }

    public override void Push(Vector3 pVelocity)
    {
        PlayerController.Push(pVelocity);
        base.Push(pVelocity);

        //PlayerController.ChangeState(PlayerController.State.none);
        //StartCoroutine(PlayerController.LateSet(PlayerController.State.Ragdoll, 0.1f));
    }

    //public override void OnFinishedBeingHooked()
    //{
    //    base.OnFinishedBeingHooked();
    //    PlayerController.OnHookableReelingDone();
    //}

    public override void OnFinishedRagdolling()
    {
        PlayerController.OnRagdollDone();
        if (StandUpParticleEffect != null)
        {
            var ps = Instantiate(StandUpParticleEffect, transform.position, Quaternion.identity);
            Destroy(ps, 3f);
        }
    }
}
