using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bubble : EffectModule
{
    public GameObject BubbleEnterParticleEffect;
    List<Rigidbody> ControlledObjects = new List<Rigidbody>();
    [SerializeField]
    private Vector3 MaxScale;
    [SerializeField]
    private float TimeUntilMaxScale;
    private float ScaleTimer = 0;
    private Vector3 StartScale;
    [SerializeField]
    private float MaxPullForce = 5;
    private float StartY = 1;
    [SerializeField]
    private float MaxYOffset = 2;

    private void Awake()
    {
        StartScale = transform.localScale;
        
    }

    // Update is called once per frame
    protected override void UpdateEffect()
    {
        base.UpdateEffect();

        ScaleTimer += Time.deltaTime;

        transform.localScale = Vector3.Lerp(StartScale, MaxScale, ScaleTimer / TimeUntilMaxScale);
        transform.position = new Vector3(transform.position.x, Mathf.Lerp(StartY, MaxYOffset, ScaleTimer / TimeUntilMaxScale), transform.position.z);


        if (ControlledObjects.Any())
        {
            for (int i = 0; i < ControlledObjects.Count; i++)
            {
                var dir = transform.position - ControlledObjects[i].transform.position;

                var pc = ControlledObjects[i].GetComponent<PlayerController>();
                if (pc != null /*&& pc.CurrentState != PlayerController.State.Hooked*/)
                {
                    //pc.Hookable.Push(dir.normalized, MaxPullForce);
                    ControlledObjects[i].AddForce(dir.normalized * MaxPullForce, ForceMode.Impulse);
                    ControlledObjects[i].AddTorque(new Vector3(Random.Range(1, 20), Random.Range(1, 20), Random.Range(1, 20)), ForceMode.Impulse);
                }
            }
        } 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            return;
        }

        var rb = other.GetComponent<Rigidbody>();
        var pc = other.GetComponent<PlayerController>();

        if (pc != null || (rb != null && !rb.isKinematic))
        {
            if (ControlledObjects.Contains(rb))
            {
                return;
            }

            pc.OnBeingHit();
            rb.drag = pc.Hookable.startDrag;
            rb.angularDrag = pc.Hookable.startAngularDrag;
            rb.isKinematic = false;
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.ThrownOut); // thrown out spacestation sound
            if (BubbleEnterParticleEffect != null)
            {
                var ps = Instantiate(BubbleEnterParticleEffect,other.ClosestPoint(transform.position),Quaternion.identity);
                Destroy(ps, 3f);
            }
            ControlledObjects.Add(rb);
            if (!popStarted)
            {
                popStarted = true;
                StartCoroutine(DelayedPop());
            }
            UsableHitEvent info = new UsableHitEvent
            {
                Description = "Push Usable hit a player",
                PlayerHit = pc.gameObject,
                RewiredID = OwnerPlayerController.RewiredID,
                Usable = Usable.Type,
                UsableOwner = UsableOwner
            };
            info.FireEvent();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var rb = other.GetComponent<Rigidbody>();
        var hookable = other.GetComponent<Hookable>();
        if (other.isTrigger || rb == null)
        {
            return;
        }

        if (ControlledObjects.Contains(rb))
        {
            rb.drag = hookable.startDrag;
            rb.angularDrag = hookable.startAngularDrag;
            ControlledObjects.Remove(rb);
            hookable.Push(rb.velocity.normalized * 1);
            if (BubbleEnterParticleEffect != null)
            {
                var ps = Instantiate(BubbleEnterParticleEffect, other.ClosestPoint(transform.position), Quaternion.identity);
                Destroy(ps, 3f);
            }
        }
    }

    bool popStarted;
    IEnumerator DelayedPop()
    {
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
        if (Usable.OnDestroyParticlesPrefab != null)
        {
            var go = Instantiate(Usable.OnDestroyParticlesPrefab,transform.position,Quaternion.identity);
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.BubblePop);
            Destroy(go,3f);
        }
    }

    private void OnDestroy()
    {
        foreach (var player in ControlledObjects)
        {
            var hookable = player.GetComponent<Hookable>();
            hookable.Push(player.velocity.normalized * 1);
        }
    }
}
