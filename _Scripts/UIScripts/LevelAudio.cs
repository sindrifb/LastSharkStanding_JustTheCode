using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAudio : MonoBehaviour
{

    [EventRef]
    public string LevelMusic;
    [EventRef]
    public string WinScreenMusic;

    public EventInstance Music;
    
	private void Awake ()
    {
        EventManager.StartListening(EventManager.EventCodes.GameEnd, PlayWinScreenMusic);
        //Debug.Log("GameEnd started listening");
        EventManager.StartListening(EventManager.EventCodes.GameStart, PlayLevelMusic);
        //Debug.Log("GameStart started listening");
    }

    public void PlayLevelMusic()
    {
  
        Music.getPlaybackState(out PLAYBACK_STATE state);
        if (state == PLAYBACK_STATE.PLAYING)
        {
            AudioManager.Instance.StopEvent(Music, FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        //Debug.Log("Play Level Music");
        
        AudioManager.Instance.PlayEvent(LevelMusic, out Music);
    }

    public void PlayWinScreenMusic()
    {
        StartCoroutine(PlayMusic());
    }

    private IEnumerator PlayMusic()
    {
        // Delayed by 2.5sec to fit the new end screen animations
        yield return new WaitForSeconds(1.5f);
        AudioManager.Instance.StopEvent(Music, FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        AudioManager.Instance.PlayEvent(WinScreenMusic, out Music);
    }

    private void OnDestroy()
    {
        Music.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }
}
