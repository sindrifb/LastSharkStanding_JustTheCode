using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AirlockVac : MonoBehaviour
{
    
    public float forceMultiplier = 1;
    public float DoorIndex;
    private List<PlayerAndForce> Players = new List<PlayerAndForce>();

    private AirlockOpener AirlockButton;

    public List<AirLockDoor> Doors = new List<AirLockDoor>();
    
    void Start()
    {
        AirlockButton = GetComponentInChildren<AirlockOpener>(true);
    }
    
    void Update()
    {
        MoveDoors();
        //return;
        List<PlayerController> playersToRemove = new List<PlayerController>();
        foreach (var player in Players)
        {
            if (player == null||player.PlayerController == null )
            {
                continue;
            }
            


            var dir = (((transform.position - new Vector3(0,transform.position.y,0)) + (-transform.forward)) - (player.PlayerController.transform.position - new Vector3(0, player.PlayerController.transform.position.y, 0)));
            float dirLength = dir.magnitude;
            float speed = Mathf.Clamp(11 - dirLength, 2.6f, 10);
            //Debug.DrawRay(transform.position,-dir,Color.red);
            //Debug.DrawRay(player.PlayerController.transform.position,-dir.normalized * .7f,Color.green);
            if (AirlockButton.Openness < .1f)
            {
                //SetConstantPushForce(player,Vector3.zero);
                continue;
            }
            if (/*dir.sqrMagnitude < (.7f*.7f)||*/Vector3.Dot(-transform.forward,-dir) <= 0)
            {
                //print("WAOAOAOAO");
                //RemoveFromVac(player);
               
                //RemoveFromVac(player.PlayerController);
                player.PlayerController.Hookable.Push(dir + (transform.forward*2), 10f);
                if (player.PlayerController.GetComponent<Death>().IsDead)
                {
                    playersToRemove.Add(player.PlayerController);
                }
            }
            else if(Vector3.Dot(-transform.forward, -dir) > 0)
            {
                if (player.PlayerController.CurrentState == PlayerController.State.Ragdoll)
                {
                    player.PlayerController.Hookable.Push(dir.normalized, speed * forceMultiplier * AirlockButton.Openness);
                }
                else
                {
                    player.PlayerController.AddForce(dir.normalized, speed * forceMultiplier * AirlockButton.Openness);
                }
                
            }
        }
        playersToRemove.ForEach(a => RemoveFromVac(a));
        Players.RemoveAll(a => a == null);
    }

    private void MoveDoors()
    {
        foreach (var item in Doors)
        {
            item.Door.transform.localPosition = Vector3.Lerp(item.ClosedLocalPos,item.OpenLocalPos,AirlockButton.Openness);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var PC = other.GetComponent<PlayerController>();
       
        if (PC != null)
        {
            var Player = Players.FirstOrDefault(a => a.PlayerController == PC);
            if (Player == null)
            {
                Players.Add(new PlayerAndForce(PC));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var PC = other.GetComponent<PlayerController>();
        if (PC != null)
        {
            RemoveFromVac(PC);
        }
    }

    private void RemoveFromVac(PlayerController PC)
    {
        //PC.ConstantPushforce = Vector3.zero;
        var Player = Players.FirstOrDefault(a => a.PlayerController == PC);
        if (Player != null)
        {
            Players.Remove(Player);
        }
       
    }

    //private void SetConstantPushForce(PlayerAndForce pPlayer, Vector3 pPushForce)
    //{
    //    .ConstantPushforce -= pPlayer.CurrentForce;
    //    pPlayer.PlayerController.ConstantPushforce += pPushForce;
    //    pPlayer.CurrentForce = pPushForce;
    //}
}

public class PlayerAndForce
{
    public PlayerController PlayerController;
    public Vector3 CurrentForce = Vector3.zero;

    public PlayerAndForce(PlayerController pPlayerController)
    {
        PlayerController = pPlayerController;
    }
}

[System.Serializable]
public class AirLockDoor
{
    public Vector3 ClosedLocalPos;
    public Vector3 OpenLocalPos;
    public GameObject Door;

    public AirLockDoor(Vector3 pClosedPos,Vector3 pOpenPos, GameObject pDoor, float pDoorIndex)
    {
        OpenLocalPos = pOpenPos;
        ClosedLocalPos = pClosedPos;
        Door = pDoor;
    }
}
