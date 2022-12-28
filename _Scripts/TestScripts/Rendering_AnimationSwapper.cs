using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rendering_AnimationSwapper : MonoBehaviour
{
    [SerializeField]
    private int poseINT;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetInteger("PoseNumber", poseINT);
    }
}
