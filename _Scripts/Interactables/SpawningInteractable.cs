using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawningInteractable : Interactable
{
    public enum ActivateType
    {
        hook, player, rigidbody, any
    }
    public ActivateType activateType;
    public GameObject ObjectToSpawn;
    public GameObject DespawnParticleEffect;
    public float SpawnLifeTime = 2f;
    public bool SpawnParticlesOnAnimator;
    bool ready = true;

    void Spawn(ActivateType pActivateType)
    {
        if ((pActivateType == activateType || activateType == ActivateType.any) && ready)
        {
            ready = false;
            StartCoroutine(LateSetReady());
            var go = Instantiate(ObjectToSpawn,transform.position,transform.rotation);
            
            if (DespawnParticleEffect != null)
            {
                StartCoroutine(DespawnEffect(go.transform, SpawnLifeTime));
            }
            else
            {
                Destroy(go, SpawnLifeTime);
            }
        }
    }

    IEnumerator DespawnEffect(Transform pTrans, float pTime)
    {
        yield return new WaitForSeconds(pTime);
        Vector3 pos = SpawnParticlesOnAnimator ? pTrans.GetComponentInChildren<Animator>().transform.position : pTrans.position;
        //print(pTrans.GetComponentInChildren<Animator>().transform.position);
        //print(pTrans.position);
        var ps = Instantiate(DespawnParticleEffect, pos, Quaternion.identity);
        Destroy(pTrans.gameObject);
        Destroy(ps, 3f);
    }

    IEnumerator LateSetReady()
    {
        yield return new WaitForSeconds(1f);
        ready = true;
        Spawn(ActivateType.any);
    }

    protected override void OnHookActivate()
    {
        Spawn(ActivateType.hook);
    }
    protected override void OnGeneralActivate()
    {
        Spawn(ActivateType.any);
    }
    protected override void OnPlayerActivate()
    {
        Spawn(ActivateType.player);
    }

    protected override void OnRigidbodyActivate()
    {
        Spawn(ActivateType.rigidbody);
    }
}
