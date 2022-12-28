using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Rewired;

public class SoundVolume : MonoBehaviour {

    [System.Serializable]
    public enum Channels { Music, SFX }

    public Channels ChannelControlling;

    //public Button OriginButton;
    private Scrollbar ScrollBar;
    private string ChannelName;
    private EventSystem ES;
    private FMOD.Studio.Bus Mixer;
    private float InputTime = 0.5f;
    private float WaitTime = 0;

    // Use this for initialization
    void Start() {
        ES = FindObjectOfType<EventSystem>();
        ScrollBar = GetComponentInChildren<Scrollbar>();

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

    public void OnValueChanged()
    {
        AudioManager.Instance.ChangeVolume(ChannelName, Mixer, ScrollBar.value);
        if (ChannelControlling == Channels.SFX)
        {
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.ButtonClick);
        }
    }
}
