using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardAchievementTrigger : MonoBehaviour
{
    private int HitCounter = 0;

    private void Awake()
    {
        HitCounter = 0;
        EventManager.StartListening(EventManager.EventCodes.RoundEnd, OnRoundEnd);
    }

    private void OnRoundEnd()
    {
        HitCounter = 0;
    }

    public enum HazardDeathTypes
    {
        none,
        Grater,
        Lava,
        Airlock
    }
    public HazardDeathTypes HazardDeathType;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerHookable>();
        if (player != null)
        {
            HitCounter++;
            HazardEvent hazInfo = new HazardEvent
            {
                Description = "Hazard Death Event Triggered",
                RewiredID = player.PlayerController.RewiredID,
                HazardType = AchHazardType.None,
                PlayerHit = player.gameObject,
                Skin = player.PlayerController.Skin,
                HitCount = HitCounter
            };

            switch (HazardDeathType)
            {
                case HazardDeathTypes.none:
                    break;
                case HazardDeathTypes.Grater:
                    hazInfo.HazardType = AchHazardType.Grater;
                        break;
                case HazardDeathTypes.Lava:
                    hazInfo.HazardType = AchHazardType.Lava;
                    break;
                case HazardDeathTypes.Airlock:
                    hazInfo.HazardType = AchHazardType.Airlock;
                    break;
                default:
                    break;
            }
            hazInfo.FireEvent();
        }
        
    }
}
