using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookYoink : EffectModule
{
    private Hookable YoinkedObject;
    [SerializeField]
    private GameObject YoinkCurvePrefab;
    [SerializeField]
    private GameObject TravelCurvePrefab;
    private BansheeGz.BGSpline.Curve.BGCurve Curve;
    private BansheeGz.BGSpline.Curve.BGCurvePointI[] Points;
    private BansheeGz.BGSpline.Components.BGCcTrs TRS;

    private bool TravelingByHook;
    public bool HookableHooked; //{ get; private set; }
    [SerializeField]
    private float ThrowForce = 25;
    private bool ResettingHook;
    private HookCurveThrow ThrowMod;

    public override void Initialize(Usable pUsable)
    {
        base.Initialize(pUsable);
        ThrowMod = pUsable.ThrowModule as HookCurveThrow;
    }

    protected override void UpdateEffect()
    {
        base.UpdateEffect();

        if (TravelingByHook)
        {
            ThrowMod.YoinkLineUpdate(ThrowMod.FishingRodEnd.position, ThrowMod.YoinkLine.transform.position);
            var distanceFromEndPoint = Vector3.Distance(Usable.Owner.transform.position, Points[Points.Length - 1].PositionWorld);
            if (TRS.DistanceRatio >= 0.7f && distanceFromEndPoint <= 1f)
            {
                UsableOwner.transform.position = Points[Points.Length - 1].PositionWorld + new Vector3(0, 1, 0);
                TravelingByHookDone();
            }
        }
        else if (HookableHooked)
        {
            ThrowMod.YoinkLineUpdate(ThrowMod.FishingRodEnd.position, ThrowMod.YoinkLine.transform.position);
            var playerHookable = YoinkedObject as PlayerHookable;

           
            if (YoinkedObject == null || !YoinkedObject.gameObject.activeInHierarchy /*|| (playerHookable != null && playerHookable.PlayerController.CurrentState == PlayerController.State.Ragdoll)*/)
            {
                //var powerUpH = YoinkedObject as PowerUpHookable;
                //if (powerUpH == null && YoinkedObject != null)
                //{
                //    YoinkedObject?.transform.SetParent(null);
                //}

                Usable.Interrupt(PlayerController.State.Reeling);
                return;
            }
            else if (TRS.DistanceRatio >= 0.98f && YoinkedObject.gameObject.activeInHierarchy)
            {
                //print("release hookable");
                Release();
            }
        }
        else if (ResettingHook)
        {
            ThrowMod.YoinkLineUpdate(ThrowMod.FishingRodEnd.position, ThrowMod.YoinkLine.transform.position);
            transform.position = Vector3.Lerp(transform.position, UsableOwner.transform.position, 8f * Time.deltaTime);

            if (Vector3.Distance(transform.position, UsableOwner.transform.position) <= 0.2f)
            {
                Usable.ResetStandardHook();
                ResettingHook = false;
                ThrowMod.YoinklineActive(false);
                if (OwnerPlayerController.CurrentState == PlayerController.State.Reeling)
                {
                    OwnerPlayerController.ChangeState(PlayerController.State.Idle);
                }
            }
        }
    }

    private void TravelingByHookDone()
    {
        TravelingByHook = false;
        TRS.ObjectToManipulate = null;
        ThrowMod.YoinklineActive(false);
        Destroy(Curve.gameObject);
        Usable.ResetStandardHook();
        OwnerPlayerController.TravelingByHookDone();
    }

    public override void OverridableOnTriggerEnter(Collider col)
    {
        var hookable = col.GetComponent<Hookable>();

        //if (col.isTrigger && !(hookable is PowerUpHookable))
        //{
        //    return;
        //}
        
        if (hookable != null && hookable.gameObject != Usable.Owner && hookable.IsAvailable)
        {
            Usable.SetUsableRenderAndColliderActive(false, true);
            if (Usable.HitParticleSystemPrefab != null)
            {
                var ps = Instantiate(Usable.HitParticleSystemPrefab, Usable.Owner.transform.position, Usable.Owner.transform.rotation);
                Destroy(ps, 3f);
            }

            HookableHit(hookable);
        }
        else if (col.gameObject.layer == Mathf.Log(DeathZoneLayer, 2))
        {
            Usable.SetUsableRenderAndColliderActive(false, true);
            NothingHit();
        }
    }

    private void NothingHit()
    {
        ThrowMod.ThrowDone();
        //OwnerPlayerController.ChangeState(PlayerController.State.Reeling);
        var aimMod = Usable.AimModule as ArcAim;
        aimMod.TRS.ObjectToManipulate = null;
        //Destroy(aimMod.Curve.gameObject);
        aimMod.ResetAim();
        ResettingHook = true;
        Usable.PlayerController.PlayUpperBodyAnimation(Constants.AnimationParameters.Pull);
    }

    private void HookableHit(Hookable pHookable)
    {
        OwnerPlayerController.ChangeState(PlayerController.State.Reeling);
        ThrowMod.ThrowDone();
        YoinkedObject = pHookable;

        var arcAim = Usable.AimModule as ArcAim;
        //arcAim.Curve.transform.SetParent(null);
        arcAim.TRS.ObjectToManipulate = null;
        //Destroy(arcAim.Curve.gameObject);
        arcAim.ResetAim();

        InitializeCurve(YoinkCurvePrefab, UsableOwner, pHookable.transform.position, UsableOwner.transform.position + Vector3.up * 4f);
        
        TRS.ObjectToManipulate = null;

        if (pHookable is PlayerHookable)
        {
            pHookable.GetComponent<Rigidbody>().velocity = Vector3.zero;
            pHookable.GetComponent<PlayerController>().OnBeingHit();
            transform.position = pHookable.GetComponent<PlayerHookable>().HookedHookPosition.position;
            pHookable.transform.SetParent(transform);
            //transform.parent = pHookable.GetComponent<PlayerHookable>().HookedHookPosition;
            //transform.localPosition = Vector3.zero;
            UsableHitEvent eInfo = new UsableHitEvent
            {
                Description = "Standard hook hit event",
                PlayerHit = pHookable.gameObject,
                RewiredID = OwnerPlayerController.RewiredID,
                Usable = Usable.UsableType.StandardHook,
                UsableOwner = UsableOwner
            };
            eInfo.FireEvent();
        }
        
        else
        {
            pHookable.transform.SetParent(transform);
            pHookable.transform.localPosition = Vector3.zero;
            if (pHookable is MineHookable)
            {
                pHookable.GetComponent<MineHit>().Throw(gameObject);
            }
            //transform.parent = pHookable.transform;
            //transform.localPosition = Vector3.zero;
        }

        pHookable.ConnectHook(true, Usable);

        //TRS.ObjectToManipulate = pHookable.transform;
        TRS.ObjectToManipulate = transform;
        //pHookable.IsHooked = true;
        //pHookable.AttachedUsable = Usable;
        HookableHooked = true;
        Usable.PlayerController.PlayUpperBodyAnimation(Constants.AnimationParameters.Pull);
    }

    public void GroundHit()
    {
        ThrowMod.ThrowDone();
        Usable.SetUsableRenderAndColliderActive(false, true);
        Usable.transform.SetParent(null);
        Usable.AimModule.Aim.transform.SetParent(null);

        var arcAim = Usable.AimModule as ArcAim;

        arcAim.TRS.ObjectToManipulate = null;
        var targetPos = arcAim.Points[arcAim.Points.Length - 1].PositionWorld + (Vector3.up * 0.5f);
        arcAim.ResetAim();


        RaycastHit hit;
        Ray ray = new Ray();
        ray.origin = arcAim.Points[arcAim.Points.Length - 1].PositionWorld + (Vector3.up * 10f);
        ray.direction = Vector3.down;
        //var rayCast = Physics.Raycast(ray, out hit, 40f, arcAim.RayCastHitLayers, QueryTriggerInteraction.Ignore);

        var sphereCast = Physics.SphereCast(ray, 0.5f, out hit, 40f, arcAim.RayCastHitLayers, QueryTriggerInteraction.Ignore);

        if (sphereCast)
        {
            if (hit.transform.gameObject.layer == Mathf.Log(GroundLayer.value, 2))
            {
                targetPos = hit.point + (Vector3.up * 0.5f);
                OwnerPlayerController.OnMiss();
                InitializeCurve(TravelCurvePrefab, UsableOwner, UsableOwner.transform.position, targetPos);
                //ChangeStartEndOnCurve(UsableOwner.transform.position, Usable.AimModule.TargetPos);
                TRS.RotateObject = false;

                TRS.ObjectToManipulate = UsableOwner.transform;
                TRS.Speed = 25;
                TravelingByHook = true;
                //ThrowMod.YoinklineActive(true);
                Usable.PlayerController.PlayUpperBodyAnimation(Constants.AnimationParameters.TravelByOwnHook);
            }
            else
            {
                NothingHit();
            }
        }
        else
        {
            NothingHit();
        }
    }

    private void Release()
    {
        HookableHooked = false;
        YoinkedObject.transform.SetParent(null);
        Usable.ResetStandardHook();
        if (YoinkedObject != null && YoinkedObject is PowerUpHookable)
        {
            Usable.PlayerController.ChangeState(PlayerController.State.Idle);
            YoinkedObject?.GetComponent<PowerUpPickupTest>().GivePowerUp(UsableOwner.GetComponent<UsableController>());
        }
        else if (YoinkedObject != null)
        {
           var rb = YoinkedObject.GetComponent<Rigidbody>();
           rb.velocity = new Vector3(rb.velocity.x,0,rb.velocity.z);
           YoinkedObject.Push(((-UsableOwner.transform.forward + (Vector3.up) / 6f)).normalized,  (ThrowForce + GameManager.Instance.Difficulty) * Usable.HookFlingAmp);
        }

        if (YoinkedObject != null)
        {
            YoinkedObject.ConnectHook(false, Usable);
            //YoinkedObject.IsHooked = false;
            //YoinkedObject.AttachedUsable = null;
        }
        
        TRS.ObjectToManipulate = null;
        YoinkedObject = null;
        ThrowMod.YoinklineActive(false);
        Destroy(Curve.gameObject);

        if (OwnerPlayerController.CurrentState == PlayerController.State.Reeling)
        {
            OwnerPlayerController.ChangeState(PlayerController.State.Idle);
        }
    }

    //protected void Break()
    //{
    //    HookableHooked = false;

    //    YoinkedObject.Push(((-UsableOwner.transform.forward + (Vector3.up) / 4f)).normalized, (ThrowForce + GameManager.Instance.Difficulty) * Usable.HookFlingAmp);

    //    YoinkedObject.IsHooked = false;
    //    YoinkedObject.AttachedUsable = null;
    //    TRS.ObjectToManipulate = null;
    //    YoinkedObject = null;
    //    Destroy(Curve.gameObject);
    //}

    protected virtual void InitializeCurve(GameObject pCurvePrefab, GameObject pOwner, Vector3 pStart, Vector3 pEnd)
    {
        var spawnedCurve = Instantiate(pCurvePrefab);
        spawnedCurve.transform.position = pOwner.transform.position; ;
        Curve = spawnedCurve.GetComponent<BansheeGz.BGSpline.Curve.BGCurve>();
        spawnedCurve.GetComponent<LineRenderer>().enabled = false;
        Curve.Points[0].PositionWorld = pStart;
        Curve.Points[Curve.Points.Length - 1].PositionWorld = pEnd;
        Points = Curve.Points;
        TRS = Curve.GetComponent<BansheeGz.BGSpline.Components.BGCcTrs>();
        TRS.Speed = 0;
    }

    public override void Interrupt()
    {
        base.Interrupt();

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
            Usable.ResetStandardHook();
            YoinkedObject = null;
            ThrowMod.YoinklineActive(false);
            Destroy(Curve.gameObject);

            OwnerPlayerController.ChangeState(PlayerController.State.Idle);
        }
        else if (TravelingByHook)
        {
            TravelingByHookDone();
        }
        else if (ResettingHook)
        {
            Usable.ResetStandardHook();
        }
    }

    public override void StealFromHook()
    {
        base.StealFromHook();
        YoinkedObject = null;
        Interrupt();
    }
}
