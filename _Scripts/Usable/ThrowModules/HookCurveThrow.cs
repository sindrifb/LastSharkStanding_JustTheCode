using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookCurveThrow : ThrowModule 
{
    public bool Thrown { get; private set; }
    private ArcAim AimMod;
    private HookYoink EffectMod;
    public LineRenderer YoinkLine;
    public Transform FishingRodEnd;

    public override void Initialize(Usable pUsable)
    {
        base.Initialize(pUsable);

        AimMod = pUsable.AimModule as ArcAim;
        EffectMod = pUsable.EffectModule as HookYoink;
        YoinkLine = GetComponentInChildren<LineRenderer>();
        YoinkLine.startColor = Usable.PlayerController.PlayerColor;
        YoinkLine.endColor = Usable.PlayerController.PlayerColor;
    }

    protected override void IndependentUpdate()
    {
        base.IndependentUpdate();

        if (Thrown)
        {
            YoinkLineUpdate(FishingRodEnd.position, YoinkLine.transform.position);
            if (AimMod.TRS.DistanceRatio >= 1f && !EffectMod.HookableHooked)
            {
                //Thrown = false;
                //ThrowDone();

                
                EffectMod.GroundHit();
            }
        }
    }

    protected override void OverridableThrow()
    {
        if (AimMod.CurveLineRend)
        {
            AimMod.CurveLineRend.enabled = false;
        }

        AimMod.TRS.ObjectToManipulate = Usable.transform;
        //AimMod.TRS.DistanceRatio = 0.3f;
        AimMod.TRS.Distance = 5;
        AimMod.TRS.Speed = 25;

        //Usable.SetUsableRenderAndColliderActive(true);
        StartCoroutine(DelayedSetCollider());
        Thrown = true;

        Usable.HookFlingAmp = 1 + (AimMod.ThrowDistance / AimMod.MaxDistance);
        Usable.UsableController.HookOnFishingRod?.SetActive(false);
        YoinklineActive(true);
    }

    private IEnumerator DelayedSetCollider()
    {
        //yield return new WaitForSecondsRealtime(0.01f);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        Usable.SetUsableRenderAndColliderActive(true);
    }

    public override void Interrupt()
    {
        base.Interrupt();
        AimMod.TRS.ObjectToManipulate = null;
        Thrown = false;
        Usable.ResetStandardHook();
        YoinklineActive(false);
    }

    public override void ThrowDone()
    {
        Thrown = false;
        base.ThrowDone();
    }

    public virtual void YoinkLineUpdate(Vector3 pStart, Vector3 pEnd)
    {
        // make linerenderer enabled, update start and end pos
        YoinkLine?.SetPosition(0, pStart);
        YoinkLine?.SetPosition(1, pEnd);
    }

    public void YoinklineActive(bool pValue)
    {
        if (!pValue)
        {
            YoinkLineUpdate(Vector3.zero, Vector3.zero);
            YoinkLine.enabled = pValue;
        }
        else
        {
            YoinkLineUpdate(FishingRodEnd.position, transform.position);
            YoinkLine.enabled = pValue;
        }
    }
}
