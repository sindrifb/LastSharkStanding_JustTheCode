using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Collider))]
public abstract class Interactable : MonoBehaviour
{
    protected virtual void OnGeneralActivate(){}
    protected virtual void OnHookActivate() { }
    protected virtual void OnPlayerActivate() { }
    protected virtual void OnRigidbodyActivate() { }

    private void OnTriggerEnter(Collider other)
    {
        var rb = other.GetComponent<Rigidbody>();
        var pc = other.GetComponent<PlayerController>();
        var hook = other.GetComponent<Hook>();
        if (rb || pc || hook)
        {
            OnGeneralActivate();
        }

        if (rb)
        {
            OnRigidbodyActivate();
        }

        if (pc)
        {
            OnPlayerActivate();
        }

        if (hook)
        {
            OnHookActivate();
        }
    }
}
