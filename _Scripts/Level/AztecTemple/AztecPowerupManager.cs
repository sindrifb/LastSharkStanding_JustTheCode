using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AztecPowerupManager : MonoBehaviour
{
    public List<AztecPowerupController> Controllers;

    int MaxPowerups => GameManager.Instance.Difficulty;
    List<GameObject> Powerups = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnPowerups());
    }

    IEnumerator SpawnPowerups()
    {
        yield return new WaitForSeconds(Random.Range(6,14));
        Powerups.RemoveAll(a => a == null);

        if (Powerups.Count < MaxPowerups)
        {
            var pu = Controllers[Random.Range(0, Controllers.Count)].Activate();
            Destroy(pu, Random.Range(5, 10));
            Powerups.Add(pu);
        }

        StartCoroutine(SpawnPowerups());
    }
}
