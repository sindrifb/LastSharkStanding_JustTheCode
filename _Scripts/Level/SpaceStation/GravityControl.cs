using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        var RB = other.GetComponent<Rigidbody>();
        var PC = other.GetComponent<PlayerController>();
        if (PC != null && RB != null && RB.useGravity)
        {
            if (PC != null)
            {
                //SpaceStationGameRules.Instance?.KillPlayer(PC);
                //FreezeVisual(PC.transform);
                //if (PC.CurrentState != PlayerController.State.Ragdoll)
                //{
                //    PC.Push((other.transform.position - new Vector3(0, 0, -5)).normalized, 30);
                //}
            }
            SpaceStationGameRules.Instance?.KillPlayer(PC);
            FreezeVisual(PC.transform);
            PC.GetComponent<Hookable>().DontGetUp = true;
            RB.useGravity = false;
            RB.angularDrag = .1f;
            RB.drag = 2f;
        }
    }

    private void FreezeVisual(Transform pTransform)
    {
        var renderers = pTransform.GetComponentsInChildren<Renderer>();
        foreach (var item in renderers)
        {
            if (item.material.HasProperty("_Color"))
            {
                item.material.color = item.material.color + Color.grey + Color.blue;
            } 
        }
    }
}
