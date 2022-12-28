using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirVentPush : MonoBehaviour
{
    public Vector3 Direction;
    public float Force;

    void OnTriggerEnter(Collider col)
    {
        var hookable = col.GetComponent<Hookable>();
        if (hookable != null)
        {
            hookable.Push(Direction.normalized, Force);
        }
    }
}
