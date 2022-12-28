using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class MapAndModeManager : MonoBehaviour
{
    [SerializeField]
    private GameObject ModeSelectionObj;
    [SerializeField]
    private MapSelection MapSelection;
    private ModeSelection ModeSelection;
    private bool ButtonPushed;
    [SerializeField]
    private GameObject TournamentManagerPrefab;

    private void Awake()
    {
        ModeSelection = ModeSelectionObj.GetComponent<ModeSelection>();
    }

    private void OnEnable()
    {
        ModeSelectionObj.SetActive(GameManager.Instance.Tournament);
        ButtonPushed = false;
    }

    private void Update()
    {
        foreach (var player in GameManager.Instance.PlayerDataModels)
        {
            if (ReInput.players.GetPlayer(player.RewiredID).GetButtonDown("MenuSelect") && !ButtonPushed)
            {
                ButtonPushed = true;
                StartCoroutine(StartPlaying());
            }
        }
    }

    private IEnumerator StartPlaying()
    {
        if (GameManager.Instance.Tournament)
        {
            var tournamentManager = Instantiate(TournamentManagerPrefab);

            var scoreNeeded = 0;
            var modeSelected = ModeSelection.CurrentlySelectedIndex;
            if (modeSelected == 0)
            {
                scoreNeeded = 3;
            }
            else if (modeSelected == 1)
            {
                scoreNeeded = 6;
            }

            tournamentManager.GetComponent<TournamentManager>().Initialize(scoreNeeded);

            yield return new WaitForEndOfFrame();
        }

        if (MapSelection.CurrentlySelectedIndex == 6)
        {
            GameManager.Instance.LoadScreenAndLoadScene(Random.Range(0,6) + 1);
        }
        else
        {
            GameManager.Instance.LoadScreenAndLoadScene(MapSelection.CurrentlySelectedIndex + 1);
        }
    }
}
