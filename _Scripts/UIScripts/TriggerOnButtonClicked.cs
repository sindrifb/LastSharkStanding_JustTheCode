using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;
using System.Linq;

public class TriggerOnButtonClicked : MonoBehaviour {

    public string keyTriggerUniqueName;
    public MenuScreens nextMenuScreen;
    private int menuNum;
    private bool IsTransitioning = false;
    private bool InputsEnabled = false;

    private void OnEnable()
    {
        InputsEnabled = false;
        StartCoroutine(LateEnableInput());
    }

    private void Start()
    {
        menuNum = (int)nextMenuScreen;
    }

    // Update is called once per frame
    void Update ()
    {
        if (GameManager.Instance.PlayerDataModels.Any() || !InputsEnabled)
        {
            return;
        }

        for (int i = 0; i < ReInput.players.playerCount; i++)
        {
            var player = ReInput.players.GetPlayer(i);
            if (keyTriggerUniqueName == "")
            {
                if (player.GetAnyButtonDown())
                {
                    NextScreen();
                }
            }
            else
            {
                if (player.GetButtonDown(keyTriggerUniqueName))
                {
                    NextScreen();
                }
            }
        }
    }

    private IEnumerator LateEnableInput()
    {
        yield return new WaitForSeconds(1f);

        InputsEnabled = true;
        IsTransitioning = false;
        //for (int i = 0; i < ReInput.players.playerCount; i++)
        //{
        //    var player = ReInput.players.GetPlayer(i);
        //    player.controllers.maps.SetMapsEnabled(true, "StartScreen");
        //    //player.AddInputEventDelegate(OnInput, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "AnyButtonDown");
        //}
    }

    private void OnInput(InputActionEventData data)
    {
        NextScreen();
    }

    private void NextScreen()
    {
        if (IsTransitioning)
        {
            return;
        }

        MenusManager.Instance.DisableAllMenus();
        MenusManager.Instance.GoToMenu(menuNum);

        MenusManager.Instance.PlayClickOptionEffect();

        //if (keyTriggerUniqueName == "")
        //{
        //    for (int i = 0; i < ReInput.players.playerCount; i++)
        //    {
        //        var player = ReInput.players.GetPlayer(i);
        //        player.RemoveInputEventDelegate(OnInput);
        //        //player.controllers.maps.SetMapsEnabled(false, "StartScreen");
        //    }
        //}
    }

    //private void OnDisable()
    //{
    //    if (keyTriggerUniqueName == "")
    //    {
    //        for (int i = 0; i < ReInput.players.playerCount; i++)
    //        {
    //            var player = ReInput.players.GetPlayer(i);
    //            player.RemoveInputEventDelegate(OnInput);
    //            //player.controllers.maps.SetMapsEnabled(false, "StartScreen");
    //        }
    //    }
    //}
}
