using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerRndBetweenPos : SpawnerBase
{

    [Tooltip("Father with all the target positions as children, if null, fill the array below")]
    public Transform TargetPosGroup;
    [Tooltip("Array of target positions, if blank, fill the group")]
    public Transform[] TargetPos;

    protected override void Start()
    {
        base.Start();
        if (TargetPosGroup != null)
        {
            // If done with foreach or getComponentsInChildren<T>() it also gets the parent component
            TargetPos = new Transform[TargetPosGroup.childCount];

            for (int i = 0; i < TargetPosGroup.childCount; i++)
            {
                TargetPos[i] = TargetPosGroup.GetChild(i).transform;
            }
        }
    }
    protected override Vector3 GetEndPos()
    {
        if (TargetPos.Length > 0)
        {
            int aux = Random.Range(0, TargetPos.Length);
            return (TargetPos[aux].position);
        }
        else
            return transform.position;
    }
}
