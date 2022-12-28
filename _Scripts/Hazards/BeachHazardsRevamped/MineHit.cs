using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MineHit : MonoBehaviour
{
    public AudioClip Explosion;
    public AudioClip Spawn;
    public AudioClip Trigger;

    public MineHazard MineHazard;
    public List<PlayerController> Players { get { return Camera.main.GetComponent<CameraFollow>().Players.Where(a => a != null).ToList(); } }
    public bool Active
    {
        set
        {
            if (Triggered && !CountDownStarted)
            {
                StartCoroutine(StartCountDown());
                //Debug.Log("Set Active Countdown started");
            }
            mActive = value;
        } get
        {
            return mActive;
        }
    }
    public bool Thrown;
    private bool mActive;
    public bool Triggered;
    private bool CountDownStarted;
    public float Radius;
    public float Force;
    public float WarningTime;
    [HideInInspector]
    public float StartTime;

    public GameObject HookedParticleSystem;
    public GameObject RangeVisualParticleSystem;
    public GameObject TriggeredParticleSystem;
    public GameObject ExplodeParticleSystem;

    private GameObject HookedParticleSandMound;
    private GameObject mRangeVisualParticleSystem;
    private GameObject mTriggeredParticleSystem;

    private GameObject IgnoreGO;
    private EventInstance SoundEvent;

    public IEnumerator Initialize()
    {
        //BeachHazardManager.Instance.PlaySound(Spawn);
        HookedParticleSandMound = null;
        AudioManager.Instance.PlayEventWithParameter(AudioManager.Instance.HazardSound.SeaMine, Constants.FmodParameters.SeaMine, 0f, out SoundEvent);
        mRangeVisualParticleSystem = Instantiate(RangeVisualParticleSystem, transform);
        mRangeVisualParticleSystem.transform.localPosition = Vector3.zero;
        yield return new WaitForSeconds(StartTime);
        Active = true;
        GetComponent<Hookable>().IsAvailable = true;

    }

    private IEnumerator StartCountDown()
    {
        //Debug.Log("Count down started!");
        //BeachHazardManager.Instance.PlaySound(Trigger);
        AudioManager.Instance.ChangeEventParameter(Constants.FmodParameters.SeaMine, 1f, SoundEvent);
        CountDownStarted = true;
        mTriggeredParticleSystem = Instantiate(TriggeredParticleSystem, transform);
        mTriggeredParticleSystem.transform.localPosition = Vector3.zero;
        Destroy(mTriggeredParticleSystem, WarningTime);
        yield return new WaitForSeconds(WarningTime);
        Explode();
    }

    public void Throw(GameObject Thrower)
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.PlayerSound.GetHit);
        if (Triggered)
        {
            SoundEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        Thrown = true;
        Active = false;
        IgnoreGO = Thrower;
        Destroy(mRangeVisualParticleSystem);
        Destroy(mTriggeredParticleSystem);
        if (HookedParticleSandMound == null)
        {
            Ray ray = new Ray(transform.position + (Vector3.up * 2), Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10, 1 << 10/*ground layer*/))
            {
                HookedParticleSandMound = Instantiate(HookedParticleSystem, hit.point, Quaternion.identity);
            }
            else
            {
                HookedParticleSandMound = Instantiate(HookedParticleSystem, transform.position, Quaternion.identity);
            }
           
        }
        GetComponent<Rigidbody>().drag = 1.3f;
        Destroy(HookedParticleSandMound, 4f);
    }

    public void Explode(bool ignoreActive = false)
    {
        transform.SetParent(null);
        GetComponent<MineHookable>().OnFinishedBeingHooked();
        if (mRangeVisualParticleSystem != null)
        {
            Destroy(mRangeVisualParticleSystem);
        }
        if (mTriggeredParticleSystem != null)
        {
            Destroy(mTriggeredParticleSystem);
        }
        if (!Active && !ignoreActive)
        {
            return;
        }

        //BeachHazardManager.Instance.PlaySound(Explosion);
        SoundEvent.getPlaybackState(out PLAYBACK_STATE isPlaying);
        if (isPlaying == PLAYBACK_STATE.PLAYING)
        {
            AudioManager.Instance.ChangeEventParameter(Constants.FmodParameters.SeaMine, 2f, SoundEvent);
            //Debug.Log("Explosion sound!");
        }
        else
        {
            AudioManager.Instance.PlayEventWithParameter(AudioManager.Instance.HazardSound.SeaMine, Constants.FmodParameters.SeaMine, 2f, out SoundEvent);
        }
        var PlayersInRange = Players.Where(a => (a.transform.position - transform.position).sqrMagnitude <= Radius * Radius);
        
        //PlayersInRange.ToList().ForEach(a => a.Hookable.Push(((Vector3.up * 2) + (new Vector3(a.transform.position.x, 0, a.transform.position.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized).normalized, Force));
        foreach (var item in PlayersInRange)
        {
            item.Hookable.Push(((Vector3.up * 2) + (new Vector3(item.transform.position.x, 0, item.transform.position.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized).normalized, Force);
            HazardEvent hazInfo = new HazardEvent
            {
                Description = "Mine Hit Hazard Event",
                HazardType = AchHazardType.SeaMine,
                PlayerHit = item.gameObject,
                RewiredID = item.RewiredID,
                Skin = item.Skin,
            };
            hazInfo.FireEvent();
        }

        var ps = Instantiate(ExplodeParticleSystem, transform.position, Quaternion.identity);
        Destroy(ps, 4);
        Triggered = false;
        Active = false;
        Thrown = false;
        GetComponent<Hookable>().IsAvailable = false;
        CountDownStarted = false;
        StopAllCoroutines();
        MineHazard.DisableMine(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerHookable>();
        if (player != null && !Triggered)
        {
            Triggered = true;
            if (Active && !CountDownStarted)
            {
                StartCoroutine(StartCountDown());
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Active)
        {
            Explode();
        }
        else if (Thrown && collision.collider.gameObject != IgnoreGO)
        {
            Explode(true);
        }
    }
}
