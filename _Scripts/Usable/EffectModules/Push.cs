using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using FMOD.Studio;

public class Push : EffectModule
{
    public GameObject HitParticleEffectPrefab;
    public float strenght = 60f;
    public bool needMinVel;
    public PowerupType Powerup;
    [SerializeField]
    private bool UseTrigger;
    [SerializeField]
    private bool UseCollider;
    private float SoundTimer = 0.5f;
    public enum PowerupType
    {
        Anchor,
        Disk,
        Boomerang,
        Selflaunch,
        Spinner
    }

    Dictionary<PlayerController, float> NonHitablePlayers = new Dictionary<PlayerController, float>();

    protected override void UpdateEffect()
    {
        base.UpdateEffect();
        if (Usable.IsActive)
        {
            var keysToRemove = new List<PlayerController>();
            var allKeys = new List<PlayerController>(NonHitablePlayers.Keys);

            foreach (var item in allKeys)
            {
                NonHitablePlayers[item] += NonHitablePlayers[item] + Time.deltaTime;
                if (NonHitablePlayers[item] > 1f)
                {
                    keysToRemove.Add(item);
                }
            }

            foreach (var key in keysToRemove)
            {
                NonHitablePlayers.Remove(key);
            }
        }      
    }

    private void Update()
    {
        SoundTimer += Time.deltaTime;
    }

    void OnTriggerEnter(Collider col)
    {
        if (UseTrigger)
        {
            OnCollision(col);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (UseCollider)
        {
            OnCollision(col.collider);
        }

        //if (Usable.IsActive && col.collider.gameObject.layer == Mathf.Log(GroundLayer.value, 2))
        //{
        //    Debug.Log("Ground Bounce Sound");
        //    PlayPowerupCollisionSound(Powerup);
        //}

        if (Usable.IsActive && Usable.Rigidbody.velocity.sqrMagnitude >= 3 * 3 && SoundTimer >= 0.2f)
        {
            //Debug.Log("Ground Bounce Sound");
            PlayPowerupCollisionSound(Powerup);
        }
    }

    private void OnCollision(Collider pCol)
    {
        var pc = pCol.GetComponent<PlayerController>();

        if (pCol.isTrigger || !Usable.IsActive)
        {
            return;
        }
        if (pc != null && NonHitablePlayers.ContainsKey(pc))
        {
            return;
        }
        var rb = pCol.GetComponent<Rigidbody>();


        if (pc != null && (Usable.Rigidbody.velocity.sqrMagnitude > 5 * 5 || !needMinVel))
        {
            NonHitablePlayers.Add(pc, 0);
            pc.Hookable.Push(((pCol.transform.position - transform.position).normalized + Vector3.up).normalized, strenght);
            PlayPowerupHitSound(Powerup);
            PlayHitParticle(pCol.ClosestPoint(transform.position));
            UsableHitEvent info = new UsableHitEvent
            {
                Description = "Push Usable Trigger hit a player",
                PlayerHit = pc.gameObject,
                RewiredID = OwnerPlayerController.RewiredID,
                Usable = Usable.Type,
                UsableOwner = UsableOwner
            };
            info.FireEvent();
        }
        else if (rb != null && !rb.isKinematic)
        {
            rb.AddForce(transform.forward + Vector3.up, ForceMode.Impulse);
            PlayPowerupHitSound(Powerup);
            PlayHitParticle(pCol.ClosestPoint(transform.position));
        }
    }

    private void PlayHitParticle(Vector3 pos)
    {
        if (HitParticleEffectPrefab != null)
        {
            var ps = Instantiate(HitParticleEffectPrefab, pos, Quaternion.identity);
            Destroy(ps, 2f);
        }
    }

    /// <summary>
    /// Sound when hitting Players or Rigidbodies
    /// </summary>
    /// <param name="pPowerupType"></param>
    private void PlayPowerupHitSound(PowerupType pPowerupType)
    {
        switch (pPowerupType)
        {
            case PowerupType.Anchor:
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.GroundBounce);
                break;
            case PowerupType.Disk:
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.GroundBounce);
                break;
            case PowerupType.Boomerang:
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.GroundBounce);
                break;
            case PowerupType.Selflaunch:
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.GroundBounce);
                break;
            case PowerupType.Spinner:
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.GroundBounce);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Sound playing on collision
    /// </summary>
    /// <param name="pPowerupType"></param>
    private void PlayPowerupCollisionSound(PowerupType pPowerupType)
    {
        switch (pPowerupType)
        {
            case PowerupType.Anchor:
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.HookSound.AnchorHit);
                break;
            case PowerupType.Disk:
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.GroundBounce);
                break;
            case PowerupType.Boomerang:
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.GroundBounce);
                break;
            case PowerupType.Selflaunch:
                //AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.GroundBounce);
                break;
            case PowerupType.Spinner:
                //AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.GroundBounce);
                break;
            default:
                break;
        }
    }
}
