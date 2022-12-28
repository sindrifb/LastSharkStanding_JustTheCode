using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Rewired;

public class UIManager : MonoBehaviour
{
    public GameObject LoadingScreenPrefab;
    public static UIManager Instance;
    public GameObject RoundStartScreenPrefab;
    public GameObject MatchEndScreenPrefab;
    public GameObject PauseMenuPrefab;
    public GameObject RoundStartScreen { get; private set; }
    private GameObject PauseMenu;   
    public GameObject MainCanvas;
    //private Coroutine LoadingCoroutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        MainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
    }

    public void Init()
    {
        MainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
    }

    public void SetRoundStartScreenAsActive(bool pValue)
    {
        MainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        if (RoundStartScreen != null)
        {
            RoundStartScreen.SetActive(pValue);
            if (pValue)
            {
                StartCoroutine(RoundStartScreen.GetComponent<RoundStartUI>().EndRound());
            }
        }
        else if (RoundStartScreen == null && pValue)
        {
            RoundStartScreen = Instantiate(RoundStartScreenPrefab, MainCanvas.transform);
            RoundStartScreen.GetComponent<RoundStartUI>().InitializeUI();
        }
    }

    public void SpawnMatchEndScreen()
    {
        //Destroy(RoundStartScreen);
        //Instantiate(MatchEndScreenPrefab, MainCanvas.transform);
        StartCoroutine(RoundStartScreen.GetComponent<RoundStartUI>().EndMatch());
    }

    public void SetPauseMenuAsActive(bool pValue)
    {
        MainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        if (PauseMenu != null)
        {
            PauseMenu.SetActive(pValue);
            if (pValue)
            {
                PauseMenu.GetComponent<PauseMenuManager>().Initialize();
                //play start animations
            }
            else
            {
                //play end animations
            }
        }
        else if (PauseMenu == null && pValue)
        {
            PauseMenu = Instantiate(PauseMenuPrefab, MainCanvas.transform);
            PauseMenu.GetComponent<PauseMenuManager>().Initialize();
        }
    }

    public void ReturnToMainMenu()
    {
        if (!GameManager.Instance.LoadingScene)
        {
            //GameManager.Instance.ClearArena();
            //GameManager.Instance.ClearPlayerDataModels();
            //StartCoroutine(GameManager.Instance.LoadScreenAndLoadScene(0));
            GameManager.Instance.LoadScreenAndLoadScene(0);
        }
    }

    public void ReturnToMapandMode()
    {
        if (!GameManager.Instance.LoadingScene)
        {
            //GameManager.Instance.ClearArena();
            //StartCoroutine(GameManager.Instance.LoadScreenAndLoadScene(0, 4));
            GameManager.Instance.LoadScreenAndLoadScene(0, 4);
        }  
    }

    //public IEnumerator LoadScreenAndLoadScene(int pSceneIndex, int pMenuScreenIndex = 0)
    //{
    //    if (!GameManager.Instance.LoadingScene)
    //    {
    //        GameManager.Instance.LoadingScene = true;
    //        var eventSystem = FindObjectOfType<InputManager>().GetComponent<EventSystem>();
    //        eventSystem.enabled = false;
    //        MainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
    //        var loadScreen = Instantiate(LoadingScreenPrefab, MainCanvas.transform);
    //        loadScreen.SetActive(true);
    //        yield return new WaitForSecondsRealtime(2f);
    //        LoadingCoroutine = StartCoroutine(GameManager.Instance.LoadScene(pSceneIndex, pMenuScreenIndex, eventSystem));
    //    }
    //}
}
