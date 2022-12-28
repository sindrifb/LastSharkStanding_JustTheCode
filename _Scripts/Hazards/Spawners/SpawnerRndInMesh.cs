using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerRndInMesh : SpawnerBase
{
    protected override Vector3 GetEndPos()
    {
        Vector3 newRndPos;

        do
        {
            newRndPos = HazardManager.Instance.GetRndPointOnMesh();

        } while (!HazardManager.Instance.CheckIfPositionIsFar(Hazard, newRndPos));

        return newRndPos;
    }    
}
