using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardDeath : MonoBehaviour
{
    public int DeathHeigth = -5;
    private BasicHazard BasicHazard;

    private void Awake()
    {
        BasicHazard = GetComponent<BasicHazard>();   
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.position.y < DeathHeigth)
        {
            BasicHazard?.KillHazard();
        }
    }
}
