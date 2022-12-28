using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

public abstract class Hook : MonoBehaviour
{
    public GameObject ThrowParticleSystem;
    public GameObject HitParticleSystem;

    protected ParentConstraint ParentConstraint;
    private List<Transform> OtherPlayers;
    protected Vector3 AimOffset = Vector3.zero;
    private Transform CurrentAimTarget;
    public LayerMask GroundLayer;
    public LayerMask RayCastHitLayers;
    public GameObject CurvePrefab;

    public float HookSpeed = 10f;
    [Tooltip("Seconds until full charge")]
    public float ChargeTime = 1f;
    public float MaxDistance = 5f;
    public float MinDistance = 1f;
    public float HookThrowAmp = 1;

    public BansheeGz.BGSpline.Curve.BGCurve Curve;
    protected BansheeGz.BGSpline.Curve.BGCurvePointI[] Points;
    protected BansheeGz.BGSpline.Components.BGCcTrs TRS;
    
    protected PlayerController PlayerController;

    public bool ReachedTarget { get; protected set; } = false;
    public bool ThrowWhenPossible { get; protected set; } = false;
    protected GameObject Aim;
    public GameObject Owner;
    public Hookable HookedObject;
    protected HookController HookController;

    public float ThrowDistance { get; protected set; }
    protected float ChargeAmount;
    protected float ChargeTimer;
    protected Vector3 AimYOffset = new Vector3(0, -0.85f, 0);
    protected Vector3 ReelingEndPos;


    protected virtual void IndependentUpdate() { }

    private void Update()
    {
        IndependentUpdate();
    }

    public void Initialize(PlayerController pController, HookController pHookController)
    {
        ParentConstraint = GetComponent<ParentConstraint>();
        PlayerController = pController;
        HookController = pHookController;
        InitializeAims();
        transform.position = Owner.transform.position;
        SetHookRenderAndColliderActive(false);
        SetAimActive(false);
        //Aim.GetComponent<Renderer>().enabled = false; //current hook hasn't been set yet, line above doesn't correctly turn the aim off (only in this initialize)
        OtherPlayers = FindObjectsOfType<PlayerController>().Where(a => a != PlayerController).Select(a => a.transform).ToList();
        //Aim.GetComponent<MeshRenderer>().material.color = PlayerController.PlayerColor;
    }

    private void InitializeAims()
    {
        //Aim = PlayerController.Aim;
        Aim.transform.position = Owner.transform.position + AimYOffset;
        Aim.GetComponent<Renderer>().enabled = false;
        ///PlayerController.AimAnchor.SetActive(false);
        //PlayerController.AimAnchor.SetActive(false);
    }

    public virtual void InitializeWindUp()
    {
        ChargeTimer = 0f;
        ChargeAmount = MinDistance / MaxDistance;
        InitializeHookCurve(CurvePrefab);
        SetAimActive(true);
        AimOffset = Vector3.zero;
        HookController.PlayHookAnimation(Constants.AnimationParameters.Charge);
    }

    public virtual void InitializeThrow()
    {
        if (ParentConstraint != null)
        {
            ParentConstraint.constraintActive = false;
        }
        //enforce minimum throw distance
        if (ThrowDistance < MinDistance)
        {
            ThrowWhenPossible = true;
        }
        else
        {
            Throw();
        }

        // check if ground
        
    }

    protected virtual void Throw()
    {
        if (Curve.GetComponent<LineRenderer>())
        {
            Curve.GetComponent<LineRenderer>().enabled = false;
        }

        if (ThrowParticleSystem != null && Owner != null)
        {
            var ps = Instantiate(ThrowParticleSystem, Owner.transform.position, Owner.transform.rotation);
            Destroy(ps, 3f);
        }
        
        SetHookRenderAndColliderActive(true);
        ConnectHook(true, transform);
        HookController?.PlayHookAnimation(Constants.AnimationParameters.Throw);


        //ThrowEvent.Play();
        //FlyEvent.Play();
        //ChargeEvent.Stop();
    }

