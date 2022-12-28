using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Explode : EffectModule
{
    public float TimeToExplodeAfterImpact = 1f;
    public float Radius = 5f;
    public float Strength = 40f;
    [SerializeField]
    private GameObject ParticleEffect;
    bool exploding;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == Mathf.Log(GroundLayer.value, 2))
        {
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.GroundBounce);
        }

        if (!exploding)
        {
            exploding = true;
            StartCoroutine(ExplodeTimer());
        }
    }

    IEnumerator ExplodeTimer()
    {
        yield return new WaitForSeconds(TimeToExplodeAfterImpact);
        var go = Instantiate(ParticleEffect, transform.position, Quaternion.identity);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.Explosion);
        Destroy(go, 4f);

        foreach (var item in FindObjectsOfType<PlayerController>())
        {
            Vector3 dir = item.transform.position - transform.position;
            if (dir.sqrMagnitude > Radius*Radius)
            {
                continue;
            }

            dir.y = 0;

            item.Hookable.Push(((dir).normalized + Vector3.up).normalized, Strength);
        }

        Destroy(gameObject);
    }
}
