using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AimModule : MonoBehaviour 
{
    public Vector3 TargetPos { get; protected set; }
    [Tooltip("Seconds until full charge"), SerializeField]
    protected float ChargeTime = 1f;
    public float MaxDistance = 15f;
    public float MinDistance = 1f;
    protected float ChargeAmount;
    protected float ChargeTimer;
    [HideInInspector]
    public float ThrowDistance;
    public GameObject Aim;
    protected Usable Usable;
    [SerializeField]
    protected Vector3 AimOffset;
    private float ChargeSpeedMultiplier;

    public Vector3 ForwardDirection
    {
        get
        {
            return Usable.Owner.transform.forward;
        }
    }

    public virtual void Initialize(Usable pUsable)
    {
        Usable = GetComponent<Usable>();
        Aim.transform.SetParent(Usable.Owner.transform);
        Aim.transform.localScale = new Vector3(1, 1, 1);
        Aim.SetActive(false);
    }

    public virtual void InitializeWindup()
    {
        ChargeTimer = 0f;
        SetupAim();
        TargetPos = Vector3.zero;
    }

    public virtual void UpdateWindup()
    {
        ChargeTimer += Time.deltaTime;
        if (ChargeTime != 0)
        {
            if (ThrowDistance < MinDistance)
            {
                ChargeSpeedMultiplier = 1.03f;
            }
            else
            {
                ChargeSpeedMultiplier = 1;
            }

            ChargeTimer *= ChargeSpeedMultiplier;
            //ChargeAmount = Mathf.Lerp((1 / MaxDistance) * MinDistance, 1, Mathf.Clamp01(ChargeTimer / ChargeTime));
            ChargeAmount = Mathf.Lerp(0, 1, Mathf.Clamp01(ChargeTimer / ChargeTime));
        }
        else
        {
            ChargeAmount = 1;
        }
        
        ThrowDistance = MaxDistance * ChargeAmount;
    }

    protected virtual void SetupAim()
    {
        Aim.transform.SetParent(Usable.Owner.transform);
        Aim.transform.localPosition = AimOffset;
        //Aim.transform.rotation = Quaternion.identity;
        Aim.SetActive(true);
        //var rend = Aim.GetComponentInChildren<Renderer>();
        //if (rend != null)
        //{
        //    rend.enabled = true;
        //}
    }

    public virtual void ResetAim()
    {
        Aim.transform.SetParent(Usable.Owner.transform);
        Aim.transform.localPosition = AimOffset;
        Aim.SetActive(false);
       // Aim.GetComponentInChildren<Renderer>().enabled = false;
    }

    public virtual void Interrupt()
    {
        ResetAim();
    }
}
