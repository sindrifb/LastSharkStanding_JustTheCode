using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DrawBridgeThrower : MonoBehaviour
{
    public List<PlayerController> PlayersOnBridge = new List<PlayerController>();
    public float DistanceMultiplier = 15;
    private Vector3 startRot;

    public void ActivateBridge(Vector3 pPushDir)
    { 
        foreach (var player in PlayersOnBridge)
        {
            var dist = Vector3.Distance(transform.position, player.transform.position);
            player.Hookable.Push(pPushDir, Mathf.Clamp(dist * DistanceMultiplier, 0f, 100f));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            PlayersOnBridge.Add(player);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            PlayersOnBridge.Remove(player);
        }
    }

    public void CheckIfOwnerOnBridge(Usable pUsable)
    {
        var player = pUsable.Owner.GetComponent<PlayerController>();
        if (!PlayersOnBridge.Contains(player))
        {
            //*****not sure why we need this****
            //if (pUsable is StandardHook)
            //{
            //    pUsable.OnFinishedThrow();
            //}
        }
    }
}
