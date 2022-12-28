using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharkRagdoll : MonoBehaviour
{

    public GameObject Ragdoll;
    private GameObject Skin;



    void Update()
    {
        if (GetComponent<Rigidbody>().isKinematic && Ragdoll.activeInHierarchy)
        {
            Ragdoll.SetActive(false);
            GetComponentInChildren<SkinSwapper>(true).gameObject.SetActive(true);
        }
        else if (!GetComponent<Rigidbody>().isKinematic && !Ragdoll.activeInHierarchy)
        {
            Ragdoll.SetActive(true);
            GetComponentInChildren<SkinSwapper>().gameObject.SetActive(false);
        }
    }
}
