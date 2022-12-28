using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CatapultLanding : MonoBehaviour
{
    public GameObject HeavyLandingParticles;
    public List<PlayerController> Players = new List<PlayerController>();
    //public GameObject PlayerThrown;
    public List<GameObject> PlayersThrown = new List<GameObject>();
    public float PushForce;
    private Vector3 PushDir;

    private void PushPlayers()
    {
        foreach (var playerThrown in PlayersThrown)
        {
            if (HeavyLandingParticles != null)
            {
                Instantiate(HeavyLandingParticles, playerThrown.transform.position, Quaternion.identity);
            }
            foreach (var player in Players)
            {
                if (player != playerThrown)
                {
                    CalculatePushDirection(player.transform, playerThrown.transform);
                    player.Hookable.Push(PushDir.normalized, PushForce);
                }
            }
        }
        //if (HeavyLandingParticles != null)
        //{
        //    Instantiate(HeavyLandingParticles, PlayerThrown.transform.position, Quaternion.identity);
        //}
        //foreach (var player in Players)
        //{
        //    CalculatePushDirection(player.transform, PlayerThrown.transform);
        //    player.Push(PushDir.normalized, PushForce);
        //}

        //PlayerThrown = null;
        PlayersThrown.Clear();
    }

    private void CalculatePushDirection(Transform pPlayerPushed, Transform pPlayerThrown)
    {
        PushDir = pPlayerPushed.position - pPlayerThrown.position;
        Debug.Log(Vector3.Distance(pPlayerThrown.position, pPlayerPushed.position));
        PushForce = Mathf.Clamp(50 / Vector3.Distance(pPlayerThrown.position, pPlayerPushed.position),0.1f,50);
        Debug.Log("PushForce = " + PushForce);
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            //if (PlayerThrown != null)
            //{
            //    PushPlayers();
            //}
            Players.Add(player);
            if (PlayersThrown.Contains(player.gameObject))
            {
                PushPlayers();
                PlayersThrown.Remove(player.gameObject);
            }
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Players.Remove(player);
        }
    }
}
