using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CanonHazard : MonoBehaviour {

    public bool randomb;

    //GameManager GameManager;
    public float Speed;
    public List<CanonModel> Canons;
    private Vector3 NavObstacleStartSize;
    private Vector3 NavObstavleStartCenter;
    private Vector3 NavObstacleMoveSize = new Vector3(4, 1, 25);
    private Vector3 NavObstacleCenterOffset = new Vector3(0, 0, 11);
   

    private void Start()
    {
        NavObstacleStartSize = Canons[0].NavObstacle.size;
        NavObstavleStartCenter = Canons[0].NavObstacle.center;

        StartCoroutine(FireRecursive());
    }

    private IEnumerator FireRecursive()
    {
        while (true)
        {
            float delay = Random.Range(3, 10);
            yield return new WaitForSeconds(delay);
            if (GameManager.Instance.Difficulty >= 1)
            {
                for (int i = 0; i < Canons.Count; i++)
                {
                    if (Canons[i].FuseParticleEffect != null)
                    {
                        Canons[i].FuseParticleEffect.SetActive(true);
                        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.CannonFuse);

                    }
                }
                StartCoroutine(IncreaseObstacleSize());
                yield return new WaitForSeconds(.5f);
                FireCanons();
            }
        }
        
        //StartCoroutine(FireRecursive());
    }

    private IEnumerator CoolShooting()
    {
        for (int i = 0; i < Canons.Count; i++)
        {
            yield return new WaitForSeconds(.2f);
            OnMoveStart(Canons[i]);
        }
    }

    private void FireCanons()
    {
        if (randomb)
        {
            StartCoroutine(CoolShooting());
        }
        else
        {
            for (int i = 0; i < Canons.Count; i++)
            {
                if (Random.Range(0, 3) <= 1 && !Canons[i].InUse)
                {
                    OnMoveStart(Canons[i]);
                }
            }
        }
       
    }

    private IEnumerator MoveCanon(CanonModel pCanon, Vector3 pCurrentTarget)
    {
        pCanon.FuseParticleEffect.SetActive(false);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.CannonFire);

        while (pCanon.Canon.transform.position != pCurrentTarget)
        {
            pCanon.Canon.transform.position = Vector3.MoveTowards(pCanon.Canon.transform.position,pCurrentTarget,Speed * Time.deltaTime);
            yield return null;
        }
        OnMoveDone(pCanon);
    }

    private void OnMoveStart(CanonModel pCanonModel)
    {
        var canon = pCanonModel;
        canon.InUse = true;
        canon.TriggerCollider.enabled = true;
        canon.PhysicalCollider.enabled = false;
        Vector3 curTarget = (canon.Target.position - canon.Canon.transform.position).sqrMagnitude < (canon.Start.position - canon.Canon.transform.position).sqrMagnitude ? canon.Start.position : canon.Target.position;
        //pCanonModel.Canon.transform.rotation = Quaternion.LookRotation(curTarget - pCanonModel.Canon.transform.position);
        canon.NavObstacle.size = NavObstacleMoveSize;
        canon.NavObstacle.center += NavObstacleCenterOffset;
        StartCoroutine(MoveCanon(canon,curTarget));
    }

    private void OnMoveDone(CanonModel pCanonModel)
    {
        pCanonModel.InUse = false;
        pCanonModel.NavObstacle.size = NavObstacleStartSize;
        pCanonModel.NavObstacle.center = NavObstavleStartCenter;
        pCanonModel.TriggerCollider.enabled = false;
        pCanonModel.PhysicalCollider.enabled = true;
        Vector3 curTarget = (pCanonModel.Target.position - pCanonModel.Canon.transform.position).sqrMagnitude < (pCanonModel.Start.position - pCanonModel.Canon.transform.position).sqrMagnitude ? pCanonModel.Start.position : pCanonModel.Target.position;
        pCanonModel.Canon.transform.rotation = Quaternion.LookRotation(curTarget - pCanonModel.Canon.transform.position);
    }

    private IEnumerator IncreaseObstacleSize()
    {
        float lerpAmount = 0;
        while (lerpAmount < 1)
        {
            yield return null;
            lerpAmount += Time.deltaTime / 0.7f;
            foreach (var canon in Canons)
            {
                canon.NavObstacle.size = Vector3.Lerp(NavObstacleStartSize, NavObstacleMoveSize, lerpAmount);
                canon.NavObstacle.center = Vector3.Lerp(NavObstavleStartCenter, NavObstacleCenterOffset, lerpAmount);
            }
        }
    }
}

[System.Serializable]
public class CanonModel
{
    public GameObject Canon;
    public Transform Target;
    public Transform Start;
    public SphereCollider TriggerCollider;
    public Collider PhysicalCollider;
    public GameObject StartParticleEffect;
    public GameObject StopParticleEffect;
    public GameObject FuseParticleEffect;
    public NavMeshObstacle NavObstacle;

    public bool InUse
    {
        set { SetAnimators(value); mInUse = value; }
        get { return mInUse; }
    }

    private bool mInUse = false;
    [Header("InUse bool parameter set")]
    public List<Animator> Animators;

    private void SetAnimators(bool pValue)
    {
        for (int i = 0; i < Animators.Count; i++)
        {
            Animators[i]?.SetBool("InUse", pValue);
        }
        if (StartParticleEffect)
        {
            StartParticleEffect?.SetActive(pValue);
        }
        if (StopParticleEffect)
        {
            StopParticleEffect?.SetActive(!pValue);
        }
    }

    
}
