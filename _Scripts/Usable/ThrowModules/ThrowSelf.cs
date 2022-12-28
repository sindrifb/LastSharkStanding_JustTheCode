using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowSelf : ThrowModule
{
    public GameObject ParticleSystemExplode;
    Transform OwnerTrans;
    Rigidbody OwnerRB;
    public override void Initialize(Usable pUsable)
    {
        base.Initialize(pUsable);
        Usable.IgnoreOwnerCollision();
        OwnerTrans = Usable.Owner.transform;
        OwnerRB = Usable.PlayerController.Rigidbody;
    }

    public override void Throw()
    {
        base.Throw();
    }
    bool thrown;
    bool done;
    protected override void OverridableThrow()
    {
        base.OverridableThrow();
        transform.parent = null;
        Usable.UsableController.ResetToStandardHook();
        
        Usable.PlayerController.Hookable.Push(Usable.Owner.transform.forward + (Vector3.up/6), 80);
        thrown = true;
        
    }

    protected override void IndependentUpdate()
    {
        base.IndependentUpdate();
        if (!done && OwnerTrans != null)
        {
            transform.position = OwnerTrans.position;
            if (thrown && OwnerRB.isKinematic)
            {
                transform.position = transform.position + Vector3.up;
                Usable.Rigidbody.isKinematic = false;
                done = true;
                gameObject.AddComponent<SphereCollider>();
                StartCoroutine(ExplodeAndDestroy());
            }
        }

        if (done)
        {
            Usable.Rigidbody.AddForce(transform.up * 60);
        }
    }

    IEnumerator ExplodeAndDestroy()
    {
        yield return new WaitForSeconds(2f);
        if (ParticleSystemExplode != null)
        {
            var ps = Instantiate(ParticleSystemExplode, transform.position, Quaternion.identity);
            Destroy(ps, 2f);
        }
        Destroy(gameObject);
    }
}
