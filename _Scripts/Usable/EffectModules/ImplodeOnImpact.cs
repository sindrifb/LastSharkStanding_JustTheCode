using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ImplodeOnImpact : EffectModule
{
    public float Radius = 7f;
    public float Strength = 60f;
    [SerializeField]
    private GameObject ParticleEffect;
    bool exploding;

    private void OnCollisionEnter(Collision collision)
    {
        //AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.GroundBounce);

        Explode();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Mathf.Log(DeathZoneLayer, 2))
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (!exploding)
        {
            exploding = true;
            var go = Instantiate(ParticleEffect, transform.position, Quaternion.identity);
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.ImplosionGrenade);
            Destroy(go, 4f);
            var players = FindObjectsOfType<PlayerController>().ToList();
            if (players.Any())
            {
                foreach (var player in players)
                {
                    Vector3 dir = player.transform.position - transform.position;
                    if (dir.sqrMagnitude > Radius * Radius)
                    {
                        continue;
                    }

                    dir.y = 0;

                    player.Hookable.Push(((-dir).normalized + Vector3.up).normalized, Strength);
                    UsableHitEvent info = new UsableHitEvent
                    {
                        Description = "Push Usable hit a player",
                        PlayerHit = player.gameObject,
                        RewiredID = OwnerPlayerController.RewiredID,
                        Usable = Usable.Type,
                        UsableOwner = UsableOwner
                    };
                    info.FireEvent();
                }
            }

            var destructibles = FindObjectsOfType<Destructible>().Where(a => (a.transform.position - transform.position).sqrMagnitude <= (Radius * Radius));
            if (destructibles.Any())
            {
                foreach (var destructible in destructibles)
                {
                    Vector3 explosionPos = destructible.transform.position + ((transform.position - destructible.transform.position).normalized * 0.5f);
                    destructible.DestroyDestructible(explosionPos);
                }
            }

            Destroy(gameObject);
        }
    }
}
