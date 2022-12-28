using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FromDownHazard : BasicHazard
{
    protected List<SphereCollider> Collider;

    protected override void Start()
    {
        base.Start();
        Hazard.WarningExplosionVisualEvent.Stop();
        Collider = gameObject.GetComponents<SphereCollider>().ToList<SphereCollider>();
        foreach (SphereCollider c in Collider)
        {
            if (c.isTrigger) c.radius = Hazard.TriggeringRadio * 2; // Puts the radio in local scale. Should be the same but idk why its half
        }
    }
    public override void Initializing()
    {
        base.Initializing();
        Hazard.OrigPos = Hazard.EndPos;
        transform.position = Hazard.EndPos;
    }
    public override void OnEnable()
    {
        base.OnEnable();
        Landed = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerHit = collision.collider.transform.root.GetComponentInChildren<PlayerController>();
        //print("Player " + PlayerHit);

        if (PlayerHit != null)
        {
            //print("DONT DO ANYTHING");
            ExplodeHazard();
            if (Hazard.DiesOneHit)
                KillHazard();
        }
    }
}
