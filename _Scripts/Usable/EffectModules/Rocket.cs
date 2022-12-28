using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rocket : EffectModule
{
    public GameObject ParticleEffect;
    private PlayerController Other;
    private GameObject DetachedOwner;
    private bool disabled;
    float timer;
    public override void Initialize(Usable pUsable)
    {
        base.Initialize(pUsable);
        DetachedOwner = Usable.Owner;
    }
    
    protected override void UpdateEffect()
    {
        base.UpdateEffect();
        if (Other != null)
        {
            Other.transform.position = transform.position;

            Usable.Rigidbody.AddForce((Vector3.up + transform.position.normalized).normalized * 40);
            Usable.transform.rotation = Quaternion.LookRotation(-Usable.Rigidbody.velocity);

            timer += Time.deltaTime;

            if (timer > 1.5f)
            {
                if (ParticleEffect != null)
                {
                    var ps = Instantiate(ParticleEffect, transform.position, Quaternion.identity);
                    Destroy(ps, 2f);
                }
                Other.GetComponent<Rigidbody>().velocity = Usable.Rigidbody.velocity;
                Destroy(gameObject);
            }
        }
    }
    //no colliders when stuck
    void DisableAllColliders()
    {
        GetComponentsInChildren<Collider>().ToList().ForEach(a => a.enabled = false);
    }
    public void OnTriggerEnter(Collider col)
    {
        if (col.isTrigger || disabled)
        {
            return;
        }

        var pc = col.GetComponent<PlayerController>();
        if (pc != null)
        {
            Other = pc;
            DisableAllColliders();
        }
    }
}
