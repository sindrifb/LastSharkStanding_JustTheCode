using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BarrageController : MonoBehaviour
{

    public Transform PlatformLeft;
    public Transform PlatformRight;
    public GameObject WarningShotPrefab;
    public float BarrageCoolDown;
    public GameObject NavObstacleLeft;
    public GameObject NavObstacleRight;
    private Vector3 WarningStartPos;
    private GameObject WarningShotGO;
    private Coroutine BarrageCoroutine;
    private Coroutine CooldownCoroutine;
    private bool BarrageOnGoing = false;
    private BarrageHazard BarrageHazard;
    private FMOD.Studio.EventInstance BarrageSoundEvent;

    private void Start()
    {
        NavObstacleLeft.SetActive(false);
        NavObstacleRight.SetActive(false);
        BarrageHazard = GetComponent<BarrageHazard>();
        EventManager.StartListening(EventManager.EventCodes.RoundEnd, ResetBarrage);
        EventManager.StartListening(EventManager.EventCodes.GameEnd, ResetBarrage);
        EventManager.StartListening(EventManager.EventCodes.RoundStart, StartBarrage);
    }

    private IEnumerator Spawner()
    {
        while (true)
        {
            yield return new WaitForSeconds(20f);
            
            BarrageCoroutine = StartCoroutine(Barrage(BarrageHazard.FindSideToSpawnBarrage(PlatformLeft, PlatformRight)));
            yield return new WaitUntil(() => !BarrageOnGoing);
        }
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.F))
    //    {
    //        //StartCoroutine(WarningShot(PlatformRight.transform));
    //        BarrageHazard.StartBarrage(BarrageHazard.TargetSide.right);
    //    }
    //}

    private void ResetBarrage()
    {
        BarrageHazard.StopBarrage();
        NavObstacleLeft.SetActive(false);
        NavObstacleRight.SetActive(false);
        AudioManager.Instance.StopEvent(BarrageSoundEvent, FMOD.Studio.STOP_MODE.IMMEDIATE);
        if (BarrageCoroutine != null)
        {
            StopCoroutine(BarrageCoroutine);
        }
        if (CooldownCoroutine != null)
        {
            StopCoroutine(CooldownCoroutine);
        }
        Destroy(WarningShotGO);
    }

    private void StartBarrage()
    {
        if (CooldownCoroutine != null)
        {
            StopCoroutine(CooldownCoroutine);
        }
        CooldownCoroutine = StartCoroutine(Spawner());
        
    }

    private IEnumerator Barrage(BarrageHazard.TargetSide pSide)
    {
        BarrageOnGoing = true;
        if (pSide == BarrageHazard.TargetSide.left)
        {
            WarningStartPos = PlatformLeft.position;
            NavObstacleLeft.SetActive(true);
        }
        else
        {
            WarningStartPos = PlatformRight.position;
            NavObstacleRight.SetActive(true);
        }
        WarningShotGO = Instantiate(WarningShotPrefab, WarningStartPos, Quaternion.identity);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.BarrageWarningShot);
        yield return new WaitForSeconds(3f);
        BarrageHazard.StartBarrage(pSide);
        yield return new WaitForSeconds(3f);
        //AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.BarrageImpact);
        AudioManager.Instance.PlayEvent(AudioManager.Instance.HazardSound.BarrageImpact, out BarrageSoundEvent);
        yield return new WaitForSeconds(4);
        NavObstacleLeft.SetActive(false);
        NavObstacleRight.SetActive(false);
        BarrageOnGoing = false;
        Destroy(WarningShotGO);
    }
}
