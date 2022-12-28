using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachToSelf : ThrowModule
{
    public GameObject DestroyParticleEffect;
    Transform OwnerTrans;
    Rigidbody OwnerRB;
    CharacterController CharacterController;
    public override void Initialize(Usable pUsable)
    {
        base.Initialize(pUsable);
        Usable.IgnoreOwnerCollision();
        OwnerTrans = Usable.Owner.transform;
        OwnerRB = Usable.PlayerController.Rigidbody;
        CharacterController = OwnerTrans.GetComponent<CharacterController>();
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
        Usable.Rigidbody.angularVelocity = Vector3.up * 40;
        OwnerRB.isKinematic = false;
      
        thrown = true;

        StartCoroutine(Release());

    }

    protected override void IndependentUpdate()
    {
        base.IndependentUpdate();
        if (!done && OwnerTrans != null)
        {
            if (Usable.Rigidbody.isKinematic)
            {
                Usable.Rigidbody.isKinematic = false;
            }
            transform.position = OwnerTrans.position;
            if (thrown)
            {
                Usable.Rigidbody.AddTorque(Vector3.up * 200 * Time.deltaTime);
                if (Usable.PlayerController.MovementController.Direction != Vector3.zero)
                {
                    Usable.PlayerController.AddForce(-Usable.PlayerController.MovementController.Direction/2f);
                }
            }
        }

        if (done)
        {
            Usable.Rigidbody.AddForce(transform.up * 10);
        }
    }

    IEnumerator Release()
    {
        yield return new WaitForSeconds(4f);
        done = true;
        transform.position = transform.position + (transform.up * 5);
        gameObject.AddComponent<SphereCollider>();
        Usable.IgnoreOwnerCollision();
        yield return new WaitForSeconds(2f);
        if (DestroyParticleEffect != null)
        {
            var ps = Instantiate(DestroyParticleEffect,transform.position,Quaternion.identity);
            Destroy(ps, 2f);
        }
        Destroy(gameObject);
    }
}
