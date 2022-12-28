using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpaceStationGameRules : MonoBehaviour
{
    public static SpaceStationGameRules Instance;
    List<PlayerDataModel> Players = new List<PlayerDataModel>();
    List<PlayerController> DeadPlayers = new List<PlayerController>();
    public GameObject DieParticleEffect;
    int playerCount = 0;

    GameManager GameManager;
    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GameManager = GameManager.Instance;
        Players = GameManager.PlayerDataModels;
        if (GameManager.UseBots)
        {
            playerCount = 4;
        }
        else
        {
            playerCount = Players.Count;
        }
        
        EventManager.StartListening(EventManager.EventCodes.RoundEnd, OnRoundEnd);
        EventManager.StartListening(EventManager.EventCodes.GameEnd, OnRoundEnd);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnRoundEnd()
    {
        //DeadPlayers.Clear();
    }

    public void KillPlayer(PlayerController PC)
    {
        if (!DeadPlayers.Contains(PC))
        {
            DeadPlayers.Add(PC);
            DeadPlayers.RemoveAll(a => a == null);
            if (DeadPlayers.Count >= (playerCount - 1))
            {
                DeadPlayers.ForEach(a => GameManager.KillPlayer(a, DieParticleEffect));
                DeadPlayers.Clear();
            }
        }
    }
}
