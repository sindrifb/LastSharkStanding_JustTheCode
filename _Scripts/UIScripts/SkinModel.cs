using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkinModel {

    public GameObject Skin;

    public Sprite GreenSprite;
    public Sprite RedSprite;
    public Sprite PurpleSprite;
    public Sprite BlueSprite;
    public string SkinName;

    public Sprite GetSprite(int pPlayerID)
    {
        switch (pPlayerID)
        {
            case 1:
                return GreenSprite;
            case 2:
                return RedSprite;
            case 3:
                return BlueSprite;
            case 4:
                return PurpleSprite;
            default:
                return GreenSprite;
        }
    }
}
