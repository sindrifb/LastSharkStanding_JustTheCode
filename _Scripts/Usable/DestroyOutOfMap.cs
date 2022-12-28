using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOutOfMap : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(CheckIfOut());
    }

    private IEnumerator CheckIfOut()
    {
        while (true)
        {
            if (transform.parent == null && transform.position.y < -5)
            {
                Destroy(gameObject);
            }
            yield return new WaitForSeconds(10f);
        }
    }
}
