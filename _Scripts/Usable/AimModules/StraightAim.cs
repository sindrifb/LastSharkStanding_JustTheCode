using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightAim : AimModule 
{

    public override void InitializeWindup()
    {
        base.InitializeWindup();
    }

    public override void ResetAim()
    {
        base.ResetAim();
    }

    //public override void UpdateWindup()
    //{
        
    //}

    protected override void SetupAim()
    {
        base.SetupAim();
        Aim.transform.rotation = Quaternion.LookRotation(Usable.Owner.transform.forward);
    }
}
