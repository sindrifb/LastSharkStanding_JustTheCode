using System.Collections;
using System.Collections.Generic;
using BansheeGz.BGSpline.Curve;
using UnityEngine;

public class PowerUpHookable : Hookable
{
    //private SphereCollider Collider;

    //public override void Initialize()
    //{
    //    base.Initialize();
    //    //Collider = GetComponent<SphereCollider>();
    //}
    //public override void Activate(Hook pHook, BGCurve pCurve = null)
    //{
    //    base.Activate(pHook, pCurve);
    //    //Collider.enabled = false;
    //}
    //public override void Initialize()
    //{
    //    //AttachedUsable = transform.GetComponentInChildren<Usable>();
    //    //PowerupUsable = transform.GetComponentInChildren<Usable>();
    //}

    //public override void OnFinishedBeingHooked()
    //{
    //    base.OnFinishedBeingHooked();
    //    //Collider.enabled = true;
    //}

    //private IEnumerator PickUpPowerUp()
    //{
    //    while (transform.position != PowerupUsable.Owner.transform.position)
    //    {
    //        transform.position = Vector3.Lerp(transform.position, PowerupUsable.Owner.transform.position, 10f);
    //        yield return null;
    //    }
    //}
    public override void OnHooked()
    {
        base.OnHooked();
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.PlayerSound.GetHit);
    }
    public override void Push(Vector3 pDir, float pForce)
    {
        
    }

    public override void Push(Vector3 pVelocity)
    {
        
    }
}
