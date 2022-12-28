using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EffectModule : MonoBehaviour 
{
    [SerializeField]
    protected LayerMask GroundLayer;
    [SerializeField]
    protected LayerMask SharkLayer;
    [SerializeField]
    protected LayerMask DeathZoneLayer;
    protected Usable Usable;
    protected GameObject UsableOwner;
    protected PlayerController OwnerPlayerController;
    bool initialized;

    public bool Thrown { get; internal set; }

    public virtual void Initialize(Usable pUsable)
    {
        Usable = pUsable;
        UsableOwner = pUsable.Owner;
        OwnerPlayerController = UsableOwner.GetComponent<PlayerController>();
        initialized = true;
    }

    private void Update()
    {
        if (!initialized || !Thrown)
        {
            return;
        }
        UpdateEffect();
    }

    protected virtual void UpdateEffect() { }

    public virtual void OverridableOnTriggerEnter(Collider col) { }

    public virtual void Interrupt() { }

    //public virtual void GroundHit()
    //{

    //}

    //public virtual void HookableHit(Hookable pHookable)
    //{

    //}

    public virtual void StealFromHook() { }
}
