using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPrompts : MonoBehaviour
{

    //public GameObject Hold;
    //public GameObject HoldOrange;
    public GameObject Select;
    public GameObject Back;

    public GameObject MainTitle;
    public GameObject MainMenu;
    public GameObject Options;
    public GameObject MapSelection;
    public GameObject Lobby;
    public GameObject Credits;
    //public GameObject LoadingScreen;
    private Vector3 LeftPos = new Vector3(-755, -480, 0);
    private Vector3 RightPos = new Vector3(-465, -480, 0);


    void Update()
    {

        if (MainTitle.activeInHierarchy)
        {
            //Hold.SetActive(false);
            //HoldOrange.SetActive(false);
            Select.SetActive(false);
            Back.SetActive(false);
        }

        if (MainMenu.activeInHierarchy)
        {
            //Hold.SetActive(false);
            //HoldOrange.SetActive(false);
            Select.SetActive(true);
            Back.SetActive(true);
            Select.transform.localPosition = LeftPos;
            Back.transform.localPosition = RightPos;
        }

        if (Options.activeInHierarchy)
        {
            //Hold.SetActive(false);
            //HoldOrange.SetActive(false);
            Select.SetActive(true);
            Back.SetActive(true);
            Select.transform.localPosition = LeftPos;
            Back.transform.localPosition = RightPos;
        }

        if (MapSelection.activeInHierarchy)
        {
            //Hold.SetActive(true);
            //HoldOrange.SetActive(true);
            Select.SetActive(true);
            Back.SetActive(true);
            Select.transform.localPosition = LeftPos;
            Back.transform.localPosition = RightPos;
        }

        if (Lobby.activeInHierarchy)
        {
            //Hold.SetActive(true);
            //HoldOrange.SetActive(true);
            Select.SetActive(true);
            Back.SetActive(false);
            Select.transform.localPosition = LeftPos;
            Back.transform.localPosition = RightPos;
        }

        if (Credits.activeInHierarchy)
        {
            //Hold.SetActive(true);
            //HoldOrange.SetActive(true);
            Select.SetActive(false);
            Back.SetActive(true);
            Back.transform.localPosition = LeftPos;
        }

        //if (LoadingScreen.activeInHierarchy)
        //{
        //    //Hold.SetActive(false);
        //    //HoldOrange.SetActive(false);
        //    Select.SetActive(false);
        //    Back.SetActive(false);
        //}
    }
}
