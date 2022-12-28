using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public enum HazardType { Mine, Wave, CanonShot }

[System.Serializable]
public class HazardData
{
    public GameObject HazardGO;
    public HazardType HazardType { get; set; }

    [Tooltip("Max number of hazard of this type on screen. If below zero, doesn't check and can exist as many as you wnat")]
    public int MaxHazardNumOnScreen = 5;
    [Tooltip("Radio of the minimum distance between hazards spawned of the type of this spawner. " +
        "WARNING! if set too big or with a lot of objects it can take a lot of time to find a random position far from each object and finally to an infinite loop.")]
    public float RadioMinDistanceBetweenHazards = 0;

    public float PushingForce = 5f;
    public float MovementMagnitude = 1f;

    [Tooltip("Time for the particle effect feedback, without the hazard on-screen")]
    public float WaitToAppear = 1f;

    public bool DiesOneHit = true; // if the hazard is killed after one hit or at least after landing

    public bool HasTimerToDie = true;
    [Tooltip("Time between hitting the floor and dissappearing. Whole life time of the hazard. If explodes, time until explosion")]
    public float WaitToDie = 5f;

    // PARTICLES
    [Tooltip("Particles for spawning the hazard, played in the spawning origin position")]
    public VisualParticleEvent SpawningVisualEvent = new VisualParticleEvent();

    [Tooltip("Particles for the aiming position (as warning feedback), playing while the hazard hasn't reached the target")]
    public VisualParticleEvent AimingVisualEvent = new VisualParticleEvent();

    [Tooltip("Particles while moving the hazard, played on the hazard")]
    public VisualParticleEvent MovingVisualEvent = new VisualParticleEvent();

    [Tooltip("Particles for the disappearance. Is the explosion particles if it explodes")]
    public VisualParticleEvent LandingVisualEvent = new VisualParticleEvent();

    [Tooltip("Particles for the disappearance. Is the explosion particles if it explodes")]
    public VisualParticleEvent ExistingVisualEvent = new VisualParticleEvent();

    [Tooltip("Particles for the disappearance. Is the explosion particles if it explodes")]
    public VisualParticleEvent DissapearingVisualEvent = new VisualParticleEvent();

    // EXPLOSION
    [Tooltip("Checked if the hazard has to explode before dissapearing")]
    public bool Explodes = true;
    public bool ExplodeOnImpact;
    [Tooltip("Uses de Wait To Die Time to explode after being triggered")]
    public bool TriggersWithProximity = true;

    public float TriggeringRadio = 3; 
    public float ExplosionRadio = 3;
    [Tooltip("Force applied in the same position of the hazard. Goes until minForce at the distance of the ExplosionRadio")]
    public float ExplosionMaxForce = 5;
    [Tooltip("Force applied in the radio distance from the hazard")]
    public float ExplosionMinForce = 0;

    public bool HasWarning = true;
    public float WarningTime = 0;
    [Tooltip("Particles to warn before the explosion")]
    public VisualParticleEvent WarningExplosionVisualEvent = new VisualParticleEvent();

    public Vector3 OrigPos { get; set; }
    public Vector3 EndPos { get; set; }
        
    public void InitializeParticles()
    {
        SpawningVisualEvent.InitializeToSetPos(OrigPos);
        AimingVisualEvent.InitializeToSetPos(EndPos);

        MovingVisualEvent.InitializeToFollow();

        LandingVisualEvent.InitializeToSetPos(EndPos);

        ExistingVisualEvent.InitializeToFollow();
        DissapearingVisualEvent.InitializeToFollow();
        WarningExplosionVisualEvent.InitializeToFollow();
    }

    public void SetOwner(GameObject pOwner)
    {
        SpawningVisualEvent.SetOwner(pOwner);
        AimingVisualEvent.SetOwner(pOwner);
        MovingVisualEvent.SetOwner(pOwner);
        LandingVisualEvent.SetOwner(pOwner);
        ExistingVisualEvent.SetOwner(pOwner);
        DissapearingVisualEvent.SetOwner(pOwner);
        WarningExplosionVisualEvent.SetOwner(pOwner);
    }

    public void StopAllParticles()
    {
        SpawningVisualEvent.Stop();
        AimingVisualEvent.Stop();
        MovingVisualEvent.Stop();
        LandingVisualEvent.Stop();
        ExistingVisualEvent.Stop();
        DissapearingVisualEvent.Stop();
        WarningExplosionVisualEvent.Stop();
    }

