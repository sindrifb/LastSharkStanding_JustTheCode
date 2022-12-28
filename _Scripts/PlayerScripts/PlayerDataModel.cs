using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
[System.Serializable]
public class PlayerDataModel {

    private Color[] Colors = new Color[4];
    public GameObject Skin;
    public Sprite PlayerSprite;
    public Color PlayerColor = Color.red;
    public GameObject PlayerPrefab;
    public int RewiredID; //{get; private set;}
    public int PlayerID; //{get; private set;}
    public int RoundWins;
    //public string PlayerName;

    public PlayerDataModel(GameObject pPlayerPrefab, int pPLayerID, int pRewiredID)
    {
        Colors[0] = new Color(87f/255f, 178f / 255f, 102f / 255f, 1f);
        Colors[1] = new Color(255f / 255f, 102f / 255f, 88f / 255f, 1f);
        Colors[2] = new Color(68f / 255f, 186f / 255f, 255f / 255f, 1f);
        Colors[3] = new Color(178f / 255f, 107f / 255f, 240f / 255f, 1f);
        PlayerPrefab = pPlayerPrefab;
        PlayerID = pPLayerID;
        RewiredID = pRewiredID;
        PlayerColor = Colors[pPLayerID - 1];
    }
}
