using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarpoonStraightThrow : ThrowModule
{
    [SerializeField]
    private Vector3 ProjectileForward = Vector3.forward;
    [SerializeField]
    private float angle = 10;
    private bool Thrown;
    private ProjectileData ProjectileData;


    private StraightAim StraightAim;

    public override void Initialize(Usable pUsable)
    {
        base.Initialize(pUsable);

        StraightAim = pUsable.AimModule as StraightAim;

        Usable.IgnoreOwnerCollision();
    }

    protected override void IndependentUpdate()
    {
        base.IndependentUpdate();

        if (Thrown)
        {
            //do throw stuff
            if (Usable.Owner != null)
            {
                if (Vector3.Distance(Usable.Owner.transform.position, transform.position) >= StraightAim.MaxDistance || Usable.Rigidbody.isKinematic)
                {
                    ThrowDone();
                }
            }
            else
            {
                ThrowDone();
            }
            
        }
    }

    protected override void OverridableThrow()
    {
        if (!Thrown)
        {
            // stuff happening once before throwing
            ProjectileData = new ProjectileData(Usable.Owner.transform.position, Usable.Owner.transform.position + Usable.Owner.transform.forward * Usable.AimModule.MaxDistance, angle, 0, 0);

            transform.parent = null;
            Usable.transform.position = Usable.Owner.transform.position;
            Usable.transform.rotation = Quaternion.LookRotation(Usable.Owner.transform.forward);

            Usable.Rigidbody.isKinematic = false;
            Usable.Rigidbody.drag = 0;
            Usable.Rigidbody.angularDrag = 0;
            Usable.Rigidbody.angularVelocity = Vector3.zero;
            Usable.Rigidbody.velocity = ProjectileData.Velocity;
            Usable.SetUsableRenderAndColliderActive(true);
            Usable.DisplayObject.SetActive(false);
            

            //OwnerPlayerController.ChangeState(PlayerController.State.Throwing);
        }
        
        Thrown = true;
    }

    public override void ThrowDone()
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HookSound.HarpoonThrow);
        Usable.IsActive = false;
        Thrown = false;
        //if (OwnerPlayerController.CurrentState == PlayerController.State.Reeling)
        //{
        //    base.ThrowDone();
        //}
        /*else*/ if (OwnerPlayerController.CurrentState == PlayerController.State.Throwing)
        {
            Usable.UsableController.ResetToStandardHook();
            //OwnerPlayerController.ChangeState(PlayerController.State.Idle);
        }

        //else if (OwnerPlayerController.CurrentState == PlayerController.State.Reeling)
        //{
        //    OwnerPlayerController.ChangeState(PlayerController.State.Idle);
        //}
    }

    public override void Interrupt()
    {
        base.Interrupt();

        Thrown = false;
        Usable.UsableController.ResetToStandardHook();
    }
}
