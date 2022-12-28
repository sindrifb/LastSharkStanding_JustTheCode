using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillOnEnter : MonoBehaviour
{
    public bool ChangeDragOnImpact = false;
    public bool LocalParticleRotation;
    public bool AttachParticleToPlayer;
    public float Drag = 2f;
    public GameObject ParticlePrefab;
    public GameObject MiscParticlePrefab;
    public GameObject HookParticlePrefab;
    public bool DestroyOnEndOfRound = false;
    public DeathType Death;
    public enum DeathType
    {
        Falling,
        Water,
        Sand,
        Lava,
        space
    }
    private float DeathIndex;

    public float GetDeathIndex(DeathType pDeathType)
    {
        switch (pDeathType)
        {
            case DeathType.Falling:
                return 0;
            case DeathType.Water:
                return 1;
            case DeathType.Sand:
                return 2;
            case DeathType.Lava:
                return 3;
            case DeathType.space:
                return 4;
            default:
                return 0;
        }
    }

    IEnumerator TempTurnOffCollisionDetection(Rigidbody rb)
    {
        rb.detectCollisions = false;
        yield return new WaitForSeconds(.4f);
        rb.detectCollisions = true;
    }

    void OnTriggerEnter(Collider col)
    {
        var usable = col.GetComponent<Usable>();
        var death = col.GetComponent<Death>();
        var rb = col.GetComponent<Rigidbody>();
        var powerupPickup = col.GetComponent<PowerUpPickupTest>();
        
        if (col.isTrigger)
        {
            if (powerupPickup != null)
            {
                powerupPickup.TimedDelete(5f);
            }

            if (usable != null)
            {
                var psPrefab = HookParticlePrefab == null ? ParticlePrefab : HookParticlePrefab;
                Quaternion rotation = LocalParticleRotation ? col.transform.rotation : Quaternion.identity;
                if (psPrefab != null)
                {
                    var ps = Instantiate(psPrefab, col.transform.position, rotation);
                    Destroy(ps, 4f);
                }


                if (rb.velocity.magnitude > 3 * 3)
                {
                    rb.velocity = rb.velocity / 2;
                    rb.angularVelocity = rb.angularVelocity / 3;
                    rb.AddForce(new Vector3(0, Mathf.Abs(rb.velocity.y * 2), 0), ForceMode.Impulse);

                    if ( Death == DeathType.Water)
                    {
                        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.WaveHit);
                    }

                    return;
                }

                if (Death != DeathType.space && Death != DeathType.Falling)
                {
                    AudioManager.Instance.PlayEventWithParameter(AudioManager.Instance.PlayerSound.Death, Constants.FmodParameters.DeathIndex, GetDeathIndex(Death), out EventInstance pEvent);
                }

                col.GetComponent<Rigidbody>().drag = Drag;

            }
            return;
        }
        
       
        if (col.transform.root == col.transform && death != null)
        {
            if (rb != null)
            {
                rb.drag = Drag;
            }
            if (Death == DeathType.space)
            {
                var pc = col.GetComponent<PlayerController>();
                rb.useGravity = false;
                FreezeVisual(pc.transform);
                pc.GetComponent<Hookable>().DontGetUp = true;
            }

            if (Death == DeathType.Lava && !death.IsDead)
            {
                var pc = col.GetComponent<PlayerController>();
                pc.gameObject.layer = 14; //IgnorePlayerAndGround
                if (col.transform.position.magnitude > 10)
                {
                    pc.Hookable.Push(col.transform.position.normalized + (Vector3.up * 2), 50);
                }
                else
                {
                    StartCoroutine(TempTurnOffCollisionDetection(rb));
                    var dir = Vector3.up + (col.transform.position.normalized / 2f);
                    pc.Hookable.Push(dir, 135);
                    Debug.DrawRay(pc.transform.position, dir * 20, Color.blue, 5f);
                }
            }

            if (!death.IsDead)
            {
                AudioManager.Instance.PlayEventWithParameter(AudioManager.Instance.PlayerSound.Death, Constants.FmodParameters.DeathIndex, GetDeathIndex(Death), out EventInstance pEvent);
                death.Die(ParticlePrefab, DestroyOnEndOfRound, null, LocalParticleRotation, AttachParticleToPlayer);
            }
        }
        else if (col.transform.root == col.transform && rb != null && !rb.isKinematic)
        {
            if (rb.velocity.magnitude > 2*2)
            {
                rb.velocity = rb.velocity / 2;
                rb.AddForce(new Vector3(0,Mathf.Abs(rb.velocity.y),0),ForceMode.Impulse);
            }
            else
            {
                if (Death == DeathType.Lava)
                {
                    var pc = col.GetComponent<PlayerController>();
                    pc.Hookable.Push(col.transform.position.normalized + (Vector3.up * 2), 40);
                    AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.LavaHit);
                }

                if (rb != null)
                {
                    rb.drag = Drag;
                }
                var psPrefab = MiscParticlePrefab == null ? ParticlePrefab : MiscParticlePrefab;
                Quaternion rotation = LocalParticleRotation ? col.transform.rotation : Quaternion.identity;
                col.GetComponent<Rigidbody>().drag = Drag;
                var ps = Instantiate(psPrefab, col.transform.position, rotation);
                Destroy(ps, 4f);
            }
        }  
    }
    private void FreezeVisual(Transform pTransform)
    {
        var renderers = pTransform.GetComponentsInChildren<Renderer>();
        foreach (var item in renderers)
        {
            if (item.material.HasProperty("_Color"))
            {
                item.material.color = item.material.color + Color.grey + Color.blue;
            }
        }
    }
}
