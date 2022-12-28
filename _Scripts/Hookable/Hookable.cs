using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Hookable : MonoBehaviour
{
    public bool IsAvailable
    {
        get
        {
            if (this is PlayerHookable)
            {
                return GetComponent<PlayerController>()?.IsAvailable ?? m_IsAvailable;
            }
            else
            {
                return m_IsAvailable;
            }
        }
        set { m_IsAvailable = value; }
    }

    public float startDrag { get; private set; }
    public float startAngularDrag { get; private set; }
    protected bool m_IsAvailable;
    [HideInInspector]
    public bool IsHooked /*{ get; protected set; }*/;
    public bool IsRagdolling { get; private set; }
    [HideInInspector]
    public bool DontGetUp = false;
    public GameObject Ragdoll;
    protected Rigidbody RagdollRigidbody;
    [HideInInspector]
    public Usable AttachedUsable;
    private float TimePassed = 0;

    public virtual void Initialize()
    {
        Ragdoll = gameObject;
        RagdollRigidbody = Ragdoll?.GetComponent<Rigidbody>();
        startDrag = RagdollRigidbody.drag;
        startAngularDrag = RagdollRigidbody.angularDrag;
    }

    private void Start()
    {
        IsAvailable = true;
        if (!(this is PlayerHookable))
        {
            Initialize();
        }
    }

    private void Update()
    {
        if (!(this is PlayerHookable))
        {
            //if (IsHooked)
            //{
            //    UpdateHooked();
            //}
            if (IsRagdolling)
            {
                UpdateRagdoll();
            }
            
        }

    }

    public virtual void OnHooked()
    {
        IsAvailable = true;
    }

    public virtual void OnFinishedBeingHooked()
    {
        IsHooked = false;
    }

    //public void UpdateHooked()
    //{

    //    if (AttachedUsable == null)
    //    {
    //        return;
    //    }

    //    if (this is PlayerHookable)
    //    {
    //        AttachedUsable.transform.localPosition = Vector3.zero;
    //        AttachedUsable.transform.localRotation = Quaternion.identity;
    //    }
    //    else
    //    {
    //        AttachedUsable.transform.position = transform.position;
    //    }
    //}

    public void ConnectHook(bool pValue, Usable pUsable)
    {
        if (pValue)
        {
            if (AttachedUsable != null)
            {
                AttachedUsable.EffectModule.StealFromHook();
            }
            AttachedUsable = pUsable;
            OnHooked();
        }
        else
        {
            AttachedUsable = null;
        }

        IsHooked = pValue;
    }

    /// <summary>
    /// Initializes ragdoll state with input force, Only for use when not hooked
    /// </summary>
    /// <param name="PushVelocity"></param>
    protected void InitializeRagdoll(Vector3 PushVelocity)
    {
        TimePassed = 0f;
        RagdollSetActive(true);
        //AttachedUsable?.UsableController.ResetToStandardHook();
        AttachedUsable?.UsableController.CurrentUsable.Interrupt(AttachedUsable.PlayerController.CurrentState);
        RagdollRigidbody.drag = startDrag;
        RagdollRigidbody.angularDrag = startAngularDrag;
        RagdollRigidbody.AddForce(PushVelocity, ForceMode.Impulse);
        RagdollRigidbody.AddTorque(new Vector3(UnityEngine.Random.Range(1, 20), UnityEngine.Random.Range(1, 20), UnityEngine.Random.Range(1, 20)), ForceMode.Impulse);
        AttachedUsable = null;
    }

    public virtual void UpdateRagdoll()
    {
        if (DontGetUp)
        {
            return;
        }
        bool grounded = GetComponent<CheckIfGrounded>()?.OnGround ?? true;

        TimePassed += Time.deltaTime;
        if (TimePassed > 1 && grounded)
        {
            RagdollRigidbody.drag = startDrag;
            RagdollRigidbody.angularDrag = startAngularDrag;
            TimePassed = 0;
            RagdollSetActive(false);
            OnFinishedRagdolling();
            return;
        }

        if (RagdollRigidbody.velocity.sqrMagnitude < 3f * 3f && grounded)
        {
            RagdollRigidbody.drag = startDrag;
            RagdollRigidbody.angularDrag = startAngularDrag;
            TimePassed = 0;
            RagdollSetActive(false);
            OnFinishedRagdolling();
        }
    }

    public virtual void OnFinishedRagdolling()
    {
        
    }

    public void RagdollSetActive(bool pValue, bool pLateSet = false)
    {
        if (pLateSet)
        {
            StartCoroutine(LateSetRagdoll());
        }
        else
        {
            IsRagdolling = pValue;
        }
       
        if (pValue)
        {
            RagdollRigidbody.isKinematic = !pValue;
            RagdollRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        else
        {
            //RagdollRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            RagdollRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            RagdollRigidbody.isKinematic = !pValue;
        }

       
    }

    public virtual void Push(Vector3 pDir, float pForce)
    {
        InitializeRagdoll(pDir * pForce);
    }
    public virtual void Push(Vector3 pVelocity)
    {
        //Push(pVelocity.normalized, pVelocity.magnitude);
        InitializeRagdoll(pVelocity);
    }

    private IEnumerator LateSetRagdoll()
    {
        yield return new WaitForSeconds(0.1f);
        IsRagdolling = true;
    }
   
    private void OnCollisionEnter(Collision col)
    {
        if (this is PlayerHookable)
        {
            var playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                if (playerController.CurrentState == PlayerController.State.Ragdoll)
                {
                    if (playerController.LandingParticleEffectPrefab != null)
                    {
                        var ps = Instantiate(playerController.LandingParticleEffectPrefab, col.contacts[0].point, Quaternion.identity);
                        Destroy(ps, 3f);
                    }
                }
            }
        }
    }

    //private void OnDestroy()
    //{
    //    if (AttachedUsable != null)
    //    {
    //        //AttachedUsable?.UsableController?.ResetToStandardHook();
    //        AttachedUsable.transform.SetParent(null);
    //    }
    //}

    //private void OnDisable()
    //{
    //    if (AttachedUsable != null)
    //    {
    //        //AttachedUsable?.UsableController?.ResetToStandardHook();
    //        AttachedUsable.transform.SetParent(null);
    //        //AttachedUsable.transform.parent = null;
    //    }
    //}
}