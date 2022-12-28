using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveHit : MonoBehaviour
{
    public float HitStrength;
    public WaveHit(float hitStrength)
    {
        HitStrength = hitStrength;
    }

    private void OnTriggerEnter(Collider other)
    {
        //var hookable = other.GetComponent<Hookable>();
        var hookable = other.GetComponent<PlayerHookable>();
        if (hookable != null)
        {
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.WaveHit);
            hookable.Push((transform.forward + (transform.up)).normalized, HitStrength);
            HazardEvent hazInfo = new HazardEvent
            {
                Description = "Wave hit a player",
                PlayerHit = hookable.gameObject,
                RewiredID = hookable.PlayerController.RewiredID,
                HazardType = AchHazardType.Wave,
                Skin = hookable.PlayerController.Skin
            };
            hazInfo.FireEvent();
        }
    }
}