    public virtual void Interrupt()
    {
        //ChangeState(State.Stunned);

        Aim.transform.localPosition = Vector3.zero;
        transform.localPosition = Vector3.zero;

        Destroy(Curve.gameObject, 5f);
    }

	protected virtual Hookable CheckForHit(GameObject pHit)
    {
        return pHit.GetComponentInChildren<Hookable>();
    }

    public virtual void UpdateThrowing()
    {
        if (Curve == null)
        {
            PlayerController.OnThrowingDone();
            return;
        }
        if (ThrowWhenPossible)
        {
            UpdateWindUp();
            if (ThrowDistance >= MinDistance)
            {
                Throw();
                ThrowWhenPossible = false;
            }
        }

        if (TRS.DistanceRatio >= .98f)
        {
            OnFinishedThrow();
        }
    }

    public void ConnectHook(bool pValue, Transform pTransform)
    {
        if (pValue)
        {
            TRS.ObjectToManipulate = pTransform;
            TRS.Speed = HookSpeed;
        }
        else
        {
            if (TRS != null)
            {
                TRS.ObjectToManipulate = null;
            }
        }
    }

    protected virtual void PullPlayerToTarget()
    {
        // stunned state
        transform.SetParent(null);
        Aim.transform.SetParent(null);
        PlayerController.OnMiss();
        HookController?.PlayHookAnimation(Constants.AnimationParameters.TravelByOwnHook);
        TRS.Speed = 0;
        TRS.DistanceRatio = 0;
        ConnectHook(true, Owner.transform);
        TRS.RotateObject = false;
        SetHookRenderAndColliderActive(true,true);
    }

    public virtual void UpdateTravelingByHook()
    {
        if (TRS.DistanceRatio == 1)
        {
            PlayerController.TravelingByHookDone();
            HookController.PlayHookAnimation(Constants.AnimationParameters.HookIdle);
            //reparent
            transform.SetParent(Owner.transform);
            Aim.transform.SetParent(Owner.transform);
        }
    }

    public void UpdateReeling()
    {
        transform.position = Vector3.Lerp(transform.position, ReelingEndPos, 8f * Time.deltaTime);
        if (Vector3.Distance(transform.position, ReelingEndPos) <= 0.2f)
        {
            OnReelingFinished();
        }
    }

    public void InitializeReeling()
    {
        Destroy(Curve.gameObject);
        SetAimActive(false);
        GetComponent<SphereCollider>().enabled = false;
        ReelingEndPos = Owner.transform.position;
    }

    public virtual void OnFinishedThrow()
    {
        PlayerController.OnThrowingDone();
    }

    public virtual void OnReelingFinished()
    {
        PlayerController.OnReelingDone();
    }

