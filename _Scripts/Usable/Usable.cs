using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Animations;

public class Usable : MonoBehaviour
{
    public bool IsActive = true;
    public GameObject HitParticleSystemPrefab;
    [SerializeField]
    public float MaxProjectileDistance { get; private set; }
    [HideInInspector]
    public PlayerController PlayerController;
    [HideInInspector]
    public  UsableController UsableController;
    [HideInInspector]
    public ParentConstraint ParentConstraint;
    [HideInInspector]
    public GameObject Owner;
    private List<Renderer> MeshRenderers;
    private List<Collider> Colliders;
    private Coroutine ThrowCoroutine;
    [SerializeField]
    private LayerMask GroundLayer;
    public float HookFlingAmp = 1;
    public AimModule AimModule { get; private set; }
    public ThrowModule ThrowModule { get; private set; }
    public EffectModule EffectModule { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public string IdleAnimParameter;
    public GameObject DisplayObject;
    bool initialized;
    public GameObject ThrownParticlesParent;
    public GameObject OnDestroyParticlesPrefab;
    public UsableType Type;
    public enum UsableType
    {
        StandardHook,
        Harpoon,
        SpaceHarpoon,
        Anchor,
        ShieldDisc,
        WheelDisc,
        SpaceDisc,
        Boomerang,
        Bubble,
        Dynamite,
        TempleImplosion,
        SpaceImplosion
    }

    public void Initialize(PlayerController pPlayerController, UsableController pUsableController)
    {
        if (initialized)
        {
            return;
        }
        initialized = true;
        ParentConstraint = GetComponent<ParentConstraint>();
        Rigidbody = GetComponentInChildren<Rigidbody>(true);
        PlayerController = pPlayerController;
        Owner = pPlayerController.gameObject;
        UsableController = pUsableController;
        AimModule = GetComponent<AimModule>();
        EffectModule = GetComponent<EffectModule>();
        ThrowModule = GetComponent<ThrowModule>();
        AimModule.Initialize(this);
        MeshRenderers = GetComponentsInChildren<Renderer>().ToList();
        Colliders = GetComponentsInChildren<Collider>(true).ToList();
        EffectModule.Initialize(this);
        ThrowModule.Initialize(this);
       
        transform.position = Owner.transform.position;
        SetUsableRenderAndColliderActive(false);
        var material = AimModule.Aim.GetComponentInChildren<Renderer>()?.material;
        var c = PlayerController.PlayerColor;
        material.color = c;
        //material.SetColor("_EmissionColor", new Vector4(c.r, c.g, c.b, 0) * 0.05f);

        if (ThrownParticlesParent != null)
        {
            ThrownParticlesParent.SetActive(false);

        }
    }

    public void IgnoreOwnerCollision(Transform pTransform = null)
    {
        if (pTransform == null)
        {
            pTransform = Owner.transform;
        }
        var Ownercolliders = pTransform.GetComponentsInChildren<Collider>();
        foreach (var col in Ownercolliders)
        {
            foreach (var item in Colliders)
            {
                Physics.IgnoreCollision(item, col);
            }
        }
    }

    public void StopIgnoringColliders(float pTime = .5f)
    {
        StartCoroutine(StopIgnoringOwnerCollision(pTime));
    }

    private IEnumerator StopIgnoringOwnerCollision(float pTime = .5f)
    {
        yield return new WaitForSeconds(pTime);
        if (Owner == null)
        {
            yield break;
        }
        var Ownercolliders = Owner.GetComponentsInChildren<Collider>();
        foreach (var col in Ownercolliders)
        {
            foreach (var item in Colliders)
            {
                if (item !=null && col != null)
                {
                    Physics.IgnoreCollision(item, col, false);
                }
            }
        }
    }

    public void InitializeWindUp()
    {
        if (AimModule == null)
        {
            Debug.Log("Usable is missing aim module");
            return;
        }
        AimModule?.InitializeWindup();
    }

    public void UpdateWindUp()
    {
        if (AimModule == null)
        {
            Debug.Log("Usable is missing aim module");
            return;
        }
        AimModule?.UpdateWindup();
    }

    public void Throw()
    {
        if (ThrowModule == null)
        {
            Debug.Log("Usable is missing throw module");
            return;
        }
        ThrowCoroutine = StartCoroutine(ThrowWhenMinDistReached());
        //ThrowModule.Throw();
        //if (ThrownParticlesParent != null)
        //{
        //    ThrownParticlesParent.SetActive(true);
        //}
    }

    private IEnumerator ThrowWhenMinDistReached()
    {
        if (AimModule.ThrowDistance < AimModule.MinDistance)
        {
            yield return new WaitUntil(() => AimModule.ThrowDistance >= AimModule.MinDistance);
        }

        if (PlayerController.CurrentState == PlayerController.State.WindUp)
        {
            ThrowModule.Throw();
            
            if (ThrownParticlesParent != null)
            {
                ThrownParticlesParent.SetActive(true);
            }
            PlayerController.ChangeState(PlayerController.State.Throwing);
        }
    }

    public void SetUsableRenderAndColliderActive(bool pValue, bool pOnlyCollider = false)
    {
        if (!pOnlyCollider)
        {
            MeshRenderers.ForEach(a => a.enabled = pValue);
        }

        Colliders.ForEach(a => a.enabled = pValue);
    }

    protected void OnTriggerEnter(Collider col)
    {
        if (EffectModule != null)
        {
            EffectModule.OverridableOnTriggerEnter(col);
        }
    }

    public void ResetStandardHook()
    {
        if (ParentConstraint != null)
        {
            ParentConstraint.constraintActive = true;
        }
        AimModule.ResetAim();
        transform.position = Owner.transform.position;
        transform.SetParent(Owner.transform);
        SetUsableRenderAndColliderActive(false);
        UsableController.HookOnFishingRod?.SetActive(true);
    }

    public void Interrupt(PlayerController.State pCurrentState)
    {
        switch (pCurrentState)
        {
            case PlayerController.State.WindUp:
                if (ThrowCoroutine != null)
                {
                    StopCoroutine(ThrowCoroutine);
                }
                AimModule.Interrupt();
                PlayerController.ChangeState(PlayerController.State.Idle);
                break;
            case PlayerController.State.Throwing:
                EffectModule.Interrupt();
                AimModule.Interrupt();
                ThrowModule.Interrupt();
                break;
            case PlayerController.State.Reeling:
                EffectModule.Interrupt();
                break;
            case PlayerController.State.Hooked:
                if (UsableController.CurrentUsable == UsableController.StandardHook)
                {
                    EffectModule.Interrupt();
                }
                break;
            default:
                break;
        }

        //if (this == UsableController.StandardHook)
        //{
        //    ResetStandardHook();
        //}
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
