using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulse : MonoBehaviour
{
    public float Speed = 20f;
    public float Force = 30f;
    public float LifeTime = 3f;

    void Start()
    {
        Destroy(this.gameObject, LifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * Speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider col)
    {
        var hookedObject = col.GetComponentInChildren<Hookable>();

        if (hookedObject != null && hookedObject.IsAvailable)
        {
            Vector3 dir = (col.transform.position - transform.position).normalized / 4f;
            if (hookedObject is PlayerHookable)
            {
                //hookedObject.GetComponent<AudioController>().PlayAnchorhit();
                hookedObject.Push((transform.forward + (Vector3.up) + dir).normalized, Force);
            }
            //else
            //{
            //    hookedObject.Push((transform.forward + (Vector3.up / 2f) + dir).normalized, 5f);
            //}
        }
    }
}
