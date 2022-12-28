using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class WaveHazard : MonoBehaviour
{
    public int CurrentlyActive { get { return Waves.Where(a => a.IsActive).Count(); } }
    public Vector3 MapCenter = Vector3.zero;
    public GameObject WaveBase;
    public GameObject Skin;
    public GameObject DespawnParticles;
    //public GameObject WarningParticle;
    public float PushStrength = 20f;

    public float DespawnTime = .5f;
    public float Speed = 30f;
    public float MaxDistanceFromCenter = 5;

    private List<Wave> Waves = new List<Wave>();

    //for getting the center of the group
    private CameraFollow CameraFollow;

    public enum TargetType { group, camper, random, any }

    void Start()
    {
        CameraFollow = Camera.main.GetComponent<CameraFollow>();
    }

    public void RemoveActiveWaves()
    {
        Waves.ForEach(a => EndWave(a));
    }

    public void FireWave(TargetType pType = TargetType.any)
    {
        
        var unusedWave = Waves.FirstOrDefault(a => a.CanBeReUsed);

        if (unusedWave != null)
        {
            ActivateWave(unusedWave, pType);
        }
        else
        {
            Spawn(pType);
        }
    }

    private void FixedUpdate()
    {
        foreach (var wave in Waves)
        {
            if (wave.IsActive)
            {
                wave.Rigidbody.MovePosition(wave.GameObject.transform.position + wave.MoveDir * Speed * Time.deltaTime * wave.GameObject.transform.localScale.y);
            }
        }
    }

    void Update()
    {
        foreach (var wave in Waves)
        {
            if (wave.IsActive)
            {
                //wave.TimeProgress += Time.deltaTime;
                //float progress = Mathf.Clamp01(wave.TimeProgress / TimeToTarget);
                //if (progress >= 1)
                //{
                //    EndWave(wave);
                //    continue;
                //}
                //wave.Rigidbody.MovePosition(wave.GameObject.transform.position + wave.MoveDir * Speed * Time.deltaTime * wave.GameObject.transform.localScale.y);
                if (!wave.IsDespawning && wave.GameObject.transform.position.x <= (MapCenter + Vector3.left * 15f).x)
                {
                    EndWave(wave);
                    //    continue;
                }
            }
            if (wave.IsDespawning)
            {
                wave.TimeProgress += Time.deltaTime;
                float progress = Mathf.Clamp01(wave.TimeProgress / DespawnTime);
                if (progress >= 1)
                {
                    DisableWave(wave);
                    continue;
                }
                wave.GameObject.transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(1, .01f, .01f) , progress);
                //wave.Rigidbody.MovePosition(wave.GameObject.transform.position + wave.GameObject.transform.forward * Time.deltaTime * wave.GameObject.transform.localScale.x * 40f);
                wave.WaveStrength = wave.GameObject.transform.localScale.y;
            }
        }
    }

    void EndWave(Wave pWave)
    {
        //var ps = Instantiate(ImpactParticle, pMeteor.GameObject.transform.position, Quaternion.identity);
        //Destroy(ps, 4);
        pWave.TimeProgress = 0f;
        pWave.IsDespawning = true;
    }

    void Spawn(TargetType pType = TargetType.any)
    {
        var goBase = Instantiate(WaveBase, Vector3.right * 1000, Quaternion.identity);
        if (Skin != null)
        {
            var pSkin = Instantiate(Skin, goBase.transform);
            pSkin.transform.localPosition = Vector3.zero;
        }

        var Wave = new Wave(goBase, Vector3.zero, Vector3.zero, 0, true);
        ActivateWave(Wave, pType);
        Waves.Add(Wave);
    }

    private void ActivateWave(Wave pWave, TargetType pType = TargetType.any)
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.Wave);
        pWave.GameObject.SetActive(true);
        pWave.Target = FindNewTarget(pType);
        //var ps = Instantiate(WarningParticle, pWave.Target, Quaternion.identity);
        //Destroy(ps, 2f);
        Vector3 offset = (pWave.Target - MapCenter);
        offset.x = Mathf.Abs(offset.x);
        offset.y = 0f;
        offset = (offset.normalized + Vector3.right).normalized;

        pWave.StartPos = pWave.Target + offset * 70f;
        pWave.GameObject.transform.position = pWave.StartPos;
        pWave.GameObject.transform.localScale = Vector3.one;
        pWave.IsActive = true;
        pWave.WaveStrength = PushStrength;
        pWave.MoveDir = (pWave.Target - pWave.StartPos).normalized;
        pWave.GameObject.transform.LookAt(pWave.Target);
        pWave.SetEmission(true);
        pWave.MeshGameObject.SetActive(true);
        pWave.CanBeReUsed = false;
    }

    private void DisableWave(Wave pWave)
    {
        pWave.Target = Vector3.zero;
        pWave.TimeProgress = 0f;
        pWave.MeshGameObject.SetActive(false);
        pWave.SetEmission(false);
        pWave.IsActive = false;
        pWave.IsDespawning = false;
        StartCoroutine(LateDisableWave(pWave));
    }

    private IEnumerator LateDisableWave(Wave pWave)
    {
        yield return new WaitForSeconds(2f);
        pWave.GameObject.SetActive(false);
        pWave.CanBeReUsed = true;
       
    }

    private Vector3 FindNewTarget(TargetType pType = TargetType.any)
    {
        int action = pType == TargetType.any ? Random.Range(1, 4) : ((int)pType + 1);

        Vector3 target = Vector3.zero;
        switch (action)
        {
            case 1:
                //random choice
                var v = Random.onUnitSphere;
                v.y = 0;
                v.Normalize();
                target = MapCenter + v * Random.Range(0, MaxDistanceFromCenter);
                break;
            case 2:
                //target campers/strays that are far away from the group
                Vector3 groupCenter = CameraFollow.GetGroupCenter();
                target = CameraFollow.Players.OrderByDescending(a => (a.transform.position - groupCenter).sqrMagnitude).FirstOrDefault()?.transform.position ?? Vector3.zero;
                break;
            case 3:
                //target the center of the group
                target = CameraFollow.GetGroupCenter();
                break;
            default:
                break;
        }
        target += Vector3.left * 3f;
        NavMeshHit hit;
        NavMesh.SamplePosition(target, out hit, 10, NavMesh.AllAreas);


        return hit.position;
    }
}

public class Wave
{
    public WaveHit WaveHit;
    public GameObject GameObject;
    public GameObject MeshGameObject;
    public Rigidbody Rigidbody;
    public Vector3 StartPos;
    public Vector3 Target;
    public Vector3 MoveDir;
    public float TimeProgress;
    public float WaveStrength { get { return mWaveStrength; } set { mWaveStrength = value; WaveHit.HitStrength = value; } }
    private float mWaveStrength;
    public bool IsActive;
    public bool IsDespawning;
    public bool CanBeReUsed;

    public Wave(GameObject gameObject, Vector3 startPos, Vector3 target, float timeProgress, bool active)
    {
        GameObject = gameObject;
        MeshGameObject = GameObject.GetComponentInChildren<MeshRenderer>().gameObject;
        Rigidbody = GameObject.GetComponent<Rigidbody>();
        WaveHit = GameObject.GetComponent<WaveHit>();
        StartPos = startPos;
        Target = target;
        TimeProgress = timeProgress;
        IsActive = active;

    }

    public void SetEmission(bool value)
    {
        var particleSystems = GameObject.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particleSystems)
        {
            var emission = ps.emission;
            emission.enabled = value;
        }
    }
}