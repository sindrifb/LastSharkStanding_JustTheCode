using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Implode : EffectModule
{
    public float Radius = 7f;
    public float Strenght = 60f;
    public float timeToExplodeAfterInpact = 1f;
    [SerializeField]
    private GameObject ParticleEffect;
    bool exploding;

    private void OnCollisionEnter(Collision collision)
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.GroundBounce);
        if (!exploding)
        {
            exploding = true;
            StartCoroutine(ExplodeTimer());
        }
    }

    IEnumerator ExplodeTimer()
    {
        yield return new WaitForSeconds(timeToExplodeAfterInpact);
        var go = Instantiate(ParticleEffect, transform.position, Quaternion.identity);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.ImplosionGrenade);
        Destroy(go, 4f);

        foreach (var item in FindObjectsOfType<PlayerController>())
        {
            Vector3 dir = item.transform.position - transform.position;
            if (dir.sqrMagnitude > Radius * Radius)
            {
                continue;
            }

            dir.y = 0;

            item.Hookable.Push(((-dir).normalized + Vector3.up).normalized, Strenght);
        }

        Destroy(gameObject);
    }
}
