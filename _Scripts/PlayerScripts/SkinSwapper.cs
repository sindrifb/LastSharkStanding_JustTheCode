using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

public class SkinSwapper : MonoBehaviour {

    public bool NakedShark;
    private Material PlayerColorMaterial;
    public Transform RightHandParentConstraint;
    private ParentConstraint[] AnimParentConstraints;
    private List<Material> Mats;
    public Material[] Materials;
    // Index needs to point to the material "TCP_Red" in skinnedMeshRenderer on prefab.
    public int MaterialIndex;
    private Color[] Colors = new Color[4];

    // Use this for initialization
    void Start () {
        Colors[0] = new Color(8f / 255f, 76f / 255f, 17f / 255f, 1f);
        Colors[1] = new Color(76f / 255f, 15f / 255f, 8f / 255f, 1f);
        Colors[2] = new Color(10f / 255f, 69f / 255f, 102f / 255f, 1f);
        Colors[3] = new Color(59f / 255f, 10f / 255f, 102f / 255f, 1f);

        Materials = GetComponent<SkinnedMeshRenderer>().materials;
        var id = GameManager.Instance.GetPlayerDataModels()?.FirstOrDefault(a => a.PlayerID == transform.root.GetComponent<PlayerController>().PlayerID);

        // base model has no colors to change
        if (!NakedShark)
        {
            Materials[MaterialIndex].color = id?.PlayerColor ?? Color.yellow;
        }
        else
        {
            Materials[MaterialIndex].color = Colors[id.PlayerID - 1];
        }


        AnimParentConstraints = transform.root.GetComponentsInChildren<ParentConstraint>(true);
        for (int i = 0; i < AnimParentConstraints.Length; i++)
        {
            ConstraintSource newSource = new ConstraintSource
            {
                sourceTransform = RightHandParentConstraint,
                weight = 1
            };
            
            AnimParentConstraints[i].AddSource(newSource);
        }
    }
}