    protected bool EndPositionIsGround()
    {
        return Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 10, transform.position.z), Vector3.down, 100, GroundLayer);  
    }

    public virtual void UpdateWindUp()
    {

        if (Curve != null && Curve.GetComponent<LineRenderer>() && !Curve.GetComponent<LineRenderer>().enabled)
        {
            Curve.GetComponent<LineRenderer>().enabled = true;
        }

        ChargeTimer += Time.deltaTime;
        ChargeAmount = Mathf.Lerp(0, 1, Mathf.Clamp01(ChargeTimer / ChargeTime));
        ThrowDistance = MaxDistance * ChargeAmount;

        Vector3 TargetPos = new Vector3();

        RaycastHit hit;
        Ray ray = new Ray();

        ray.origin = Owner.transform.position + (Owner.transform.forward * ThrowDistance);
        ray.direction = Vector3.down;
        var rayCast = Physics.Raycast(ray, out hit, 20f, GroundLayer);

        if (rayCast)
        {

        }



        TargetPos = (Owner.transform.position + Owner.transform.forward * ThrowDistance) + AimOffset;
        //AimAssist(TargetPos);
        ChangeStartEndOnCurve(Owner.transform.position, TargetPos);

        //Aim.transform.position = Owner.transform.position + AimYOffset + Owner.transform.forward * ThrowDistance;

        Aim.transform.position = Points[Points.Length - 1].PositionWorld + AimYOffset;
    }

    public virtual void InitializeHookCurve(GameObject pCurvePrefab)
    {
        var spawnedCurve = Instantiate(pCurvePrefab);
        var curveLineRend = spawnedCurve.GetComponent<LineRenderer>();
        if (curveLineRend)
        {
            curveLineRend.endColor = PlayerController.PlayerColor;
            curveLineRend.startColor = PlayerController.PlayerColor;
        }
        
        spawnedCurve.transform.position = Owner.transform.position;
        Curve = spawnedCurve.GetComponent<BansheeGz.BGSpline.Curve.BGCurve>();
        if (Curve.GetComponent<LineRenderer>())
        {
            Curve.GetComponent<LineRenderer>().enabled = false;
        }

        Points = Curve.Points;
        TRS = Curve.GetComponent<BansheeGz.BGSpline.Components.BGCcTrs>();
        ChangeStartEndOnCurve(Owner.transform.position, Owner.transform.position);
    }

    public virtual void ResetHook()
    {
        if (ParentConstraint != null)
        {
            ParentConstraint.constraintActive = true;
        }
        Aim.transform.SetParent(Owner.transform);
        Aim.transform.position = Owner.transform.position + AimYOffset;
        transform.position = Owner.transform.position;
        transform.SetParent(Owner.transform);
        SetHookRenderAndColliderActive(false);
        SetAimActive(false);

        HookedObject = null;
        if (Curve != null)
        {
            Destroy(Curve.gameObject);
        }
    }

    public virtual void SetHookRenderAndColliderActive(bool pValue, bool pOnlyCollider = false)
    {
        if (!pOnlyCollider)
        {
            GetComponent<MeshRenderer>().enabled = pValue;
        }
        GetComponent<SphereCollider>().enabled = pOnlyCollider ? false : pValue;
    }

    public void SetActiveHook(bool pValue)
    {
        gameObject.SetActive(pValue);
        //SetHookActive(pValue);
    }

    protected void SetAimActive(bool pValue)
    {
        if (HookController.CurrentHook is StandardHook)
        {
            Aim.GetComponent<Renderer>().enabled = pValue;
        }
        else if (HookController.CurrentHook is AnchorHook)
        {
            //PlayerController.AimAnchor.SetActive(pValue);
        }
        else
        {
           // PlayerController.AimHarpoon.SetActive(pValue);
        }
    }

    protected virtual void OnTriggerEnter(Collider col)
    {
        if (col.isTrigger)
        {
            return;
        }
        var hookedObject = CheckForHit(col.gameObject);

        // If not hooking anything, and hookedObject hit is not nothing and it is not the owner
        if (HookedObject == null && hookedObject != null && col.gameObject != Owner && hookedObject.IsAvailable)
        {
            TRS.ObjectToManipulate = null;
            //hookedObject.Activate(this);
            HookedObject = hookedObject;
            HookController.PlayHookAnimation(Constants.AnimationParameters.Pull);
            if (HitParticleSystem != null)
            {
                var ps = Instantiate(HitParticleSystem, HookedObject.transform.position, Quaternion.identity);
                Destroy(ps, 3f);
            }

            if (!(col.GetComponent<Hookable>() is PlayerHookable))
            {
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.HookSound.HookHit);
            }
        }

    }

    protected void ChangeStartEndOnCurve(Vector3 pStart, Vector3 pEnd)
    {
        if (Points != null)
        {
            Points[0].PositionWorld = pStart;
            Points[Points.Length - 1].PositionWorld = pEnd;
        }
    }

    private void OnDestroy()
    {
        if (Curve != null)
        {
            Destroy(Curve.gameObject);
        }
        Destroy(Aim);
    }
}