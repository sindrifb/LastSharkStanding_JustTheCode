using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using Rewired;
using UnityEngine.EventSystems;

[System.Serializable]
public enum MenuScreens { MainTitle, MainMenu, Options, Lobby, MapAndModes, WinScreen, EndScreen, Credits}

public class MenusManager : MonoBehaviour
{
    public static MenusManager Instance;

    public GameObject BaseBackground;
    public GameObject WaterAnim;
    public GameObject LoadingScreen;
    public float LoadingTimer = 3.5f;
   

    public GameObject[] MenusList;

    private Coroutine NextMenuCoroutine;

    private EventSystem EventSystem;
    private GameObject currentObj;

    [HideInInspector]
    public bool IsTransitioning;

    private void Awake()
    {
        Cursor.visible = false;
        Instance = this;
        EventSystem = FindObjectOfType<EventSystem>();
    }

    private void Start()
    {
        Time.timeScale = 1;
        foreach (var player in ReInput.players.AllPlayers)
        {
            player.controllers.maps.SetMapsEnabled(true, "Menus");
        }

        UIManager.Instance.Init();
    }

    private void Update()
    {
        if (currentObj != EventSystem.currentSelectedGameObject)
        {
            currentObj = EventSystem.currentSelectedGameObject;
            PlayChangedOptionEffect();
        }
        
    }

    public void GoToMenu(MenuScreens pMenuID)
    {
        //if (pMenuID == MenuScreens.MapAndModes)
        //{
        //    WaterAnim.GetComponent<Animator>().SetTrigger("play");
        //}
        GoToMenu((int)pMenuID);
    }

    public void GoToMenu(int pMenuID)
    {
        if (pMenuID >= 0 && pMenuID < MenusList.Length && !IsTransitioning)
        {
            IsTransitioning = true;
            EventSystem.enabled = false;
            if (NextMenuCoroutine != null)
            {
                StopCoroutine(NextMenuCoroutine);
            }

            if (pMenuID == 1)
            {
                GameManager.Instance.Tournament = false;
            }

            NextMenuCoroutine = StartCoroutine(showMenu(pMenuID));
        }
    }

    public void GoToMenuWithoutTransition(int pMenuID)
    {
        if (pMenuID >= 0 && pMenuID < MenusList.Length)
        {

            if (pMenuID == 1)
            {
                GameManager.Instance.Tournament = false;
            }

            DisableAllMenus();
            MenusList[pMenuID]?.SetActive(true);
        }
    }

    public IEnumerator showMenu(int pMenuID)
    {
        WaterAnim?.GetComponent<Animator>()?.SetTrigger("ChangeMenu");
        //WaterAnim?.GetComponent<AudioSource>()?.Play();
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.WaveTransition);
        yield return new WaitForSeconds(0.5f);
        DisableAllMenus();
        MenusList[pMenuID]?.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        EventSystem.enabled = true;
        IsTransitioning = false;
    }

    public void InstantiateMenu(MenuScreens pMenuID)
    {
        InstantiateMenu((int)pMenuID);
    }

    public void InstantiateMenu(int pMenuID)
    {
        Canvas pCanvas = FindObjectOfType<Canvas>();
        bool pAlreadyThere = false;

        foreach (Transform e in pCanvas.GetComponentsInChildren<Transform>())
        {
            pAlreadyThere = (e.name == MenusList[pMenuID].name || e.name  == MenusList[pMenuID].name + " (clone)");
        }

        if (!pAlreadyThere)
            if (MenusList[pMenuID] != null)
                Instantiate(MenusList[pMenuID]);
            else
                //Debug.LogError("Menu " + (MenuScreens)pMenuID + " not assigned");

        GoToMenu(pMenuID);
    }

    public void DisableAllMenus()
    {
        foreach (GameObject g in MenusList)
            g.SetActive(false);
    }

    //public void LoadScene(string pSceneName)
    //{
    //    StartCoroutine(LoadSceneInTime(SceneManager.GetSceneByName(pSceneName).buildIndex, LoadingTimer));
    //}

    //public void LoadScene(int pSceneIndex)
    //{
    //    StartCoroutine(LoadSceneInTime(pSceneIndex, LoadingTimer));
    //}

    public void ReloadCurrentScene()
    {
        //StartCoroutine(GameManager.Instance.LoadScreenAndLoadScene(SceneManager.GetActiveScene().buildIndex));
        GameManager.Instance.LoadScreenAndLoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadPirateLevelButton()
    {
        //StartCoroutine(GameManager.Instance.LoadScreenAndLoadScene(2));
        GameManager.Instance.LoadScreenAndLoadScene(2);
    }

    public void LoadBeachLevelButton()
    {
        //StartCoroutine(GameManager.Instance.LoadScreenAndLoadScene(1));
        GameManager.Instance.LoadScreenAndLoadScene(1);
    }

    public void LoadVolcanoLevelButton()
    {
        //StartCoroutine(GameManager.Instance.LoadScreenAndLoadScene(3));
        GameManager.Instance.LoadScreenAndLoadScene(3);
    }

    public void LoadCastleLevelButton()
    {
        //StartCoroutine(GameManager.Instance.LoadScreenAndLoadScene(5));
        GameManager.Instance.LoadScreenAndLoadScene(5);
    }

    public void LoadSpaceStationLevelButton()
    {
        //StartCoroutine(GameManager.Instance.LoadScreenAndLoadScene(4));
        GameManager.Instance.LoadScreenAndLoadScene(4);
    }

    public void LoadAztecLevelButton()
    {
        //StartCoroutine(GameManager.Instance.LoadScreenAndLoadScene(6));
        GameManager.Instance.LoadScreenAndLoadScene(6);
    }
    public void LoadDungeonLevelButton()
    {
        //StartCoroutine(GameManager.Instance.LoadScreenAndLoadScene(6));
        GameManager.Instance.LoadScreenAndLoadScene(7);
    }

    public void QuitGame()
    {
        SteamManager.Instance.OnExitGame();
        Application.Quit();
    }

    //private IEnumerator LoadSceneByIndex(int pSceneIndex)
    //{ 
    //    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(pSceneIndex);

    //    while (!asyncLoad.isDone)
    //        yield return null;
    //}

    //private IEnumerator LoadSceneInTime (int pSceneIndex, float pTime)
    //{
    //    DisableAllMenus();
    //    LoadingScreen.SetActive(true);
    //    WaterAnim?.GetComponent<Animator>()?.SetTrigger("TriggerLoadingScene");

    //    yield return new WaitForSeconds(pTime);
    //    StartCoroutine(LoadSceneByIndex(pSceneIndex));
    //}

    public void PlayChangedOptionEffect()
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.ButtonClick);
    }

    public void PlayClickOptionEffect()
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.ButtonClick);
    }

    public void StartTournamentButton()
    {
        GameManager.Instance.Tournament = true;
    }
}