using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicCCDSwapper : MonoBehaviour
{
    Rigidbody Rigidbody;
    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }
    void LateUpdate()
    {
        if (Rigidbody != null)
        {
            if (Rigidbody.isKinematic && Rigidbody.collisionDetectionMode != CollisionDetectionMode.Discrete)
            {
                Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
            else if (!Rigidbody.isKinematic && Rigidbody.collisionDetectionMode != CollisionDetectionMode.ContinuousDynamic)
            {
                Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
        }
    }
}
