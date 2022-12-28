using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using Rewired;

public class GameEndUIController : MonoBehaviour {

    [SerializeField]
    private Image NextLevelImg;
    [SerializeField]
    private TextMeshProUGUI NextLevelText;
    public GameObject FirstSelected;
    private EventSystem EventSystem;
    private GameObject CurrentSelected;
    [SerializeField]
    private List<Sprite> LevelSprites;
    [SerializeField]
    private List<string> LevelNames;
    private int NextLevelIndex;
    private TournamentManager TournamentManager;
    [SerializeField]
    private GameObject MenuSelection;
    [SerializeField]
    private GameObject ButtonPrompt;
    [SerializeField]
    private RoundStartUI RoundStartUI;
    private bool TournamentOverPromptActive;
    [SerializeField]
    private Image RightArrow;
    [SerializeField]
    private Image LeftArrow;
    public Color ArrowStandard;
    public Color ArrowHighlight;
    private float PressTimer = 0;
    private int CurrentLevelIndex;
    private bool InputEnabled;
    private int SelectionWhenRandomizing;

    private void Awake()
    {
        EventSystem = FindObjectOfType<EventSystem>();
        var nextLvl = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextLvl > LevelSprites.Count)
        {
            NextLevelIndex = 1;
        }
        else
        {
            NextLevelIndex = nextLvl;
        }
        CurrentLevelIndex = NextLevelIndex;
        InputEnabled = false;

        if (GameManager.Instance.Tournament)
        {
            TournamentManager = FindObjectOfType<TournamentManager>();
            if (TournamentManager.TournamentOver())
            {
                //enable stuff to show if tournament is over
                FirstSelected = null;
                MenuSelection.SetActive(false);
                ButtonPrompt.SetActive(true);
                StartCoroutine(ButtonPromptCoroutine());
            }
            else
            {
                //enable stuff to show if tournament is ongoing
                AssignNextLevel(CurrentLevelIndex);
                MenuSelection.SetActive(true);
                ButtonPrompt.SetActive(false);
            }
        }
        else
        {
            //enable stuff to show in "quick play"
            AssignNextLevel(CurrentLevelIndex);
            MenuSelection.SetActive(true);
            ButtonPrompt.SetActive(false);
        }
    }

    private void Start()
    {
        StartCoroutine(DelayedButtonSelection());
    }

    private IEnumerator DelayedButtonSelection()
    {
        yield return new WaitForSeconds(1.5f);
        InputEnabled = true;
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.ButtonClick);
        EventSystem.SetSelectedGameObject(FirstSelected);
        CurrentSelected = FirstSelected;
    }

    private IEnumerator ButtonPromptCoroutine()
    {
        foreach (var rePlayer in ReInput.players.GetPlayers())
        {
            rePlayer.controllers.maps.SetMapsEnabled(true, "Menus");
        }

        while (true)
        {
            yield return null;
            foreach (var rePlayer in ReInput.players.GetPlayers())
            {
                if (rePlayer.GetButtonDown("MenuBack"))
                {
                    ReturnToMainMenuButton();
                    //StopCoroutine(ButtonPromptCoroutine());
                    yield break;
                }
            }
        }
    }

    private void Update()
    {
        PressTimer += Time.deltaTime;

        if (CurrentSelected != EventSystem.currentSelectedGameObject)
        {
            CurrentSelected = EventSystem.currentSelectedGameObject;
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.ButtonClick);
        }

        if (InputEnabled)
        {
            foreach (var player in GameManager.Instance.PlayerDataModels)
            {
                var axis = ReInput.players.GetPlayer(player.RewiredID).GetAxis("MenuHorizontal");

                if (Mathf.Abs(axis) >= 0.5f && PressTimer > 0.3f)
                {
                    ChangeLevel(axis);
                    PressTimer = 0;
                }
            }
        }
    } 

    public void ReturnToMainMenuButton()
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.ButtonClick);
        UIManager.Instance.ReturnToMainMenu();
    }

    public void RematchButton()
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.ButtonClick);
        StartCoroutine(RematchWithAnimation());
    }

    private IEnumerator RematchWithAnimation()
    {
        RoundStartUI.ScoreboardAnimator.SetTrigger("Rematch");
        RoundStartUI.WinnerFlare.GetComponent<Animator>().SetTrigger("FadeOut");
        yield return new WaitForSeconds(0.6f);

        GameManager.Instance.StartGame();
    }

    private IEnumerator HighlightArrow(float pAxis)
    {
        Image arrowToChange;

        if (pAxis > 0)
        {
            arrowToChange = RightArrow;
        }
        else
        {
            arrowToChange = LeftArrow;
        }

        arrowToChange.color = ArrowHighlight;
        yield return new WaitForSeconds(0.1f);
        arrowToChange.color = ArrowStandard;
    }

    public void ChangeLevel(float pAxis)
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.ChangeOutfit);
        if (pAxis > 0)
        {
            CurrentLevelIndex += 1;
            if (CurrentLevelIndex > 6)
            {
                CurrentLevelIndex = 1;
            }
        }
        else if (pAxis < 0)
        {
            CurrentLevelIndex -= 1;
            if (CurrentLevelIndex < 1)
            {
                CurrentLevelIndex = 6;
            }
        }
        AssignNextLevel(CurrentLevelIndex);
        StartCoroutine(HighlightArrow(pAxis));
    }

    private void RandomizeLevel()
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.ChangeOutfit);
        CurrentLevelIndex = RandomLvlIndex();
        AssignNextLevel(CurrentLevelIndex);
    }

    private int RandomLvlIndex()
    {
        var rng = Random.Range(1, 7); // 1-6, int non-inclusive
        if (rng == CurrentLevelIndex)
        {
            return RandomLvlIndex();
        }
        else
        {
            return rng;
        }
    }

    public void RandomizeLevelButton()
    {
        SelectionWhenRandomizing = CurrentLevelIndex;
        StartCoroutine(RandomizeMaps());
    }

    /// <summary>
    /// Randomizes map 5 times. If the map picked is the same as you had selected when pressing randomize, randomizes once more to make sure its not the same.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RandomizeMaps()
    {
        InputEnabled = false;
        yield return new WaitForSeconds(0.05f);
        RandomizeLevel();
        yield return new WaitForSeconds(0.05f);
        RandomizeLevel();
        yield return new WaitForSeconds(0.05f);
        RandomizeLevel();
        yield return new WaitForSeconds(0.05f);
        RandomizeLevel();
        yield return new WaitForSeconds(0.05f);
        RandomizeLevel();
        if (CurrentLevelIndex == SelectionWhenRandomizing)
        {
            RandomizeLevel();
        }
        InputEnabled = true;
    }

    // CurrentLevelIndex 1-6 like build index. NextLevelIndex 0-5 by list of maps (currentindex -1)
    private void AssignNextLevel(int pCurrentLevelIndex)
    {
        NextLevelIndex = pCurrentLevelIndex;
        NextLevelImg.sprite = null;
        NextLevelImg.sprite = LevelSprites[NextLevelIndex - 1];
        NextLevelText.text = LevelNames[NextLevelIndex - 1];
    }

    public void PlayNextLevelButton()
    {
        GameManager.Instance.LoadScreenAndLoadScene(NextLevelIndex);
    }
}  