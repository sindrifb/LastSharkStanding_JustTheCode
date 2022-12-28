using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
public class MeteorHazard : MonoBehaviour
{

    public int CurrentlyActive { get { return Meteors.Where(a => a.IsActive).Count(); } }
    public Vector3 MapCenter = Vector3.zero;
    public GameObject MeteorBase;
    public GameObject Skin;
    public GameObject ImpactParticle;
    public GameObject WarningParticle;
    public float ImpactRadius = 1f;
    public float ImpactStrenght = 20f;

    public float WarningTime = .5f;
    public float TimeToLand = 2f;
    public float MaxDistanceFromCenter = 5;

    private List<Meteor> Meteors = new List<Meteor>();

    //for getting the center of the group
    private CameraFollow CameraFollow;

    public enum TargetType {random , camper, group, any}

    void Start()
    {
        CameraFollow = Camera.main.GetComponent<CameraFollow>();
    }

    public void RemoveActiveMeteors()
    {
        Meteors.ForEach(a => DisableMeteor(a));
    }

    public void FireMeteor(TargetType pType = TargetType.any)
    {
        var unusedMeteor = Meteors.FirstOrDefault(a => !a.IsActive);

        if (unusedMeteor != null)
        {
            ActivateMeteor(unusedMeteor, pType);
        }
        else
        {
            Spawn(pType);
        }
    }

    void Update()
    {
        foreach (var meteor in Meteors)
        {
            if (meteor.IsActive && meteor.IsMoving)
            {
                meteor.TimeProgress += Time.deltaTime;
                float progress = Mathf.Clamp01(meteor.TimeProgress / TimeToLand);
                if (progress >= 1)
                {
                    Land(meteor);
                    continue;
                }
                meteor.GameObject.transform.position = Vector3.Lerp(meteor.StartPos,meteor.Target,progress);
            }
        }
    }

    void Land(Meteor pMeteor)
    {
        //BeachHazardManager.Instance.PlaySound(LandAudioClip);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.BeachBallLand);
        var cleanedList = CameraFollow.Players.Where(a => a != null);
        var playersInRange = cleanedList.Where(a => (a.transform.position - pMeteor.GameObject.transform.position).sqrMagnitude < ImpactRadius * ImpactRadius);
        //playersInRange.ToList().ForEach(a => a.Hookable.Push(((Vector3.up * 2) + (new Vector3(a.transform.position.x, 0, a.transform.position.z) - new Vector3(pMeteor.GameObject.transform.position.x,0, pMeteor.GameObject.transform.position.z)).normalized).normalized, ImpactStrenght));
        foreach (var item in playersInRange)
        {
            item.Hookable.Push(((Vector3.up * 2) + (new Vector3(item.transform.position.x, 0, item.transform.position.z) - new Vector3(pMeteor.GameObject.transform.position.x, 0, pMeteor.GameObject.transform.position.z)).normalized).normalized, ImpactStrenght);
            HazardEvent hazInfo = new HazardEvent
            {
                Description = "Beach Ball Hazard Event",
                HazardType = AchHazardType.BeachBall,
                PlayerHit = item.gameObject,
                RewiredID = item.RewiredID,
                Skin = item.Skin,
            };
            hazInfo.FireEvent();
        }
        var ps = Instantiate(ImpactParticle, pMeteor.GameObject.transform.position, Quaternion.identity);
        Destroy(ps,4);
        DisableMeteor(pMeteor);
    }

    void Spawn(TargetType pType = TargetType.any)
    {
        var goBase = Instantiate(MeteorBase,transform.position,Quaternion.identity);
        var pSkin = Instantiate(Skin, goBase.transform);
        pSkin.transform.localPosition = Vector3.zero;

        var meteor = new Meteor(goBase,Vector3.zero, Vector3.zero, 0, true);
        ActivateMeteor(meteor, pType);
        Meteors.Add(meteor);
    }

    private void ActivateMeteor(Meteor pMeteor, TargetType pType = TargetType.any)
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.BeachBallFall);
        pMeteor.GameObject.SetActive(true);
        pMeteor.Target = FindNewTarget(pType);
        var ps = Instantiate(WarningParticle, pMeteor.Target, Quaternion.identity);
        Destroy(ps, WarningTime + TimeToLand);
        pMeteor.StartPos = pMeteor.Target + Vector3.up * 20;
        pMeteor.GameObject.transform.position = pMeteor.Target + Vector3.up * 20;
        pMeteor.GameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
        pMeteor.IsActive = true;
        StartCoroutine(StartMovingMeteor(pMeteor));
    }

    private IEnumerator StartMovingMeteor(Meteor pMeteor)
    {
        yield return new WaitForSeconds(WarningTime);
        if (pMeteor.GameObject.activeInHierarchy)
        {
            pMeteor.GameObject.GetComponentInChildren<MeshRenderer>().enabled = true;
            pMeteor.IsMoving = true;
        }
    }

    private void DisableMeteor(Meteor pMeteor)
    {
        pMeteor.GameObject.SetActive(false);
        pMeteor.Target = Vector3.zero;
        pMeteor.TimeProgress = 0f;
        pMeteor.IsActive = false;
        pMeteor.IsMoving = false;
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
                var players = CameraFollow.Players.Where(a => a != null);
                target = players.OrderByDescending(a => (a.transform.position - groupCenter).sqrMagnitude).FirstOrDefault()?.transform.position ?? Vector3.zero;
                break;
            case 3:
                //target the center of the group
                target = CameraFollow.GetGroupCenter();
                break;
            default:
                break;
        }

        target += Random.insideUnitSphere;

        NavMeshHit hit;
        NavMesh.SamplePosition(target, out hit,50, NavMesh.AllAreas);

        return hit.position;
    }
}

public class Meteor
{
    public GameObject GameObject;
    public Vector3 StartPos;
    public Vector3 Target;
    public float TimeProgress;
    public bool IsActive;
    public bool IsMoving;

    public Meteor(GameObject gameObject, Vector3 startPos, Vector3 target, float timeProgress, bool active)
    {
        GameObject = gameObject;
        StartPos = startPos;
        Target = target;
        TimeProgress = timeProgress;
        IsActive = active;
    }
}