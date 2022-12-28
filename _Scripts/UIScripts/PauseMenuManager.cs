using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Rewired;
using UnityEngine.UI;
using System.Linq;

public class PauseMenuManager : MonoBehaviour 
{
    private EventSystem UIEventSystem;
    private bool InOptionsMenu = false;

    //public Player ControllingPlayer { get; private set; }
    public GameObject TitleFirstSelected;
    public GameObject OptionsFirstSelected;
    public GameObject PauseTitleMenu;
    public GameObject PauseOptionsMenu;
    public Scrollbar MusicScrollbar;
    public Scrollbar SFXScrollbar;
    //public Image LeftArrow;
    //public Image RightArrow;
    public bool CanGoBack = true;
    public Image ControllerMapImage;
    public List<Sprite> ControllerMapSprites;
    private float Timer = 0;
    [System.Serializable]
    public class ControlMapArrows
    {
        public Image Left;
        public Image Right;
        public Sprite LeftArrow;
        public Sprite RightArrow;
        public Sprite LeftArrowHighlight;
        public Sprite RightArrowHighlight;
    }
    public ControlMapArrows Arrows;
    private GameObject CurrentSelected;

    private FMOD.Studio.EventInstance LevelMusic;

    private void Awake() 
	{
        UIEventSystem = FindObjectOfType<EventSystem>();
        LevelMusic = FindObjectOfType<LevelAudio>().Music;
    }

    private void Update()
    {
        Timer += Time.unscaledDeltaTime;
        for (int i = 0; i < ReInput.players.playerCount; i++)
        {


            var ChangeMaps = ReInput.players.GetPlayer(i).GetAxis("MenuHorizontal");
            if (Mathf.Abs(ChangeMaps) >= 0.6f && Timer > 0.3f)
            {
                StartCoroutine(HighlightControlMapArrow(Arrows, ChangeMaps));
                var prevSprite = ControllerMapImage.sprite;
                ControllerMapImage.sprite = ControllerMapSprites.FirstOrDefault(a => a != prevSprite);
                Timer = 0f;
            }

            if (ReInput.players.GetPlayer(i).GetButtonDown("MenuBack") || ReInput.players.GetPlayer(i).GetButtonDown("UnpauseGame"))
            {
                if (InOptionsMenu && CanGoBack)
                {
                    SetOptionsMenuActive(false);
                }
                else if (!InOptionsMenu)
                {
                    GameManager.Instance.SetGamePaused(false);
                }
            }
        }
        //if (ControllingPlayer.GetButtonDown("UnpauseGame"))
        //{
        //    GameManager.Instance.SetGamePaused(ControllingPlayer.id, false);
        //}

        if (CurrentSelected != UIEventSystem.currentSelectedGameObject && UIEventSystem.currentSelectedGameObject != null)
        {
            CurrentSelected = UIEventSystem.currentSelectedGameObject;
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.ButtonClick);
        }
    }

    public void Initialize()
    {
        PauseTitleMenu.SetActive(true);
        PauseOptionsMenu.SetActive(false);
        StartCoroutine(OnMenuEnabled(TitleFirstSelected));
    }

    private IEnumerator OnMenuEnabled(GameObject pFirstselected)
    {
        UIEventSystem.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        UIEventSystem.SetSelectedGameObject(pFirstselected);
    }

    public void SetOptionsMenuActive(bool pValue)
    {
        PauseTitleMenu.SetActive(!pValue);
        PauseOptionsMenu.SetActive(pValue);
        if (pValue)
        {
            StartCoroutine(OnMenuEnabled(OptionsFirstSelected));
        }
        else
        {
            AudioManager.Instance.ChangeEventParameter(Constants.FmodParameters.PauseSnapshot, 1f, LevelMusic);
            StartCoroutine(OnMenuEnabled(TitleFirstSelected));
        }
        InOptionsMenu = pValue;
    }

    private IEnumerator HighlightControlMapArrow(ControlMapArrows pOutfitArrows, float pDir)
    {
        Image arrowToChange;
        Sprite highlightSprite;
        if (pDir < 0)
        {
            arrowToChange = pOutfitArrows.Left;
            highlightSprite = pOutfitArrows.LeftArrowHighlight;
        }
        else
        {
            arrowToChange = pOutfitArrows.Right;
            highlightSprite = pOutfitArrows.RightArrowHighlight;
        }

        var prevSprite = arrowToChange.sprite;
        arrowToChange.sprite = highlightSprite;
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.ChangeOutfit);
        yield return new WaitForSecondsRealtime(0.1f);
        arrowToChange.sprite = prevSprite;
    }

    public void ResumeOnClick()
    {
        GameManager.Instance.SetGamePaused(false);
    }

    public void ReturnToMainMenuButton()
    {
        GameManager.Instance.ResetDifficulty();
        UIManager.Instance.ReturnToMainMenu();
    }

    public void ReturnToMapandModeButton()
    {
       UIManager.Instance.ReturnToMapandMode();
    }
}
