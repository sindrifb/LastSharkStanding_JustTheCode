using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSpawnerInTime : SpawnerBase {

    protected override Vector3 GetEndPos()
    {
        return HazardManager.Instance.GetRndPointOnMesh();
    }

    public void Start()
    {
        StartCoroutine(rndSpawning());
        
    }

    public IEnumerator rndSpawning()
    {
        float spawnRate = 15 / Mathf.Clamp(GameManager.Instance.Difficulty, 1, 10);
        yield return new WaitForSeconds(Random.Range(spawnRate, spawnRate + 2f));
        if (GameManager.Instance.Difficulty > 0)
        {
            int newHaz = Random.Range(0, 3);
            Hazard = (HazardType)newHaz;
            SpawnObject();
        }
  
        RestartCoroutine();
    }

    public void RestartCoroutine()
    {
        StartCoroutine(rndSpawning());
    }

}
