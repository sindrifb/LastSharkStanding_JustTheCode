using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModelController : MonoBehaviour
{
    public List<PlayerModel> PlayerModels = new List<PlayerModel>();


    
}

[System.Serializable]
public class PlayerModel
{
    public GameObject Model;
    public Sprite[] ColorSprites;
    public Dictionary<int, Sprite> ModelSprites = new Dictionary<int, Sprite>();

    public PlayerModel(GameObject pModel, Sprite[] pColorSprites, Dictionary<int, Sprite> pModelSprites)
    {
        Model = pModel;
        ColorSprites = pColorSprites;
        ModelSprites = pModelSprites;
    }
}

