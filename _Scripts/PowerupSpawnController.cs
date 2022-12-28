using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PowerupSpawnController : MonoBehaviour
{
    public GameObject SpawnParticleEffect;
    public LayerMask SpawnCheckLayerMask;
    public List<GameObject> PowerUps = new List<GameObject>();
    public float SpawnTime;
    public int ActivePowerUpSpawnLimit;
    public Transform PowerUpSpawnPointsParent;
    public List<Transform> SpawnLocations = new List<Transform>();
    public List<PowerUpSpawnPoint> PowerupSpawnPoints = new List<PowerUpSpawnPoint>();
    private List<GameObject> ActivePowerUps = new List<GameObject>();
    private GameObject SpawnedPowerup;

    private Vector3 SpawnPos;

    private void Start()
    {
        SpawnLocations = PowerUpSpawnPointsParent.transform.GetComponentsInChildren<Transform>().ToList().Where(a => a != PowerUpSpawnPointsParent.transform).ToList();

        foreach (var spawn in SpawnLocations)
        {
            PopulateSpawnList(spawn.transform, false);
        }

        if (ActivePowerUpSpawnLimit > SpawnLocations.Count-1)
        {
            ActivePowerUpSpawnLimit = SpawnLocations.Count - 1;
        }
        StartCoroutine(Spawner());
    }

    public void PopulateSpawnList(Transform pTransform, bool pValue)
    {
        PowerUpSpawnPoint spawnpoint = new PowerUpSpawnPoint(pTransform, pValue);

        PowerupSpawnPoints.Add(spawnpoint);
    }

    private List<PowerUpSpawnPoint> FindAvailableSpawnPoints()
    {
        for (int i = 0; i < PowerupSpawnPoints.Count; i++)
        {
            PowerupSpawnPoints[i].IsAvailable = !SpawnPosIsOccupied(PowerupSpawnPoints[i].SpawnLocation.position);
        }
        return PowerupSpawnPoints.Where(a => a.IsAvailable).ToList();
    }

    private IEnumerator Spawner()
    {
        while (true)
        {
            if (GameManager.TimeSinceRoundStart > 5)
            {
                if (ActivePowerUps.Count < ActivePowerUpSpawnLimit)
                {
                    var spawnTransform = SpawnFurthestFromPlayers();
                    
                    if (spawnTransform != null)
                    {
                        SpawnPos = spawnTransform.position;
                        SpawnPowerUp();
                    }
                }
            
                yield return new WaitForSeconds(SpawnTime);
                RefreshActivePowerUpList();
            }
            yield return null;
        }
    }

    private Transform SpawnFurthestFromPlayers()
    {
        List<PlayerController> players = FindObjectsOfType<PlayerController>().ToList();
        int index = 0;
        float highestMagnitude = 0;
        var availableSpawns = FindAvailableSpawnPoints();

        if (availableSpawns.Any())
        {
            for (int i = 0; i < availableSpawns.Count; i++)
            {
                float totalMagnitude = 0;

                foreach (var player in players)
                {
                    totalMagnitude += (player.transform.position - availableSpawns[i].SpawnLocation.position).sqrMagnitude;
                }

                if (totalMagnitude > highestMagnitude)
                {
                    highestMagnitude = totalMagnitude;
                    index = i;
                }
                //print("available spawn nr " + i + " is = '" + availableSpawns[i].SpawnLocation.transform + "\n" + availableSpawns[i].SpawnLocation + " totalmagnitude = " + totalMagnitude);
            }
            //Debug.Log("Out of: ' " + availableSpawns.Count + "' SpawnPoints, This: " + availableSpawns[index].SpawnLocation.gameObject + " Was Picked.");
            return availableSpawns[index].SpawnLocation;
        }
        return null;
    }

    private bool SpawnPosIsOccupied(Vector3 pSpawnPos)
    {
        //return Physics.Raycast(new Vector3(pSpawnPos.x, pSpawnPos.y + 10, pSpawnPos.z), Vector3.down, 100, SpawnCheckLayerMask);
        Ray ray = new Ray(new Vector3(pSpawnPos.x, pSpawnPos.y + 10, pSpawnPos.z), Vector3.down);
        RaycastHit[] hits = Physics.SphereCastAll(ray, 3, 30);
        List<GameObject> hitObjects = new List<GameObject>();
        hits.ToList().ForEach(a => hitObjects.Add(a.collider.gameObject));
        bool obstructed = hitObjects.FirstOrDefault(a => a.layer == 9 || a.layer == 12) != null;
        //Debug.DrawRay(ray.origin, ray.direction * 30, Color.red, 1);

        //return Physics.SphereCast(ray, 8, 30, SpawnCheckLayerMask);
        return obstructed;
    }

    private void RefreshActivePowerUpList()
    {
        ActivePowerUps = ActivePowerUps.Where(a => a != null).ToList();
    }

    private void SpawnPowerUp()
    {
        var rng = UnityEngine.Random.Range(0, PowerUps.Count);
        SpawnedPowerup = Instantiate(PowerUps[rng]);

        GameManager.Instance.SpawnedObjects.Add(SpawnedPowerup);
        SpawnedPowerup.transform.position = SpawnPos;
        ActivePowerUps.Add(SpawnedPowerup);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HookSound.PowerUpSpawn);

        if (SpawnParticleEffect)
        {
            var particle = Instantiate(SpawnParticleEffect, SpawnedPowerup.transform.position, Quaternion.identity);
            GameManager.Instance.SpawnedObjects.Add(particle);
        }
    }

    public class PowerUpSpawnPoint
    {
        public Transform SpawnLocation;
        public bool IsAvailable;

        public PowerUpSpawnPoint (Transform pSpawnLocation, bool pIsAvailable)
        {
            SpawnLocation = pSpawnLocation;
            IsAvailable = pIsAvailable;
        }
    }
}
