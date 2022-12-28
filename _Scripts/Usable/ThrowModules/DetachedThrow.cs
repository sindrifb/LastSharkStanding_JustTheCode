using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetachedThrow : ThrowModule
{
    [SerializeField]
    private float MoveSpeed;
    private bool thrown;

    public override void Initialize(Usable pUsable)
    {
        base.Initialize(pUsable);

        Physics.IgnoreCollision(Usable.Owner.GetComponent<Collider>(), GetComponent<Collider>());
    }

    protected override void OverridableThrow()
    {
        base.OverridableThrow();

        Usable.PlayerController.ChangeState(PlayerController.State.Idle);
        thrown = true;
        Usable.transform.forward = Usable.AimModule.ForwardDirection;
    }

    protected override void IndependentUpdate()
    {
        base.IndependentUpdate();

        if (thrown)
        {
            Usable.Rigidbody.MovePosition(transform.position + transform.forward * Time.deltaTime * MoveSpeed);
        }
    }
}
