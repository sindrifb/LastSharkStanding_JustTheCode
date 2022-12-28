using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SphereCollider sc = gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider
/// </summary>

public abstract class BasicHazard : MonoBehaviour
{
    public HazardType HazardType;
    [HideInInspector]
    public HazardData Hazard;
    protected PlayerController PlayerHit;

    // NEEDS AT LEAST 2 BECAUSE SOME SOUNDS MIGHT OVERLAP
    public AudioSource AimingExistingAudioSource;
    public AudioSource MovingWarningAudioSource;
    public AudioSource EffectsAudioSource;

    protected bool Landed = false;
    protected bool LandedOnlyOnce = false;
    protected bool KilledAlready = false;

    // If the hazard has alrady started its cycle, but its in the WaitToAppear Time
    public bool IsWaitingToSpawn { get; set; } = false;

    protected virtual void Start() { }
    protected virtual void CheckLanded() { }
    protected virtual void Movement() { }

    public virtual void Initializing()
    {
        IsWaitingToSpawn = true;
        Landed = false;
        LandedOnlyOnce = false;
        KilledAlready = false;
        transform.position = Hazard.OrigPos;

        try
        {
            Hazard.SpawningVisualEvent.SetAudioSource(EffectsAudioSource);
            Hazard.LandingVisualEvent.SetAudioSource(EffectsAudioSource);
            Hazard.DissapearingVisualEvent.SetAudioSource(EffectsAudioSource);

            Hazard.AimingVisualEvent.SetAudioSource(AimingExistingAudioSource);
            Hazard.ExistingVisualEvent.SetAudioSource(AimingExistingAudioSource);

            Hazard.MovingVisualEvent.SetAudioSource(MovingWarningAudioSource);
            Hazard.WarningExplosionVisualEvent.SetAudioSource(MovingWarningAudioSource);

        }catch (System.Exception)
        {
            Debug.LogWarning("Hazard audio source NOT assigned correctly");
        }
    }

    protected virtual void Update()
    {
        if (Landed)
        {
            if (!LandedOnlyOnce) OnLanded();
        }
        else
        {
            CheckLanded();
            Movement();
        }
    }

    // Start hazard's behaviour 
    public virtual void TriggerHazBehaviour()
    {
        // Initializing position, booleans and audio source
        Initializing();

        // Play aim
        //Hazard.StopAllParticles();
        Hazard.AimingVisualEvent.Play();

        // Timer to spawn
        /// I know invokes are shit, but its the only timer 
        /// I can think about that works with disabled objects :3
        Invoke("SpawnHaz", Hazard.WaitToAppear);
    }

    public virtual void OnEnable()
    {
        Rigidbody auxRB = gameObject.GetComponent<Rigidbody>();
        if (auxRB != null)
        {
            auxRB.isKinematic = true;
            transform.rotation = Quaternion.identity;
        }

        Hazard.SpawningVisualEvent.Play();
        Hazard.MovingVisualEvent.Play();
    }

    public virtual void OnDisable()
    {
        IsWaitingToSpawn = false;
        StopAllCoroutines();
        Hazard.SpawningVisualEvent.Stop();
        Hazard.AimingVisualEvent.Stop();
        Hazard.MovingVisualEvent.Stop();
        Hazard.LandingVisualEvent.Stop();
        Hazard.DissapearingVisualEvent.Stop();
        Hazard.WarningExplosionVisualEvent.Stop();
    }

    protected virtual void SpawnHaz()
    {
        gameObject.SetActive(true);
        IsWaitingToSpawn = false;
    }

    public virtual void OnTriggerEnter(Collider pOther)
    {
        // 
        if (pOther.GetComponent<PlayerController>())
        {
            PlayerHit = pOther.GetComponent<PlayerController>();
            if (Hazard.TriggersWithProximity)
                TriggerWarningAndExplosionInTime();
            else if(Hazard.DiesOneHit)
                TriggerHazard();
        }
    }

    protected virtual void OnLanded()
    {
        LandedOnlyOnce = true;

        Hazard.AimingVisualEvent.Stop();
        Hazard.MovingVisualEvent.Stop();

        Hazard.LandingVisualEvent.Play();
        Hazard.ExistingVisualEvent.Play();

        if (Hazard.ExplodeOnImpact)
        {
            TriggerHazard(); // explode without coroutines
        }
        else if (Hazard.HasTimerToDie)
        {
            if (Hazard.HasWarning)
            {
                TriggerWarningAndExplosionInTime();  // warning and then explodes
            }
            else
            {
                TriggerExplosionInTime(Hazard.WaitToDie); // explodes after time
            }
        }
    }

