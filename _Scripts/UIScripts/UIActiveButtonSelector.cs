using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIActiveButtonSelector : MonoBehaviour {

    private EventSystem EventSys;
    public Button FirstSelectedButton;

    private void Awake()
    {
        EventSys = FindObjectOfType<EventSystem>();
    }

    public void SetFirstSelectedButton()
    {
        FirstSelectedButton.Select();
        EventSys.firstSelectedGameObject = FirstSelectedButton.gameObject;
    }
}
