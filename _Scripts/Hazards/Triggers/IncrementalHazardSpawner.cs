using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class IncrementalHazardSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnerProb
    {
        public SpawnerBase HazardSpawner;
        [Tooltip("Dont touch it, just for clarify")]
        public string HazardType;
        [Tooltip("Probability (in percentage) of appearance")]
        [Range(0, 100)]
        public float AppearingProbability;
    }

    [Tooltip("This would be respected even while the time passes")]
    public float MinTimeBetweenHazards = 0;
    [Tooltip("Initial time between hazards. This would be reduced.")]
    public float MaxTimeIntervalBetweenHazards = 5;
    [Tooltip("Reducing time factor when the difficulty increases")]
    [Range(0, 1)]
    public float ReducingFactor = 0.75f;

    private int LastDifficutyLevel = 1;
    private float timeToWait; // The actual thing

    public Transform SpawnerGroup;

    public SpawnerProb[] Spawners;

    public void OnValidate()
    {
        if (Spawners.Length == 0 && SpawnerGroup != null)
        {
            SpawnerBase[] aux = SpawnerGroup.GetComponentsInChildren<SpawnerBase>().ToArray();
            Spawners = new SpawnerProb[aux.Length];

            float auxProb = 100 / aux.Length;

            for (int i = 0; i < aux.Length; i++)
            {
                Spawners[i].HazardSpawner = aux[i];
                Spawners[i].HazardType = aux[i].Hazard.ToString();
                Spawners[i].AppearingProbability = auxProb;
            }
        }
        CheckProbability();
    }

    private void CheckProbability()
    {
        float aux = 0;
        foreach (SpawnerProb sp in Spawners)
        {
            aux += sp.AppearingProbability;
        }

        if (aux > 100)
            Debug.LogError("Probabilities not set correctly, they go over 100%");
    }

    private void Start()
    {
        timeToWait = MaxTimeIntervalBetweenHazards;
        StartCoroutine(SpawnNextHazard());
    }

    private IEnumerator SpawnNextHazard()
    {
        yield return new WaitUntil(() => (GameManager.Instance.Difficulty > 0));

        int aux = PosInArrayWithProb();
        if (aux >= 0 && aux < Spawners.Length)
            Spawners[aux].HazardSpawner.SpawnObject();

        yield return new WaitForSeconds(timeToWait);

        TriggerNextHazard();

        if (LastDifficutyLevel != GameManager.Instance.Difficulty)
        {
            LastDifficutyLevel = GameManager.Instance.Difficulty;

            if (LastDifficutyLevel > 0)
            {
                timeToWait = (timeToWait * ReducingFactor);
                timeToWait = Mathf.Clamp(timeToWait, MinTimeBetweenHazards, MaxTimeIntervalBetweenHazards);
            }
            else
            {
                StopAllCoroutines();
                HazardManager.Instance?.KillAllHazards();
                timeToWait = MaxTimeIntervalBetweenHazards;
                TriggerNextHazard();
            }
        }
    }
    private void TriggerNextHazard()
    {
        StartCoroutine(SpawnNextHazard());
    }

    private int PosInArrayWithProb()
    {
        float pTargetProb = Random.Range(0f, 100f);

        int i = 0;
        float pAcummulated = 0f;
        while (i < Spawners.Length && pAcummulated < pTargetProb)
        {
            pAcummulated += Spawners[i].AppearingProbability;
            i++;
        }
        return (i - 1); // returns i-1 because it adds +1 to i before checking if this is the hazard choosen
    }
}

// GAME MANAGER DIFFICULTY (0 = nothing happenning, 1 = easy, 10 = difficult asf)
