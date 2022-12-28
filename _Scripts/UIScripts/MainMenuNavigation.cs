using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Rewired;
using System.Collections.Generic;

public class MainMenuNavigation : MonoBehaviour
{

    public MenuScreens BackMenu = 0;
    public GameObject FirstSelected;
    private bool OneMovePerTime = false;
    private EventSystem UIEventSystem;
    public static bool CanGoBack = true;
    public bool DisableButtonAfterPress = true;
    private List<Button> Buttons = new List<Button>();

    public bool OnCooldown = false;

    // Use this for initialization
    private void OnEnable()
    {
        OnCooldown = true;
        StartCoroutine(CoolDown());
        if (FirstSelected != null)
        {
            StartCoroutine(OnMenuEnabled());
        }

        foreach (var button in Buttons)
        {

            button.interactable = true;
        }
    }

    private IEnumerator OnMenuEnabled()
    {
        UIEventSystem.SetSelectedGameObject(null);
        yield return new WaitForSeconds(0.8f);
        UIEventSystem.SetSelectedGameObject(FirstSelected);
    }

    private void Awake()
    {

        UIEventSystem = FindObjectOfType<EventSystem>();
    }

    private void Start()
    {
        if (DisableButtonAfterPress)
        {
            Buttons = gameObject.GetComponentsInChildren<Button>().ToList();

            foreach (var button in Buttons)
            {

                button.onClick.AddListener(() => MakeNonInteractable(button));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < ReInput.players.playerCount; i++)
        {
            if (CanGoBack && ReInput.players.GetPlayer(i).GetButtonDown("MenuBack") && !OnCooldown && !MenusManager.Instance.IsTransitioning)
            {
                OnCooldown = true;
                MenusManager.Instance.GoToMenu(BackMenu);
            }
        }
    }

    private IEnumerator CoolDown()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        OnCooldown = false;
    }

    public void MakeNonInteractable(Button pButton)
    {
        pButton.interactable = false;
    }

    //private void OnDisable()
    //{
    //    OnCooldown = false;
    //}
}
