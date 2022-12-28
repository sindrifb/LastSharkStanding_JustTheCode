using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardHook : Hook
{
    public BansheeGz.BGSpline.Curve.BGCurve LineCurve;
    private LineRenderer LineRenderer;
    private SphereCollider SphereCollider;
    public Transform FishingRodEndPos;

    private void Start()
    {
        LineRenderer = GetComponent<LineRenderer>();
        SphereCollider = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        LineRenderer.enabled = SphereCollider.enabled || PlayerController.IsTravelingByOwnHook;

        if (LineCurve != null)
        {
            var points = LineCurve.Points;

            points[0].PositionWorld = FishingRodEndPos.position;
            points[points.Length - 1].PositionWorld = transform.position;
        }
    }

    public override void UpdateThrowing()
    {
        if (ThrowWhenPossible)
        {
            UpdateWindUp();
            if (ThrowDistance >= MinDistance)
            {
                Throw();
                ThrowWhenPossible = false;
            }
        }
        // if HookedObject is null, the hook did not hit anything hookable
        if (TRS.DistanceRatio == 1 && HookedObject == null)
        {
            //TODO: Add check if target is on ground
            if (EndPositionIsGround())
            {
                PullPlayerToTarget();
            }
            else
            {
                OnFinishedThrow();
            }
        }// Hit something Hookable
        else if (TRS.DistanceRatio == 1 && HookedObject != null)
        {
            OnFinishedThrow();
        }
    }

    public override void UpdateWindUp()
    {
        ChargeTimer += Time.deltaTime;
        ChargeAmount = Mathf.Lerp(0, 1, Mathf.Clamp01(ChargeTimer / ChargeTime));
        ThrowDistance = MaxDistance * ChargeAmount;

        Vector3 targetPos = new Vector3();

        RaycastHit hit;
        Ray ray = new Ray();

        ray.origin = Owner.transform.position + (Owner.transform.forward * ThrowDistance) + (Vector3.up * 20f);
        ray.direction = Vector3.down;
        var rayCast = Physics.Raycast(ray, out hit, 40f, RayCastHitLayers, QueryTriggerInteraction.Ignore);
        //var rayCast = Physics.Raycast(ray, out hit, 40f, ~0, QueryTriggerInteraction.Ignore);

        if (rayCast)
        {
            if (hit.transform.gameObject.layer == Mathf.Log(GroundLayer.value, 2))
            {
                SetAimActive(true);
            }
            else
            {
                SetAimActive(false);
            }

            targetPos = hit.point;
        }
        else
        {
            var outsideRay = new Ray(Owner.transform.position + (Owner.transform.forward * ThrowDistance) + (Vector3.up * 10f), Vector3.down);
            if (Physics.Raycast(outsideRay, out hit, 20f))
            {
                targetPos = hit.point;
                SetAimActive(true);
            }
            else
            {
                targetPos = ray.origin + (Vector3.down * 20f);
                SetAimActive(false);
            }
        }

        //Vector3 TargetPos = (Owner.transform.position + Owner.transform.forward * ThrowDistance);

        ChangeStartEndOnCurve(Owner.transform.position, targetPos);
        Aim.transform.position = Points[Points.Length - 1].PositionWorld + AimOffset;


        if (Curve != null && Curve.GetComponent<LineRenderer>() && !Curve.GetComponent<LineRenderer>().enabled)
        {
            Curve.GetComponent<LineRenderer>().enabled = true;
        }
    }

    public override void InitializeHookCurve(GameObject pCurvePrefab)
    {
        base.InitializeHookCurve(pCurvePrefab);
        ChangeStartEndOnCurve(HookController.HookOnFishRod.transform.position, Owner.transform.position);
    }

    public override void OnFinishedThrow()
    {
        base.OnFinishedThrow();
    }

    protected override void Throw()
    {
        ShowFishingPoleHook(false);
        base.Throw();
        HookThrowAmp = 1 + (ThrowDistance/MaxDistance);
    }

    public override void InitializeWindUp()
    {
        base.InitializeWindUp();
        AimOffset = new Vector3(0, 0.3f, 0);
    }

    public override void ResetHook()
    {
        base.ResetHook();
        ShowFishingPoleHook(true);
    }

    /// <summary>
    /// Activates/Deactivates hook on fishing pole
    /// </summary>
    /// <param name="pValue"></param>
    private void ShowFishingPoleHook(bool pValue)
    {
        HookController.HookOnFishRod.SetActive(pValue);

    }
}
