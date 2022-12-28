using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class GameModeController : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(late());
    }

    private void Update()
    {
        foreach (var player in ReInput.players.Players)
        {
            if (player.GetButtonDown("PauseGame"))
            {
                GameManager.Instance.SetGamePaused(true);
            }
        }
    }

    IEnumerator late()
    {
        yield return new WaitForSeconds(.5f);

        UIManager.Instance.Init();
        GameManager.Instance.StartGame();

    }
}
