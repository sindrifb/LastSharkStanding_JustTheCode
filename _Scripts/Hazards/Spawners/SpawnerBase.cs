using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpawnerBase : MonoBehaviour
{

    public KeyCode Key; // for debugging and testing

    protected Vector3 EndPos;

    public HazardType Hazard;

    [Tooltip("Father with all the spawning positions as children, if null, fill the array below")]
    public Transform StartingPosGroup;
    [Tooltip("Array of spawning positions, if blank, fill the group")]
    public Transform[] StartingPos;

    protected GameObject SpawnedHazard;

    protected virtual void Start()
    {
        if (StartingPosGroup == null && StartingPos.Length == 0)
        {
            StartingPos = new Transform[1];
            StartingPos[0] = this.transform;
        }

        if (StartingPosGroup != null)
        {
            // If done with foreach or getComponentsInChildren<T>() it also gets the parent component
            StartingPos = new Transform[StartingPosGroup.childCount];

            for (int i = 0; i < StartingPosGroup.childCount; i++)
            {
                StartingPos[i] = StartingPosGroup.GetChild(i).transform;
            }
        }
    }

    protected abstract Vector3 GetEndPos();

    protected virtual Vector3 GetOrigPos()
    {
        Vector3 tempOrigPos = (StartingPosGroup ? StartingPosGroup.position : Vector3.zero);

        if (StartingPos.Length > 0)
        {
            int aux = Random.Range(0, StartingPos.Length);
            tempOrigPos = (StartingPos[aux].position);
        }
        return tempOrigPos;
    }

    public virtual void SpawnObject()
    {
        Vector3 endPos, origPos;

        origPos = GetOrigPos();

        endPos = GetEndPos();

        HazardManager.Instance.InstantiateHazard(Hazard, origPos, endPos);
    }

    private void Update()
    {
        if (HazardManager.Instance.DEBUGGING && Input.GetKeyDown(Key))
            SpawnObject();
    }
}
