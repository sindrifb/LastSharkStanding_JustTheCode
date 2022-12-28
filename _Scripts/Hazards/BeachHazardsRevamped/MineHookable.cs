using BansheeGz.BGSpline.Curve;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineHookable : Hookable
{
    //public override void Activate(Hook pHook, BGCurve pCurve = null)
    //{
    //    base.Activate(pHook, pCurve);
    //    GetComponent<MineHit>().Throw(pHook.Owner);
    //    IsAvailable = false;
    //}

    //public override void OnFinishedBeingHooked()
    //{
    //    base.OnFinishedBeingHooked();
    //    if (!(CurrentHook is AnchorHook))
    //    {
    //        InitializeRagdoll();
    //    }
    //    IsAvailable = true;
    //}

    public override void UpdateRagdoll()
    {

    }
}
