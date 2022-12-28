using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var hook = other.GetComponent<Hook>();
        if (hook != null)
        {
            return;
        }
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.transform.SetParent(null);
        }
    }
}
