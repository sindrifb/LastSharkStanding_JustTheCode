using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FromSideHazard : BasicHazard
{
    protected CharacterController c;
    protected Vector3 Direction;
    private List<PlayerController> InTrigger = new List<PlayerController>();

    protected override void Update()
    {
        base.Update();

        if (InTrigger.Any())
        {
            foreach (PlayerController pc in InTrigger)
            {
                if (pc == null)
                    InTrigger.Remove(pc);

                else
                    HitPlayer(pc);
            }
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        c = GetComponent<CharacterController>();
        // FOR NOW IT DESTROYS ONLY WITH TIME
        TriggerExplosionInTime(Hazard.WaitToDie);
    }
    protected override void Movement()
    {

            c?.Move(Direction * Hazard.MovementMagnitude * Time.deltaTime);       
    }

    public override void TriggerHazBehaviour()
    {
        base.TriggerHazBehaviour();

        transform.LookAt(Hazard.EndPos);
        Direction = transform.forward;
    }

    public override void OnTriggerEnter(Collider pOther)
    {
        var player = pOther.GetComponent<PlayerController>();
        if (player != null)
        {
            InTrigger.Add(player);
        }
    }

    public void OnTriggerExit(Collider pOther)
    {
        var player = pOther.GetComponent<PlayerController>();
        if (InTrigger.Contains(player))
        {
            InTrigger.Remove(player);
        }
    }

    //public virtual void OnTriggerStay(Collider pOthers)
    //{
    //    if (pOthers.GetComponent<PlayerController>())
    //    {
    //        PlayerHit = pOthers.GetComponent<PlayerController>();
    //        HitPlayer(PlayerHit);
    //    }
    //}

    protected override void HitPlayer(PlayerController pPlayer)
    {
        Vector3 aux = Direction + Vector3.up;
        aux.Normalize();
        pPlayer.Hookable.Push( aux, Hazard.PushingForce);
    }  

    // would be with line equations 
    protected void GetLimitLine()
    {
        //Vector2 orthogonalDir = Vector2.Perpendicular(new Vector2(Direction.x, Direction.z));
        //orthogonalDir.Normalize();

        //orthogonalDir += new Vector2(Hazard.EndPos.x, Hazard.EndPos.z);
    }
}
