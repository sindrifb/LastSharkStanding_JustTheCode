using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
   
    [System.Serializable]
    public class PlayerSounds
    {
        [EventRef]
        public string GetHit;
        [EventRef]
        public string StandUp;
        [EventRef]
        public string FallOff;
        [EventRef]
        public string Dash;
        [EventRef]
        public string Fling;
        [EventRef]
        public string Land;
        [EventRef]
        public string Spawn;
        [EventRef]
        public string Death;
        [EventRef]
        public string Footstep;
    }
    public PlayerSounds PlayerSound;
    [System.Serializable]
    public class HookSounds
    {
        [EventRef]
        public string HookThrow;
        [EventRef]
        public string HookHit;
        [EventRef]
        public string AnchorHit;
        [EventRef]
        public string AnchorThrow;
        [EventRef]
        public string HarpoonThrow;
        [EventRef]
        public string PowerUpPickup;
        [EventRef]
        public string PowerUpSpawn;
    }
    public HookSounds HookSound;
    [System.Serializable]
    public class HazardSounds
    {
        [Header("Beach Level")]
        [EventRef]
        public string SeaMine;
        [EventRef]
        public string Wave;
        [EventRef]
        public string BeachBallFall;
        [EventRef]
        public string BeachBallLand;
        [EventRef]
        public string WaveHit;

        [Header("Pirate Ship")]
        [EventRef]
        public string CannonFuse;
        [EventRef]
        public string CannonFire;
        [EventRef]
        public string GraterOpen;
        [EventRef]
        public string GraterClose;

        [Header("Volcano Level")]
        [EventRef]
        public string PlatformSink;
        [EventRef]
        public string LavaHit;

        [Header("Space Level")]
        [EventRef]
        public string AirlockOpen;
        [EventRef]
        public string AirlockClose;
        [EventRef]
        public string ThrownOut;
        [EventRef]
        public string AirlockWarning;
        [EventRef]
        public string AirlockSuction;

        [Header("Castle Level")]
        [EventRef]
        public string BarrageImpact;
        [EventRef]
        public string BarrageWarningShot;
        [EventRef]
        public string CatapultHeavyLanding;

        [Header("Temple Level")]
        [EventRef]
        public string TempleWallFire;
    }
    public HazardSounds HazardSound;
    [System.Serializable]
    public class UISounds
    {
        [EventRef]
        public string ButtonClick;
        [EventRef]
        public string LobbyReady;
        [EventRef]
        public string LobbyPlayerLeave;
        [EventRef]
        public string ChangeOutfit;
        [EventRef]
        public string RoundStartIn3;
        [EventRef]
        public string RoundStartIn5;
        [EventRef]
        public string WaveTransition;
        [EventRef]
        public string FingerClick;
        [EventRef]
        public string GainScore;
        [EventRef]
        public string GainTrophy;
        [EventRef]
        public string GainCrown;
        [EventRef]
        public string Scoreboard_Woosh;
        [EventRef]
        public string Scoreboard_BorderPop;
    }
    public UISounds UISound;

    [System.Serializable]
    public class EnvironmentSounds
    {
        [EventRef]
        public string StoneScrape;
        [EventRef]
        public string Temple_PillarUp;
        [EventRef]
        public string Temple_PillarDown;
    }
    public EnvironmentSounds EnvironmentSound;

    [System.Serializable]
    public class PowerupSounds
    {
        [Header("Powerups")]
        [EventRef]
        public string ImplosionGrenade;
        [EventRef]
        public string Explosion;
        [EventRef]
        public string GroundBounce;
        [EventRef]
        public string BoomerangThrow;
        [EventRef]
        public string PowerupPickup;
        [EventRef]
        public string BubblePop;
    }
    public PowerupSounds PowerupSound;

    public Bus SFXMixer;
    public Bus MusicMixer;
    public Bus GameplaySFX;

    public static AudioManager Instance;
    private FMOD.Studio.System StudioSystem;
    private FMOD.System LowLevelSystem;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StudioSystem = RuntimeManager.StudioSystem;
        StudioSystem.getCPUUsage(out CPU_USAGE cpuUsage);

        LowLevelSystem = RuntimeManager.LowlevelSystem;
        LowLevelSystem.getVersion(out uint version);

        MusicMixer = RuntimeManager.GetBus(Constants.FmodParameters.MusicMixer);
        SFXMixer = RuntimeManager.GetBus(Constants.FmodParameters.SFXMixer);
        GameplaySFX = RuntimeManager.GetBus(Constants.FmodParameters.GameplaySFXMixer);
        
        ChangeVolume(Constants.AudioMixerChannels.Music, MusicMixer, PlayerPrefs.GetFloat(Constants.AudioMixerChannels.Music, 1));
        ChangeVolume(Constants.AudioMixerChannels.SFX, SFXMixer, PlayerPrefs.GetFloat(Constants.AudioMixerChannels.SFX, 1));

    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.F))
    //    {
    //        PlayEventWithParameter(HazardSound.SeaMine, "SeaMine", 0f, out EventInstance testEvent);
    //        StartCoroutine(TestInvoke(testEvent));
    //    }
    //}

    //private IEnumerator TestInvoke(EventInstance pEvent)
    //{
    //    yield return new WaitForSeconds(0.7f);
    //    ChangeEventParameter("SeaMine", 1f, pEvent);
    //    yield return new WaitForSeconds(0.7f);
    //    ChangeEventParameter("SeaMine", 2f, pEvent);

    //    yield return null;
    //}

    public void PlayOneShot(string pEvent)
    {
        RuntimeManager.PlayOneShot(pEvent);
    }

    public void PlayEventWithParameter(string pPath, string pParameterName, float pParamaterValue, out EventInstance pEvent, float pReleaseTime = 10f)
    {
        pEvent = RuntimeManager.CreateInstance(pPath);
        pEvent.setParameterValue(pParameterName, pParamaterValue);
        pEvent.start();
        StartCoroutine(ReleaseEvent(pEvent, pReleaseTime));
    }

    public void PlayEvent(string pPath, out EventInstance pEvent, float pReleaseTime = 10f)
    {
        pEvent = RuntimeManager.CreateInstance(pPath);
        pEvent.start();
        StartCoroutine(ReleaseEvent(pEvent, pReleaseTime));
    }

    public void StopEvent(EventInstance pEvent, FMOD.Studio.STOP_MODE pStop_Mode)
    {
        pEvent.stop(pStop_Mode);
    }

    public void ChangeEventParameter(string pParameterName, float pParameterValue, EventInstance pEvent)
    {
        //pEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        pEvent.setParameterValue(pParameterName, pParameterValue);
        //pEvent.start();
        //pEvent.release();
    }

    public void PauseSounds(bool pValue)
    {
        GameplaySFX.setPaused(pValue);
        var levelaudio = FindObjectOfType<LevelAudio>();
        if (levelaudio != null)
        {
            var music = levelaudio.Music;

            if (pValue)
            {
                ChangeEventParameter(Constants.FmodParameters.PauseSnapshot, 1f, music);
            }
            else
            {
                ChangeEventParameter(Constants.FmodParameters.PauseSnapshot, 0f, music);
            }
        }
    }

    public void ChangingScenes()
    {
        GameplaySFX.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        GameplaySFX.setPaused(false);
    }

    private IEnumerator ReleaseEvent(EventInstance pEvent, float pReleaseTime)
    {
        yield return new WaitForSeconds(pReleaseTime);
        pEvent.release();
    }

    //FMOD volume level set as a linear gain. 0 = silent, 1 = full volume.
    public void ChangeVolume(string pChannelName, Bus pMixer, float pSliderValue)
    {
        PlayerPrefs.SetFloat(pChannelName, pSliderValue);
        pMixer.setVolume(pSliderValue);
    }
}