    public void OnValidate()
    {
        // CHECK
        //HazardType = HazardGO.GetComponent<BasicHazard>().HazardType;
    }

    // EMISSION MODULES SWITCHERS
    public void SetSpawningEvent(bool pEnabled)
    {
        if (pEnabled) SpawningVisualEvent.Play();
        else SpawningVisualEvent.Stop();
    }
    public void SetAimingEvent(bool pEnabled)
    {
        if (pEnabled) AimingVisualEvent.Play();
        else AimingVisualEvent.Stop();
    }
    public void SetMovingEvent(bool pEnabled)
    {
        if (pEnabled) MovingVisualEvent.Play();
        else MovingVisualEvent.Stop();
    }
    public void SetLandingEvent(bool pEnabled)
    {
        if (pEnabled) LandingVisualEvent.Play();
        else LandingVisualEvent.Stop();
    }
    public void SetExistingEvent(bool pEnabled)
    {
        if (pEnabled) ExistingVisualEvent.Play();
        else ExistingVisualEvent.Stop();
    }
    public void SetDissapearingEvent(bool pEnabled)
    {
        if (pEnabled) DissapearingVisualEvent.Play();
        else DissapearingVisualEvent.Stop();
    }
    public void SetWarningExplodeEvent(bool pEnabled)
    {
        if (pEnabled) WarningExplosionVisualEvent.Play();
        else WarningExplosionVisualEvent.Stop();
    }

    // COPIES THE HAZARD DATA
    public void SetHazardData(HazardData pHazardData)
    {
        HazardGO = pHazardData.HazardGO;

        HazardType = pHazardData.HazardType;

        PushingForce = pHazardData.PushingForce;

        MovementMagnitude = pHazardData.MovementMagnitude;

        WaitToAppear = pHazardData.WaitToAppear;
        DiesOneHit = pHazardData.DiesOneHit;
        HasTimerToDie = pHazardData.HasTimerToDie;

        WaitToDie = pHazardData.WaitToDie;

        SpawningVisualEvent.SetVisualSoundEffect(pHazardData.SpawningVisualEvent);
        AimingVisualEvent.SetVisualSoundEffect(pHazardData.AimingVisualEvent);
        MovingVisualEvent.SetVisualSoundEffect(pHazardData.MovingVisualEvent);
        LandingVisualEvent.SetVisualSoundEffect(pHazardData.LandingVisualEvent);
        ExistingVisualEvent.SetVisualSoundEffect(pHazardData.ExistingVisualEvent);
        DissapearingVisualEvent.SetVisualSoundEffect(pHazardData.DissapearingVisualEvent);


        Explodes = pHazardData.Explodes;
        ExplodeOnImpact = pHazardData.ExplodeOnImpact;
        ExplosionRadio = pHazardData.ExplosionRadio;
        ExplosionMaxForce = pHazardData.ExplosionMaxForce;
        ExplosionMinForce = pHazardData.ExplosionMinForce;

        TriggersWithProximity = pHazardData.TriggersWithProximity;
        TriggeringRadio = pHazardData.TriggeringRadio;

        HasWarning = pHazardData.HasWarning;
        WarningTime = pHazardData.WarningTime;
        WarningExplosionVisualEvent.SetVisualSoundEffect(pHazardData.WarningExplosionVisualEvent);;


        InitializeParticles();
    }

    // AUX GET AND SET
    public float GetForce() { return PushingForce; }
    public void SetForce(float pForce) { PushingForce = pForce; }
    public float GetMovementMagnitude() { return MovementMagnitude; }
    public void SetMovementMagnitude(float pVel) { MovementMagnitude = pVel; }
    public bool IsExploding() { return Explodes; }
    public void SetExploding(bool pExplodes) { Explodes = pExplodes; }
}

[System.Serializable]
public class VisualParticleEvent : VisualEvent
{
    // PARTICLES
    public GameObject ParticleModel;
    protected GameObject Particle;

    protected ParticleSystem[] ParticleSystems = new ParticleSystem[0];
    public ParticleSystem.EmissionModule EmissionModuleVisualEvent;