    protected virtual void TriggerWarningAndExplosionInTime()
    {
        StartCoroutine(TriggerWarningInTime(Hazard.WaitToDie - Hazard.WarningTime > 0 ? Hazard.WaitToDie - Hazard.WarningTime : 0)); // Interval between landing and warning
    }
    protected virtual IEnumerator TriggerWarningInTime(float pTime)
    {
        yield return new WaitForSeconds(pTime);

        Hazard.WarningExplosionVisualEvent.Play();
        TriggerExplosionInTime(Hazard.WarningTime); // Explodes in warning time because its the time "left" to explode the hazard
    }

    protected virtual void TriggerExplosionInTime(float pTime)
    {
        StartCoroutine(TriggerExplosionInTimeCoroutine(pTime));
    }
    protected virtual IEnumerator TriggerExplosionInTimeCoroutine(float pTime)
    {
        yield return new WaitForSeconds(pTime);

        if (isActiveAndEnabled)
        {
            Hazard.WarningExplosionVisualEvent.Stop();
            TriggerHazard();
        }
    }

    protected virtual void TriggerHazard()
    {
        if (Hazard.Explodes || Hazard.ExplodeOnImpact)
        {
            ExplodeHazard();
            KillHazard();
        }
        else if (PlayerHit != null)
        {
            HitPlayer(PlayerHit);
            if (Hazard.DiesOneHit)
                KillHazard();
        }else
            KillHazard();
    }

    protected virtual void HitPlayer(PlayerController pPlayer)
    {
        pPlayer?.Hookable.Push(-pPlayer.gameObject.transform.forward + Vector3.up, Hazard.PushingForce);
        PlayerHit = null;
    }

    public virtual void KillHazard()
    {
        if(!KilledAlready){
            Hazard.DissapearingVisualEvent.Play();
            Hazard.ExistingVisualEvent.Stop();
            Hazard.WarningExplosionVisualEvent.Stop();

            KilledAlready = true;

            float time = Hazard.DissapearingVisualEvent.SFX != null ? Hazard.DissapearingVisualEvent.SFX.length : Hazard.DissapearingVisualEvent.PlayLength();

            HazardManager.Instance.KillHazard(this.gameObject, time);
            StopAllCoroutines();
        }
        
    }

    protected virtual void ExplodeHazard()
    {
        /// TOCHECK RESOURCE CONSUME
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, Hazard.ExplosionRadio, Vector3.forward);

        foreach (RaycastHit r in hits)
        {
            if (r.transform?.GetComponent<PlayerController>())
            {
                Vector3 dir = (r.transform.position - this.transform.position);//new Vector3(r.transform.position.x - transform.position.x, 1, r.transform.position.z - transform.position.z);
                dir.y = 0;
                dir.Normalize();
                dir += Vector3.up;

                float dist = Vector3.Distance(r.transform.position, transform.position);

                float force = ExplosionForce(dist);

                if (force >= Hazard.ExplosionMinForce)
                    r.transform.GetComponent<PlayerController>().Hookable.Push(dir, force);
            }
        }
    }

    public float ExplosionForce(float pDistance)
    {
        float relativeDist = pDistance / Hazard.ExplosionRadio;
        float force = 0;

        if (relativeDist < 1 && relativeDist >= 0)
        {
            relativeDist = 1 - relativeDist;

            force = ((Hazard.ExplosionMaxForce-Hazard.ExplosionMinForce) * relativeDist) + Hazard.ExplosionMinForce;
        }
        return force;
    }

    // GETTERS AND SETTERS
    public void SetOriginAndEndPos(Vector3 pOrigPos, Vector3 pEndPos)
    {
        Hazard.OrigPos = pOrigPos;
        Hazard.EndPos = pEndPos;
    }
    public void SetHazardData(HazardData pHData)
    {
        Hazard.SetOwner(this.gameObject);
        Hazard.SetHazardData(pHData);
    }
    public HazardData GetHazardData() { return Hazard; }

    private void OnDestroy()
    {
        if (GameManager.Instance.SpawnedObjects.Contains(gameObject))
        {
            GameManager.Instance.SpawnedObjects.Remove(gameObject);
        }
    }
}
