using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderParticleEffectController : MonoBehaviour
{
    public Rigidbody BoulderRigidbody;

    void Update()
    {
        var dir = BoulderRigidbody.velocity;
        if (dir == Vector3.zero)
        {
            //disable particle effect here!
            return;
        }
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);
    }
}
