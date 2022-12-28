using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class BarrageHazard : MonoBehaviour
{
    public Transform LeftPlatformCorner;
    public Transform RightPlatformCorner;
    public float Width;
    public float Length;
    public float Offset;
    public List<Vector3> BarrageLocationsLeft = new List<Vector3>();
    public List<Vector3> BarrageLocationsRight = new List<Vector3>();

    public int CurrentlyActive { get { return Meteors.Where(a => a.IsActive).Count(); } }
    public Vector3 MapCenter = Vector3.zero;
    public GameObject MeteorBase;
    public GameObject Skin;
    public GameObject ImpactParticle;
    public GameObject WarningParticle;
    public float ImpactRadius = 4f;
    public float ImpactStrenght = 20f;

    public float WarningTime = .5f;
    public float TimeToLand = 2f;
    public float MaxDistanceFromCenter = 5;

    private List<BarrageMeteor> Meteors = new List<BarrageMeteor>();
    private Coroutine BarrageCoroutine;

    //for getting the center of the group
    private CameraFollow CameraFollow;

    public enum TargetType { random, camper, group, any }
    public enum TargetSide { left, right }

    void Start()
    {
        GetBarrageLocations();
        CameraFollow = Camera.main.GetComponent<CameraFollow>();
    }

    public TargetSide FindSideToSpawnBarrage(Transform pPlatformLeft, Transform pPlatformRight)
    {
        List<PlayerController> players = FindObjectsOfType<PlayerController>().ToList();
        float leftMagnitude = 0;
        float rightMagnitude = 0;

        foreach (var player in players)
        {
            leftMagnitude += (player.transform.position - pPlatformLeft.position).sqrMagnitude;
            rightMagnitude += (player.transform.position - pPlatformRight.position).sqrMagnitude;
        }
        //Debug.Log("LeftMagnitude = " + leftMagnitude);
        //Debug.Log("RightMagnitude = " + rightMagnitude);
        if (leftMagnitude < rightMagnitude)
        {
            return TargetSide.left;
        }
        else
        {
            return TargetSide.right;
        }
    }

    private void GetBarrageLocations()
    {
        BarrageLocationsLeft.Clear();
        BarrageLocationsRight.Clear();
        for (int j = 0; j < Length; j++)
        {
            var posL = new Vector3(RightPlatformCorner.position.x, RightPlatformCorner.position.y, RightPlatformCorner.position.z - (j * 3) - Offset);
            

            for (int i = 0; i < Width; i++)
            {
                var posW = new Vector3(posL.x + (i * Random.Range(2.5f, 3.5f) + (Offset / 2)), posL.y, posL.z + Random.Range(0.1f,1.1f));
                BarrageLocationsRight.Add(posW);
            }
        }

        IListExtensions.Shuffle(BarrageLocationsRight);

        for (int k = 0; k < Length; k++)
        {
            var posL = new Vector3(LeftPlatformCorner.position.x, LeftPlatformCorner.position.y, LeftPlatformCorner.position.z - (k * 3) - Offset);


            for (int l = 0; l < Width; l++)
            {
                var posW = new Vector3(posL.x + (l * Random.Range(2.5f, 3.5f)+(Offset / 2)), posL.y, posL.z + Random.Range(0.1f, 1.1f));
                BarrageLocationsLeft.Add(posW);
            }
        }

        IListExtensions.Shuffle(BarrageLocationsLeft);

    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawSphere(RightPlatformCorner.position, 1f);
    //    for (int j = 0; j < Length; j++)
    //    {
    //        var posL = new Vector3(RightPlatformCorner.position.x, RightPlatformCorner.position.y, RightPlatformCorner.position.z - (j * 3f) - Offset);
    //        Gizmos.DrawSphere(posL, 1f);

    //        for (int i = 0; i < Width; i++)
    //        {
    //            var posW = new Vector3(posL.x + (i * 3f) + (Offset / 2), posL.y, posL.z);
    //            Gizmos.DrawSphere(posW, 1f);
    //        }
    //    }
    //    Gizmos.DrawSphere(LeftPlatformCorner.position, 1f);
    //}

    public void StartBarrage(TargetSide pSide)
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.BeachBallFall);
        BarrageCoroutine = StartCoroutine(FireBarrage(pSide));
    }

    public void StopBarrage()
    {
        if (BarrageCoroutine != null)
        {
            StopCoroutine(BarrageCoroutine);
        }
    }

    public IEnumerator FireBarrage(TargetSide pSide)
    {
        switch (pSide)
        {
            case TargetSide.left:
                foreach (var pos in BarrageLocationsLeft)
                {
                    FireMeteor(pos);
                    yield return new WaitForSeconds(0.0001f);
                }
                break;
            case TargetSide.right:
                foreach (var pos in BarrageLocationsRight)
                {
                    FireMeteor(pos);
                    yield return new WaitForSeconds(0.0001f);
                }
                break;
            default:
                break;
        }
        GetBarrageLocations();
    }

    public void RemoveActiveMeteors()
    {
        Meteors.ForEach(a => DisableMeteor(a));
    }

    public void FireMeteor(Vector3 pTarget)
    {
        var unusedMeteor = Meteors.FirstOrDefault(a => !a.IsActive);

        if (unusedMeteor != null)
        {
            ActivateMeteor(unusedMeteor, pTarget);
        }
        else
        {
            Spawn(pTarget);
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
                meteor.GameObject.transform.position = Vector3.Lerp(meteor.StartPos, meteor.Target, progress);
            }
        }
    }

    void Land(BarrageMeteor pMeteor)
    {
        //AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.BarrageImpact);
        var cleanedList = CameraFollow.Players.Where(a => a != null);
        var playersInRange = cleanedList.Where(a => (a.transform.position - pMeteor.GameObject.transform.position).sqrMagnitude < ImpactRadius * ImpactRadius);
        //playersInRange.ToList().ForEach(a => a.Hookable.Push(((Vector3.up) + a.transform.position.normalized).normalized, ImpactStrenght));
        foreach (var item in playersInRange)
        {
            item.Hookable.Push(((Vector3.up) + item.transform.position.normalized).normalized, ImpactStrenght);
            HazardEvent hazInfo = new HazardEvent
            {
                Description = "Barrage Hazard Event",
                HazardType = AchHazardType.Barrage,
                PlayerHit = item.gameObject,
                RewiredID = item.RewiredID,
                Skin = item.Skin,
            };
            hazInfo.FireEvent();
        }
        
        var ps = Instantiate(ImpactParticle, pMeteor.GameObject.transform.position, Quaternion.identity);
        Destroy(ps, 4);
        DisableMeteor(pMeteor);
    }

    void Spawn(Vector3 pTarget)
    {
        var goBase = Instantiate(MeteorBase, transform.position, Quaternion.identity);
        var pSkin = Instantiate(Skin, goBase.transform);
        pSkin.transform.localPosition = Vector3.zero;
        goBase.GetComponentInChildren<MeshRenderer>().enabled = false;

        var meteor = new BarrageMeteor(goBase, Vector3.zero, Vector3.zero, 0, true);
        ActivateMeteor(meteor, pTarget);
        Meteors.Add(meteor);
    }

    private void ActivateMeteor(BarrageMeteor pMeteor, Vector3 pTarget)
    {
        //AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.BeachBallFall);
        pMeteor.GameObject.SetActive(true);
        pMeteor.GameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
        //pMeteor.Target = FindNewTarget(pType);
        pMeteor.Target = pTarget;
        var ps = Instantiate(WarningParticle, pMeteor.Target, Quaternion.identity);
        Destroy(ps, WarningTime + TimeToLand);
        pMeteor.StartPos = pMeteor.Target + Vector3.up * 20;
        pMeteor.GameObject.transform.position = pMeteor.Target + Vector3.up * 20;
        pMeteor.IsActive = true;
        StartCoroutine(StartMovingMeteor(pMeteor));
    }

    private IEnumerator StartMovingMeteor(BarrageMeteor pMeteor)
    {
        yield return new WaitForSeconds(WarningTime);
        if (pMeteor.GameObject.activeInHierarchy)
        {
            pMeteor.GameObject.GetComponentInChildren<MeshRenderer>().enabled = true;
            pMeteor.IsMoving = true;
        }
    }

    private void DisableMeteor(BarrageMeteor pMeteor)
    {
        pMeteor.GameObject.SetActive(false);
        pMeteor.Target = Vector3.zero;
        pMeteor.TimeProgress = 0f;
        pMeteor.IsActive = false;
        pMeteor.IsMoving = false;
    }

    //private Vector3 FindNewTarget(TargetType pType = TargetType.any)
    //{
    //    int action = pType == TargetType.any ? Random.Range(1, 4) : ((int)pType + 1);

    //    Vector3 target = Vector3.zero;
    //    switch (action)
    //    {
    //        case 1:
    //            //random choice
    //            var v = Random.onUnitSphere;
    //            v.y = 0;
    //            v.Normalize();
    //            target = MapCenter + v * Random.Range(0, MaxDistanceFromCenter);
    //            break;
    //        case 2:
    //            //target campers/strays that are far away from the group
    //            Vector3 groupCenter = CameraFollow.GetGroupCenter();
    //            var players = CameraFollow.Players.Where(a => a != null);
    //            target = players.OrderByDescending(a => (a.transform.position - groupCenter).sqrMagnitude).FirstOrDefault()?.transform.position ?? Vector3.zero;
    //            break;
    //        case 3:
    //            //target the center of the group
    //            target = CameraFollow.GetGroupCenter();
    //            break;
    //        default:
    //            break;
    //    }

    //    target += Random.insideUnitSphere;

    //    NavMeshHit hit;
    //    NavMesh.SamplePosition(target, out hit, 50, NavMesh.AllAreas);

    //    return hit.position;
    //}
}

public class BarrageMeteor
{
    public GameObject GameObject;
    public Vector3 StartPos;
    public Vector3 Target;
    public float TimeProgress;
    public bool IsActive;
    public bool IsMoving;

    public BarrageMeteor(GameObject gameObject, Vector3 startPos, Vector3 target, float timeProgress, bool active)
    {
        GameObject = gameObject;
        StartPos = startPos;
        Target = target;
        TimeProgress = timeProgress;
        IsActive = active;
    }
}

public static class IListExtensions
{
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}
