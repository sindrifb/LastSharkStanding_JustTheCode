using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileThrow : ThrowModule
{
    [SerializeField]
    private Vector3 ProjectileForward = Vector3.forward;
    [SerializeField]
    private float angle = 10;
    [SerializeField]
    private float Range = 15;
    private ProjectileData ProjectileData;
    public override void Initialize(Usable pUsable)
    {
        base.Initialize(pUsable);
        var ProjectileAim = GetComponent<ProjectileAim>();
        if (ProjectileAim != null)
        {
            ProjectileData = ProjectileAim.ProjectileData;
        }
        Usable.IgnoreOwnerCollision();
    }

    public override void Throw()
    {
        base.Throw();
    }

    protected override void OverridableThrow()
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HookSound.AnchorThrow);
        base.OverridableThrow();
        if (/*ProjectileData == null*/true)
        {
            ProjectileData = new ProjectileData(Usable.Owner.transform.position, Usable.Owner.transform.position+ Usable.Owner.transform.forward * Range,angle,0,0);
        }
        transform.parent = null;
        Usable.transform.position = Usable.Owner.transform.position;
        Usable.transform.rotation = Quaternion.LookRotation(Usable.Owner.transform.forward);
        //Usable.transform.rotation = Quaternion.LookRotation(Usable.transform.forward);
        
        Usable.Rigidbody.isKinematic = false;
        Usable.Rigidbody.drag = 0;
        Usable.Rigidbody.angularDrag = 0;
        Usable.Rigidbody.angularVelocity = Vector3.zero;
        Usable.Rigidbody.velocity = ProjectileData.Velocity;

        Usable.IsActive = true;

        Usable.UsableController.ResetToStandardHook();

        GameManager.Instance?.SpawnedObjects.Add(Usable.gameObject);
        Usable.StopIgnoringColliders();
    }
    protected override void IndependentUpdate()
    {
        base.IndependentUpdate();
        if (Usable?.Rigidbody?.velocity.magnitude < 2f)
        {
            Usable.IsActive = false;
        }
        //else if (Usable != null)
        //{
        //    Usable.IsActive = true;
        //}
    }
}
