using Rewired;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsMenuManager : MonoBehaviour 
{
    public Button MusicButton;
    public Button SFXButton;
    public Scrollbar MusicScrollBar;
    public Scrollbar SFXScrollBar;
    public TMPro.TMP_Dropdown ResolutionDropdown;
    public Toggle FullscreenToggle;
    bool Fullscreen;
    private Camera Camera;

    private int ResolutionIndex;
    private List<Resolution> Resolutions = new List<Resolution>();
    private EventSystem ES;

	private void Awake() 
	{
        ES = FindObjectOfType<EventSystem>();
        MusicButton.onClick.AddListener(MusicButtonPressed);
        SFXButton.onClick.AddListener(SFXButtonPressed);
        Fullscreen = Screen.fullScreen;
        FullscreenToggle.isOn = Fullscreen;
        Camera = Camera.main;
    }

    private void OnEnable()
    {
        GetResolutions();
        FullscreenToggle.isOn = Screen.fullScreen;
    }

    private void Update()
    {
        if (ResolutionDropdown.IsExpanded)
        {
            MainMenuNavigation.CanGoBack = false;
            for (int i = 0; i < ReInput.players.playerCount; i++)
            {
                var player = ReInput.players.GetPlayer(i);
                if (player.GetButtonDown("MenuBack"))
                {
                    var val = ResolutionDropdown.value;
                    ResolutionDropdown.value = val;
                    MainMenuNavigation.CanGoBack = true;
                }
            }
        }
        else
        {
            MainMenuNavigation.CanGoBack = true;
        }
    }

    public void MusicButtonPressed()
    {
        MusicScrollBar.Select();
    }

    public void SFXButtonPressed()
    {
        SFXScrollBar.Select();
    }

    private void GetResolutions()
    {
        Resolutions = Screen.resolutions.ToList();
        Resolutions.Reverse();
        ResolutionDropdown.ClearOptions();
        Resolutions.Remove(Resolutions.Find(a => a.height == 600));
        
        List<string> options = new List<string>();
        var currentResolutionIndex = 0;

        for (int i = 0; i < Resolutions.Count; i++)
        {
            string option = Resolutions[i].width + " x " + Resolutions[i].height;
            options.Add(option);
            if (Resolutions[i].width == Screen.width &&
                Resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
                ResolutionIndex = i;
            }
        }
        ResolutionDropdown.AddOptions(options);

        ResolutionDropdown.value = currentResolutionIndex;
        ResolutionDropdown.RefreshShownValue();
        

    }

    public void SetResolution()
    {
        ResolutionIndex = ResolutionDropdown.value;
    }

    public void SetFullscreen(bool pIsFullscreen)
    {
        Screen.fullScreen = pIsFullscreen;
    }

    public void FullscreenToggleChange()
    {
        Fullscreen = FullscreenToggle.isOn;
    }

    public void ApplyButton()
    {
        StartCoroutine(ChangeResolution());
    }

    private IEnumerator ChangeResolution()
    {
        Resolution resolution = Resolutions[ResolutionIndex];
        yield return new WaitForEndOfFrame();
        Screen.SetResolution(resolution.width, resolution.height, Fullscreen);
        Camera.ResetAspect();
        yield return new WaitForEndOfFrame();
        Camera.GetComponent<CamViewPortTest>().UpdateCamOnResolutionChange();
    }
}
