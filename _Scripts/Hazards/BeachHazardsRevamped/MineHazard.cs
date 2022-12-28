using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class MineHazard : MonoBehaviour
{
    public int CurrentlyActive { get
        {
            if (Mines.Any())
            {
                return Mines.Where(a => !a.CanBeReUsed).Count();
            }
            else
            {
                return 0;
            }
           
        }
    }
    private List<PlayerController> Players { get { return Camera.main.GetComponent<CameraFollow>().Players.Where(a => a != null).ToList(); } }
    public GameObject MinePrefab;
    public float Radius = 2f;
    public float Force = 50f;
    public float WarningTime = 1f;
    public float StartTime = 1f;
    private List<Mine> Mines = new List<Mine>();

    public void SweepMines()
    {
        Mines.Where(a => !a.CanBeReUsed).ToList().ForEach(a => a.GameObject.GetComponent<MineHit>().Explode(true));
    }

    public void FireMine()
    {
        var unusedMine = Mines.FirstOrDefault(a => a.CanBeReUsed);

        if (unusedMine != null)
        {
            ActivateMine(unusedMine);
        }
        else
        {
            Spawn();
        }
    }

    private void ActivateMine(Mine pMine)
    {
        pMine.CanBeReUsed = false;
        pMine.GameObject.transform.position = FindSpawnPoint();
        pMine.GameObject.SetActive(true);
        StartCoroutine(pMine.GameObject.GetComponent<MineHit>().Initialize());
    }

    public void DisableMine(GameObject pMine)
    {
        var mine = Mines.FirstOrDefault(a => a.GameObject == pMine);
        mine.CanBeReUsed = true;
        mine.GameObject.SetActive(false);
        mine.GameObject.transform.rotation = Quaternion.identity;
        var rb = mine.GameObject.GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void Spawn()
    {
        var goBase = Instantiate(MinePrefab, transform.position, Quaternion.identity);
        var mineHit = goBase.GetComponent<MineHit>();
        var hookable = goBase.GetComponent<Hookable>();
        hookable.IsAvailable = false;
        mineHit.MineHazard = this;
        mineHit.Radius = Radius;
        mineHit.StartTime = StartTime;
        mineHit.WarningTime = WarningTime;
        mineHit.Force = Force;
        var mine = new Mine(goBase);
        ActivateMine(mine);
        Mines.Add(mine);
    }

    private Vector3 FindSpawnPoint()
    {
        var v = UnityEngine.Random.onUnitSphere;
        v.y = 0;
        v.Normalize();
        var target = v * UnityEngine.Random.Range(0, 13);

        target = TestSpawnPoint(target);

        NavMeshHit navHit;
        NavMesh.SamplePosition(target, out navHit, 15, NavMesh.AllAreas);

        Vector3 rayStartPos = navHit.position + (Vector3.up * 10);
        Ray ray = new Ray(rayStartPos, Vector3.down);
        RaycastHit rayHit;
        Physics.Raycast(ray, out rayHit, Mathf.Infinity, 1 << 10, QueryTriggerInteraction.Ignore);

        return new Vector3(rayHit.point.x, 0, rayHit.point.z);
    }

    //no overlapping mines
    private Vector3 TestSpawnPoint(Vector3 pPos)
    {
        int count = 0;
        bool acceptable = false;
        Vector3 testPos = pPos;
        while (!acceptable && count < 5f)
        {
            count++;
            var activeMines = Mines.Where(a => !a.CanBeReUsed);
            var minesTooClose = activeMines.Where(a => (testPos - a.GameObject.transform.position).sqrMagnitude <= 4f * 4f);
            if (minesTooClose.Any())
            {
                var avoid = minesTooClose.OrderBy(a => (testPos - a.GameObject.transform.position).sqrMagnitude).FirstOrDefault();
                testPos += (avoid.GameObject.transform.position - pPos) * 4.1f;
                if (testPos.magnitude >= 13f)
                {
                    testPos = Quaternion.AngleAxis(45, Vector3.up) * (testPos.normalized * 5f);
                }
            }
            else
            {
                acceptable = true;
            }
        }
        return testPos;
    }
}

public class Mine
{
    public bool CanBeReUsed;
    public GameObject GameObject;

    public Mine(GameObject gameObject)
    {
        GameObject = gameObject;
    }
}
