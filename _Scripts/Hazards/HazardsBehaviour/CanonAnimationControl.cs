using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[RequireComponent(typeof(Animator))]
public class CanonAnimationControl : MonoBehaviour {
    private Animator Animator;

    private void Start()
    {
        Animator = GetComponent<Animator>();
    }
	void LateUpdate ()
    {
        bool parentInuse = transform.root.GetComponent<CanonHazard>().Canons.FirstOrDefault(a => a.Canon.transform == transform.parent).InUse;

        Animator.SetBool("InUse",parentInuse);
	}
}
