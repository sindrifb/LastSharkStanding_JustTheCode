using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarpoonYoink : EffectModule
{
    public GameObject HarpoonLandParticles;
    [SerializeField]
    private GameObject BreakParticlePrefab;
    private Hookable YoinkedObject;
    [SerializeField]
    private GameObject YoinkCurvePrefab;
    private BansheeGz.BGSpline.Curve.BGCurve Curve;
    private BansheeGz.BGSpline.Curve.BGCurvePointI[] Points;
    private BansheeGz.BGSpline.Components.BGCcTrs TRS;
    private bool HookableHooked;
    [SerializeField]
    private float ThrowForce = 25;
    private bool ResettingHook;
    private LineRenderer YoinkLine;
    private CapsuleCollider CollisionCollider;
    private Vector3 HookedObjPos;
    private bool Interrupted = false;

    public override void Initialize(Usable pUsable)
    {
        base.Initialize(pUsable);
        YoinkLine = GetComponentInChildren<LineRenderer>();
        YoinkLine.startColor = Usable.PlayerController.PlayerColor;
        YoinkLine.endColor = Usable.PlayerController.PlayerColor;
        CollisionCollider = GetComponentInChildren<CapsuleCollider>();
        Usable.IgnoreOwnerCollision();
        CollisionCollider.enabled = true;
    }

    protected override void UpdateEffect()
    {
        base.UpdateEffect();

        if (HookableHooked)
        {
            YoinkLineUpdate(YoinkLine, UsableOwner.transform.position, YoinkLine.transform.position);
            if (YoinkedObject == null || !YoinkedObject.gameObject.activeInHierarchy)
            {
                //YoinkedObject?.transform.SetParent(null);
                //Usable.Interrupt(Usable.PlayerController.CurrentState);
                Interrupt();
                return;
            }
            if (TRS.DistanceRatio >= 0.98f && YoinkedObject.gameObject.activeInHierarchy)
            {
                Release();
            }
        }

    }

    public override void OverridableOnTriggerEnter(Collider col)
    {
        if (!Usable.IsActive)
        {
            return;
        }
        var hookable = col.GetComponent<Hookable>();
        var killOnEnter = col.GetComponent<KillOnEnter>();

        //if (col.isTrigger && !(hookable is PowerUpHookable))
        //{
        //    return;
        //}

        if (hookable != null && hookable.gameObject != Usable.Owner && hookable.IsAvailable)
        {
            if (!Interrupted)
            {
                Usable.SetUsableRenderAndColliderActive(false, true);
                if (Usable.HitParticleSystemPrefab != null)
                {
                    var ps = Instantiate(Usable.HitParticleSystemPrefab, transform.position, transform.rotation);
                    Destroy(ps, 3f);
                }
                Usable.IsActive = false;
                HookableHit(hookable);
            }
            else
            {
                if (BreakParticlePrefab != null)
                {
                    var ps = Instantiate(BreakParticlePrefab, transform.position, transform.rotation);
                    Destroy(ps, 3f);
                }
                Usable.UsableController?.ResetToStandardHook(false);
                Destroy(gameObject);
            }
        }
        else if (killOnEnter != null)
        {
            Usable.IsActive = false;
            if (!Interrupted)
            {
                Usable.UsableController?.ResetToStandardHook();
            }
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        var collider = col.collider;
        if (!Usable.Rigidbody.isKinematic && collider.gameObject.layer != SharkLayer && collider.gameObject.layer != 16 && collider.gameObject.layer != 21)
        {

            Usable.Rigidbody.isKinematic = true;
            CollisionCollider.enabled = false;
            Usable.SetUsableRenderAndColliderActive(false, true);
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.GroundBounce);
            var particles = Instantiate(HarpoonLandParticles, transform.position, Quaternion.identity);
            Destroy(particles, 4);
            GameManager.Instance.SpawnedObjects.Add(gameObject);
            if (Usable.IsActive)
            {
                Usable.IsActive = false;
                Usable.UsableController?.ResetToStandardHook();
            }
        }
    }

    private void HookableHit(Hookable pHookable)
    {
        //if (OwnerPlayerController.CurrentState == PlayerController.State.WindUp)
        //{
        //    OwnerPlayerController.InterruptWindUp();
        //}
        OwnerPlayerController.ChangeState(PlayerController.State.Reeling);
        YoinkedObject = pHookable;

        InitializeCurve(YoinkCurvePrefab, UsableOwner, pHookable.transform.position, UsableOwner.transform.position + Vector3.up * 4.5f);

        TRS.ObjectToManipulate = null;

        Usable.Rigidbody.velocity = Vector3.zero;

        if (pHookable is PlayerHookable)
        {
            pHookable.GetComponent<PlayerController>().OnBeingHit();
            transform.position = pHookable.GetComponent<PlayerHookable>().HookedHookPosition.position;
            pHookable.transform.SetParent(transform);
            //transform.parent = pHookable.GetComponent<PlayerHookable>().HookedHookPosition;
            //transform.localPosition = Vector3.zero;
            UsableHitEvent uEvent = new UsableHitEvent
            {
                Description = "Usable Harpoon Event",
                Usable = Usable.Type,
                PlayerHit = pHookable.gameObject,
                RewiredID = OwnerPlayerController.RewiredID,
                UsableOwner = UsableOwner
            };
            uEvent.FireEvent();
        }
        else
        {
            pHookable.transform.SetParent(transform);
            pHookable.transform.localPosition = Vector3.zero;
            //transform.parent = pHookable.transform;
            //transform.localPosition = Vector3.zero;
        }

        
        pHookable.ConnectHook(true, Usable);
        TRS.ObjectToManipulate = transform;
        //TRS.ObjectToManipulate = pHookable.transform;
        //ChangeStartEndOnCurve(pHookable.transform.position, UsableOwner.transform.position + Vector3.up * 4f);
        YoinklineActive(YoinkLine, true);
        //pHookable.IsHooked = true;
        HookableHooked = true;

        HookedObjPos = pHookable.transform.position;
    }

    private void Release()
    {
        HookableHooked = false;
        YoinkedObject?.transform.SetParent(null);
        var dir = (UsableOwner.transform.position - HookedObjPos).normalized;

        if (YoinkedObject != null && YoinkedObject is PlayerHookable)
        {
            //YoinkedObject.Push((-UsableOwner.transform.forward + (Vector3.up) / 2f) * (ThrowForce + GameManager.Instance.Difficulty) * Usable.HookFlingAmp);
            YoinkedObject.Push((dir + (Vector3.up) / 2f) * (ThrowForce + GameManager.Instance.Difficulty) * Usable.HookFlingAmp);
        }
        else if (YoinkedObject != null && YoinkedObject is PowerUpHookable)
        {
            //YoinkedObject.GetComponent<Rigidbody>().isKinematic = false;
            //YoinkedObject.GetComponent<Rigidbody>().useGravity = false;
            //YoinkedObject.OnFinishedBeingHooked();
            Usable.PlayerController.ChangeState(PlayerController.State.Idle);
            YoinkedObject.GetComponent<PowerUpPickupTest>().GivePowerUp(UsableOwner.GetComponent<UsableController>());
        }
        else if (YoinkedObject != null && YoinkedObject is HazardHookable)
        {
            YoinkedObject.OnFinishedBeingHooked();
        }
        else if (YoinkedObject != null)
        {
            var rb = YoinkedObject.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.AddForce((-UsableOwner.transform.forward + (Vector3.up) / 2f) * (ThrowForce + GameManager.Instance.Difficulty) * Usable.HookFlingAmp, ForceMode.Impulse);
            rb.AddForce((dir + (Vector3.up) / 2f) * (ThrowForce + GameManager.Instance.Difficulty) * Usable.HookFlingAmp, ForceMode.Impulse);
        }

        YoinklineActive(YoinkLine, false);

        if (YoinkedObject != null)
        {
            //YoinkedObject.IsHooked = false;
            YoinkedObject.ConnectHook(false, Usable);
        }

        YoinkedObject = null;
        TRS.ObjectToManipulate = null;
        Destroy(Curve.gameObject);

        if (Usable.PlayerController.CurrentState != PlayerController.State.Idle)
        {
            Usable.UsableController.ResetToStandardHook();
        }
    }

    protected virtual void InitializeCurve(GameObject pCurvePrefab, GameObject pOwner, Vector3 pStart, Vector3 pEnd)
    {
        var spawnedCurve = Instantiate(pCurvePrefab);
        spawnedCurve.transform.position = pOwner.transform.position; ;
        Curve = spawnedCurve.GetComponent<BansheeGz.BGSpline.Curve.BGCurve>();
        spawnedCurve.GetComponent<LineRenderer>().enabled = false;
        Points = Curve.Points;
        TRS = Curve.GetComponent<BansheeGz.BGSpline.Components.BGCcTrs>();
        TRS.Speed = 0;
        //ChangeStartEndOnCurve(pOwner.transform.position, pOwner.transform.position);
        Points[0].PositionWorld = pStart;
        Points[Points.Length - 1].PositionWorld = pEnd;
    }

    protected virtual void YoinkLineUpdate(LineRenderer pYoinkLine, Vector3 pStart, Vector3 pEnd)
    {
        // make linerenderer enabled, update start and end pos
        pYoinkLine.SetPosition(0, pStart);
        pYoinkLine.SetPosition(1, pEnd);
    }

    protected void YoinklineActive(LineRenderer pYoinkLine, bool pValue)
    {
        pYoinkLine.enabled = pValue;
    }

    public override void Interrupt()
    {
        base.Interrupt();

        Interrupted = true;
        // release results in stackoverflow with harpoonstraightthrow, but works with projectile throw
        //Usable.UsableController.ResetToStandardHook();
        //Release();

        if (HookableHooked)
        {
            HookableHooked = false;

            if (YoinkedObject != null)
            {
                YoinkedObject.transform.SetParent(null);
                YoinkedObject.ConnectHook(false, Usable);

                if (YoinkedObject.gameObject.activeInHierarchy)
                {
                    var rb = YoinkedObject.GetComponent<Rigidbody>();
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    YoinkedObject.Push(((-UsableOwner.transform.forward + (Vector3.up) / 6f)).normalized, ThrowForce);
                }
            }

            TRS.ObjectToManipulate = null;
            YoinkedObject = null;
            YoinklineActive(YoinkLine, false);
            Destroy(Curve.gameObject);

            //OwnerPlayerController.ChangeState(PlayerController.State.Idle);
            Usable.UsableController.ResetToStandardHook();
        }

    }
}