    // SOUNDS AND ANIMATIONS ARE HANDLED IN VISUALEVENT
    public virtual void InitializeToFollow()
    {
        if (ParticleModel != null && Owner != null)
        {
            if (Particle == null)
            {
                Particle = (MonoBehaviour.Instantiate(ParticleModel, Owner.transform.position, Quaternion.identity, Owner.transform));
                Particle.transform.position = Owner.transform.position;
                ParticleSystems = Particle.GetComponentsInChildren<ParticleSystem>();
                Particle.SetActive(false);
            }
            Particle.transform.position = Owner.transform.position;
        }
        else
        {
            //if (ParticleModel)
            //    Debug.LogWarning("NOT INIT (Follow) " + ParticleModel.name);
        }
    }

    public virtual void InitializeToSetPos(Vector3 pParticlePos)
    {
        if (ParticleModel != null)
        {
            if (Particle == null)
            {
                Particle = (MonoBehaviour.Instantiate(ParticleModel, pParticlePos, Quaternion.identity));
                ParticleSystems = Particle.GetComponentsInChildren<ParticleSystem>();
                Particle.SetActive(false);
            }
            Particle.transform.position = pParticlePos;
        }
    }

    public override void Play()
    {
        try
        {
            base.Play();

            if (Particle != null && ParticleSystems.Length > 0 && !ParticleSystems[0].isPlaying)
            {
                Particle.transform.rotation = Quaternion.identity;

                Particle.SetActive(true);
                foreach (ParticleSystem ps in ParticleSystems)
                    ps?.Play();
            }
        }
        catch (System.Exception e)
        {
            //Debug.LogError("EXCEPTION PLAY OBJ " + ParticleModel.name + ", " + e.Message);
        }
    }
    public override void Stop()
    {
        base.Stop();
        try
        {
            if (ParticleModel!= null && Particle != null && ParticleSystems.Length > 0)
            {
                foreach (ParticleSystem ps in ParticleSystems)
                    ps?.Stop(true, ParticleSystemStopBehavior.StopEmitting);

                Particle?.SetActive(false);
            }
        }
        catch (System.Exception e)
        {
            //Debug.LogError("EXCEPTION STOP OBJ " + ParticleModel.name + ", " + e.Message);
        }
    }
    
    public virtual bool IsPlaying()
    {
        return ParticleSystems.Length > 0 ? ParticleSystems[0]?.isPlaying ?? false : false;
    }

    public virtual float PlayLength()
    {
        return ParticleSystems.Length > 0 ? ParticleSystems[0]?.duration ?? -1 : -1;
    }

    public virtual void SetVisualSoundEffect(VisualParticleEvent pVisualSoundEffect)
    {
        base.SetVisualEffect(pVisualSoundEffect);
        ParticleModel = pVisualSoundEffect.ParticleModel;
    }
}

[System.Serializable]
public class VisualEvent
{
    public bool Continuous;
    protected GameObject Owner;
    public AudioClip SFX;
    // Should be private or protected
    protected AudioSource AudioSource;

    public string AnimationString;
    protected Animator Animator;

    private uint AudioID; // CHECK

    public virtual void Play()
    {
        if (SFX != null && AudioSource != null && AudioSource?.clip != SFX)
        {
            if (Continuous)
            {
                this.AudioSource.clip = SFX;
                AudioSource?.Play();
            }
            else
                AudioSource?.PlayOneShot(SFX);

            if(Animator != null && AnimationString != "")
            {
                Animator?.Play(AnimationString);
            }
        }
    }

    public virtual void Stop()
    {
        if (SFX != null && AudioSource != null)// && AudioSource.isPlaying)
        {
            AudioSource.Stop();
            AudioSource.clip = null;
        }
    }

    public virtual void SetVisualEffect(VisualEvent pVisualEffect)
    {
        Continuous = pVisualEffect.Continuous;
        SFX = pVisualEffect.SFX;
        AudioSource = pVisualEffect.AudioSource;
        AnimationString = pVisualEffect.AnimationString;
        AudioID = pVisualEffect.AudioID;
    }

    // GETS AND SETS
    public virtual void SetAudioSource(AudioSource pAudioSource)
    {
        AudioSource = ((pAudioSource != null && SFX != null) ? pAudioSource : null);
    }
    public virtual void SetAnimator(Animator pAnimator)
    {
        Animator = pAnimator;
    }
    public virtual AudioSource GetAudioSource() { return AudioSource; }

    public virtual void SetOwner(GameObject pOwner) { Owner = pOwner; }
}