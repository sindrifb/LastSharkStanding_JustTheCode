using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombCurveThrow : ThrowModule
{
    private ArcAim AimMod;
    private bool Thrown;

    public override void Initialize(Usable pUsable)
    {
        base.Initialize(pUsable);

        AimMod = pUsable.AimModule as ArcAim;
    }

    private void Update()
    {
        if (Thrown)
        {
            if (AimMod.TRS.DistanceRatio >= 0.95f)
            {
                AimMod.TRS.ObjectToManipulate = null;
            }
        }
    }

    protected override void OverridableThrow()
    {
        if (AimMod.CurveLineRend)
        {
            AimMod.CurveLineRend.enabled = false;
        }

        AimMod.Curve.transform.SetParent(null);
        transform.SetParent(null);
        Usable.Rigidbody.isKinematic = false;

        AimMod.TRS.ObjectToManipulate = Usable.transform;
        AimMod.TRS.Distance = 5;
        AimMod.TRS.Speed = 25;

        Thrown = true;

        StartCoroutine(DelayedSetCollider());

        Usable.UsableController.ResetToStandardHook();

        GameManager.Instance?.SpawnedObjects.Add(Usable.gameObject);
    }

    private IEnumerator DelayedSetCollider()
    {
        //yield return new WaitForSecondsRealtime(0.01f);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        Usable.SetUsableRenderAndColliderActive(true);
    }

    private void OnDestroy()
    {
        if (AimMod != null && AimMod.Curve.gameObject != null)
        {
            Destroy(AimMod.Curve.gameObject);
        }
    }
}
