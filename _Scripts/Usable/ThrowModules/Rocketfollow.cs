using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rocketfollow : ThrowModule
{
    public GameObject ParticleEffect;
    PlayerController Target;

    public override void Initialize(Usable pUsable)
    {
        base.Initialize(pUsable);
        Usable.IgnoreOwnerCollision();
    }

    public override void Throw()
    {
        base.Throw();
    }

    protected override void OverridableThrow()
    {

        base.OverridableThrow();

        Target = FindObjectsOfType<PlayerController>().FirstOrDefault(a => a != Usable.PlayerController && !a.GetComponent<Death>().IsDead);
        transform.parent = null;
        Usable.transform.position = Usable.Owner.transform.position;
        Usable.transform.rotation = Quaternion.LookRotation(Usable.Owner.transform.forward);
        //Usable.transform.rotation = Quaternion.LookRotation(Usable.transform.forward);

        Usable.Rigidbody.isKinematic = false;
        Usable.Rigidbody.drag = 0;
        Usable.Rigidbody.angularDrag = 0;
        Usable.Rigidbody.angularVelocity = Vector3.zero;
        Usable.Rigidbody.AddForce((Vector3.up + transform.forward/2).normalized * 25,ForceMode.Impulse);

        Usable.UsableController.ResetToStandardHook();
        following = true;
        GameManager.Instance?.SpawnedObjects.Add(Usable.gameObject);
        ParticleEffect.SetActive(true);
        Usable.StopIgnoringColliders();
    }

    bool following;
    float timer;
    float disruptTimer;
    Vector3 rndDir;
    protected override void IndependentUpdate()
    {

        base.IndependentUpdate();
        if (Usable != null && Usable.Rigidbody.velocity != Vector3.zero)
        {
            Usable.transform.rotation = Quaternion.LookRotation(-Usable.Rigidbody.velocity);
        }
        
        if (following)
        {

            timer += Time.deltaTime;
            if (timer > .6f)
            {
                if (Usable.Rigidbody.useGravity)
                {
                    Usable.Rigidbody.useGravity = false;
                }
                var dir = transform.forward;
                if (Target != null)
                {
                   dir  = (Target.transform.position - transform.position).normalized;
                }
                
                rndDir = transform.rotation * rndDir;
                Usable.Rigidbody.AddForce((dir * 40) + rndDir * 10);
                

                disruptTimer += Time.deltaTime;
                if (disruptTimer > .3f)
                {
                    rndDir = Random.onUnitSphere;
                    rndDir.z = 0;
                }
            }
            else
            {
                transform.localScale = Vector3.one * Mathf.MoveTowards(transform.localScale.x,2,Time.deltaTime * 2);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            following = false;
        }
    }
}
