using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerFixedPos : SpawnerBase
{

    public Transform TargetPoint;

    protected override Vector3 GetEndPos()
    {
        if (TargetPoint != null)
            return TargetPoint.position;
        else
            return transform.position;
    }
}
