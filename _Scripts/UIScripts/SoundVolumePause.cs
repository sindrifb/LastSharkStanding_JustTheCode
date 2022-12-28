using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SoundVolumePause : MonoBehaviour 
{
    [System.Serializable]
    public enum Channels { Music, SFX }

    public Channels ChannelControlling;
    public PauseMenuManager PauseManager;

    //public Button OriginButton;
    private Scrollbar ScrollBar;
    private string ChannelName;
    private EventSystem ES;
    private FMOD.Studio.Bus Mixer;
    private FMOD.Studio.EventInstance LevelMusic;


    // Use this for initialization
    void Start()
    {
        ES = FindObjectOfType<EventSystem>();
        ScrollBar = GetComponentInChildren<Scrollbar>();
        LevelMusic = FindObjectOfType<LevelAudio>().Music;

        switch (ChannelControlling)
        {
            case Channels.Music:
                ChannelName = Constants.AudioMixerChannels.Music;
                Mixer = AudioManager.Instance.MusicMixer;
                break;
            case Channels.SFX:
                ChannelName = Constants.AudioMixerChannels.SFX;
                Mixer = AudioManager.Instance.SFXMixer;
                break;
        }

        ScrollBar.value = PlayerPrefs.GetFloat(ChannelName, 1);
    }
    public void Update()
    {
        if (ES.currentSelectedGameObject == ScrollBar.gameObject)
        {
            //PauseManager.CanGoBack = false;
            if (ChannelControlling == Channels.Music)
            {
                AudioManager.Instance.ChangeEventParameter(Constants.FmodParameters.PauseSnapshot, 0f, LevelMusic);
            }
            else
            {
                AudioManager.Instance.ChangeEventParameter(Constants.FmodParameters.PauseSnapshot, 1f, LevelMusic);
            }
        }
    }

    public void OnValueChanged()
    {
        AudioManager.Instance.ChangeVolume(ChannelName, Mixer, ScrollBar.value);
        if (ChannelControlling == Channels.SFX)
        {
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.ButtonClick);
        }
    }
}
