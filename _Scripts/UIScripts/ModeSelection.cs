using Rewired;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ModeSelection : MonoBehaviour
{
    [SerializeField]
    private List<string> Modes;
    [SerializeField]
    private TextMeshProUGUI CurrentlySelectedTxt;
    [SerializeField]
    private TextMeshProUGUI NextTxt;
    [SerializeField]
    private TextMeshProUGUI PreviousTxt;

    private EventSystem EventSystem;
    private float PressTimer = 0;
    public int CurrentlySelectedIndex { get; private set; } = 0;
    [SerializeField]
    private Image RightArrow;
    [SerializeField]
    private Image LeftArrow;
    public Color ArrowStandard;
    public Color ArrowHighlight;

    private int NextIndex
    {
        get
        {
            int nextIndex = CurrentlySelectedIndex + 1;
            if (nextIndex >= Modes.Count)
            {
                nextIndex = 0;
            }

            return nextIndex;
        }
    }

    private int PreviousIndex
    {
        get
        {
            int prevIndex = CurrentlySelectedIndex - 1;
            if (prevIndex < 0)
            {
                prevIndex = Modes.Count - 1;
            }

            return prevIndex;
        }
    }

    private void Start()
    {
        EventSystem = FindObjectOfType<EventSystem>();
    }

    private void Update()
    {
        PressTimer += Time.deltaTime;
        if (EventSystem.currentSelectedGameObject == gameObject)
        {
            foreach (var player in GameManager.Instance.PlayerDataModels)
            {
                var axis = ReInput.players.GetPlayer(player.RewiredID).GetAxis("MenuHorizontal");

                if (Mathf.Abs(axis) >= 0.5f && PressTimer > 0.3f)
                {
                    ChangeMap(axis);
                    AudioManager.Instance.PlayOneShot(AudioManager.Instance.UISound.ChangeOutfit);
                    PressTimer = 0;
                }
            }
        }
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

    private void UpdateText(int pCurrentlySelectedIndex)
    {
        //CurrentlySelectedImg.sprite = MapSprites[pCurrentlySelectedIndex];
        //NextImg.sprite = MapSprites[NextIndex];
        //PreviousImg.sprite = MapSprites[PreviousIndex];
        CurrentlySelectedTxt.text = Modes[pCurrentlySelectedIndex];
        NextTxt.text = Modes[NextIndex];
        PreviousTxt.text = Modes[PreviousIndex];
    }

    private void ChangeMap(float pAxis)
    {
        if (pAxis > 0)
        {
            CurrentlySelectedIndex = NextIndex;
        }
        else if (pAxis < 0)
        {
            CurrentlySelectedIndex = PreviousIndex;
        }

        StartCoroutine(HighlightArrow(pAxis));
        UpdateText(CurrentlySelectedIndex);
    }
}
