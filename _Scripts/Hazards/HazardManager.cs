using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(RandomPointOnMesh))]
public class HazardManager : MonoBehaviour
{
    public static HazardManager Instance;

    [Tooltip("Shows info and allows keyboard control if true")]
    public bool DEBUGGING = true; // For the debug.Log in all hazard script (hazardManager, hazards and spawners)

    [Tooltip("Models containing all Hazard's important data")]
    public HazardData[] HazardModels;

    /// <summary>
    ///  position in array means the type of hazard (equals the one in "HazardModels")
    ///  first searchs in the models for the type of hazard that want to be spawned
    ///  getting the position in the models, equals the position in pool for that type
    ///  within the pool, find the one inactive or instantiates a new one
    /// </summary>
    protected List<GameObject>[] HazardPool;

    protected int[] AmountOfHazardOfEach;

    // generate rnd pos on mesh
    RandomPointOnMesh RndPointGenerator;

    private void Awake()
    {
        Instance = this;

        HazardPool = new List<GameObject>[HazardModels.Length];
        RndPointGenerator = GetComponent<RandomPointOnMesh>();

        for (int i = 0; i < HazardModels.Length; i++)
        {
            HazardModels[i].HazardType = HazardModels[i].HazardGO.GetComponent<BasicHazard>().HazardType;
            HazardPool[i] = new List<GameObject>();
        }

        AmountOfHazardOfEach = new int[HazardModels.Length];
    }    

    // INSTANTIATING HAZARDS 
    // (returns instantiated hazard GO)
    // Completly random
    public GameObject InstantiateHazardAtRndPos(HazardType pHType)
    {
        Vector3 tRndPos = RndPointGenerator.GetRandomPoint();
        Vector3 tRndEnd = RndPointGenerator.GetRandomPoint();

        return InstantiateHazard(pHType, tRndPos, tRndEnd);
    }

    // Random end, fixed origin pos
    public GameObject InstantiateHazardAtRndPos(HazardType pHType, Vector3 pInstantiateOrigPos)
    {
        Vector3 tRndEnd = RndPointGenerator.GetRandomPoint();

        return InstantiateHazard(pHType, pInstantiateOrigPos, tRndEnd);
    }

    // End and Origin pos is fixed and the same
    public GameObject InstantiateHazard(HazardType pHType, Vector3 pInstantiatePos)
    {
        return InstantiateHazard(pHType, pInstantiatePos, pInstantiatePos);
    }

    // End and Origin pos is fixed and can be different 
    // ACTUALLY THE ONLY ONE THAT DOES SOMETHING
    public GameObject InstantiateHazard(HazardType pHType, Vector3 pInstantiateOrigPos, Vector3 pInstantiateEndPos)
    {

        int auxPosInList;
        GameObject aux = GetDeadHazard(pHType, out auxPosInList);

        if (HazardModels[auxPosInList].MaxHazardNumOnScreen < 0 || AmountOfHazardOfEach[auxPosInList] < HazardModels[auxPosInList].MaxHazardNumOnScreen)
        {
            //print("SPAWNED: " + pHType);
            AmountOfHazardOfEach[auxPosInList]++;

            // CHECK: CALL ORDER
            aux.GetComponent<BasicHazard>().SetOriginAndEndPos(pInstantiateOrigPos, pInstantiateEndPos);
            aux.GetComponent<BasicHazard>().SetHazardData(HazardModels[auxPosInList]);
            aux.GetComponent<BasicHazard>().TriggerHazBehaviour();
        }
        return aux;
    }

    // AUX METHODS
    // Gets the first disabled hazard or creates one if needed
    private GameObject GetDeadHazard(HazardType pHType, out int pPosInList)
    {
        pPosInList = FindPositioninList(pHType);

        if (pPosInList == -1)
        {
            //Debug.LogError("Hazard " + pHType + " not found in models");
            return null;
        }
        else
        {
            GameObject auxGO = HazardPool[pPosInList].Find(x => (!x.activeInHierarchy && !x.GetComponent<BasicHazard>().IsWaitingToSpawn)); // Finds inactive

            if (auxGO == null)
            {
                auxGO = Instantiate(HazardModels[pPosInList].HazardGO);
                auxGO.SetActive(false);
                HazardPool[pPosInList].Add(auxGO);
            }
            return auxGO;
        }
    }

    // Finds the type of hazard it is
    private int FindPositioninList(HazardType pHType)
    {
        int i = 0;
        while (i < HazardModels.Length &&
            HazardModels[i].HazardType != pHType)
            i++;

        return (i < HazardModels.Length ? i : -1); // -1 if it doesn't find the type
    }

    public Vector3 GetRndPointOnMesh()
    {
        // CHECK meter aqui lo de la posicion con rango. tal vez circle cast con un radio, total, solo es para las minas.
        // ver si hay obj con basicHazard component en un rango. generar punto hasta conseguirlo
        // tratar de meter separacion de los bordes en el original ?
        return RndPointGenerator.GetRandomPoint();
    }

    // returns true if the position is far enough to spawn the next hazard or not
    public bool CheckIfPositionIsFar(HazardType pHazardType, Vector3 pPosToCheck)
    {
        int posInList = FindPositioninList(pHazardType);
        bool isFarEnough = true;

        if (HazardModels[posInList].RadioMinDistanceBetweenHazards > 0)
        {
            isFarEnough = !HazardPool[posInList].Exists(x => x.activeInHierarchy && Vector3.Distance(x.transform.position, pPosToCheck) < HazardModels[posInList].RadioMinDistanceBetweenHazards);

            return isFarEnough;
        }
        return true;
    }

    // KILL HAZARD
    // Disables the hazard 
    public void KillHazard(GameObject pHazard, float pTime = 0f)
    {
        var aux = FindPositioninList(pHazard.GetComponent<BasicHazard>().HazardType);
        AmountOfHazardOfEach[aux]--;

        StartCoroutine(KillHazardCoroutine(pHazard, pTime));
    }

    public IEnumerator KillHazardCoroutine(GameObject pHazard, float pTime = 0f)
    {
        if (pTime < 0)
        {
            yield return new WaitForSeconds(Random.Range(.1f, .6f));
            var ps = Instantiate(GameManager.Instance.DestroyParticleEffect, pHazard.transform.position, Quaternion.identity);
            Destroy(ps, 2);
            pTime = 0;
        }
        
        pHazard.GetComponentsInChildren<MeshRenderer>().All(x=> x.enabled = false);
        var coll = pHazard.GetComponents<Collider>();
        foreach(Collider c in coll)
        {
            c.enabled = false;
        }
        
        if (pTime >= 0)
            yield return new WaitForSeconds(pTime);

        pHazard.SetActive(false);
        pHazard.GetComponentsInChildren<MeshRenderer>().All(x => x.enabled = true);
        foreach (Collider c in coll)
        {
            c.enabled = true;
        }
    }


    public void KillAllHazards()
    {
        for (int i = 0; i < HazardPool.Length; i++)
            foreach (GameObject go in HazardPool[i])
            {
                KillHazard(go, -1);
            }
    }

    // GETTERS AND SETTERS
    // CHECK REMOVE
    // Returns the hazardData of a certain type of HazardType
    public HazardData GetHazardData(HazardType pHType)
    {
        return HazardModels[FindPositioninList(pHType)];
    }
}
