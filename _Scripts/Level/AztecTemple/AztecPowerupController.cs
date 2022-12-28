using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AztecPowerupController : MonoBehaviour
{
    public Transform[] DartParents;
    private List<List<Tuple<Rigidbody, Vector3>>> ListOfListOfDarts = new List<List<Tuple<Rigidbody, Vector3>>>();
    public Transform Trap;
    public Transform Pedestal;
    public GameObject[] PowerUpPrefabs;
    private GameObject SpawnedPowerup;
    public GameObject TrapActivateParticleEffect;
    List<PlayerController> PlayersInTrap = new List<PlayerController>();
    private Vector3 ActiveTrapPosition;
    public Vector3 DeactiveTrapPosition;

    private Vector3 ActivePowerupPosition;
    public Vector3 DeactivePowerupPosition;
    bool reset;
    public enum PowerupState
    {
        active, activating, activatingTrap, deactivating, deactivateTrap, deactive, none
    }

    private PowerupState CurrentPowerUpState = PowerupState.deactive;

    void Start()
    {
        foreach (var parent in DartParents)
        {
            List<Tuple<Rigidbody, Vector3>> Darts = new List<Tuple<Rigidbody, Vector3>>();
            foreach (Transform dart in parent)
            {
                var rb = dart.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Darts.Add(new Tuple<Rigidbody, Vector3>(rb,rb.transform.localPosition));
                }
            }
            ListOfListOfDarts.Add(Darts);
        }
        ActivePowerupPosition = Pedestal.localPosition;
        ActiveTrapPosition = Trap.localPosition;

        Trap.localPosition = DeactiveTrapPosition;
        Pedestal.localPosition = DeactivePowerupPosition;
        //StartCoroutine(RNDInit());
        EventManager.StartListening(EventManager.EventCodes.GameEnd,Reset);
        EventManager.StartListening(EventManager.EventCodes.RoundEnd, Reset);
    }

    public void Reset()
    {
        if (SpawnedPowerup != null)
        {
            Destroy(SpawnedPowerup);
        }
    }


    IEnumerator RNDInit()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(6, 40));
        Activate();
        StartCoroutine(RNDInit());
    }
    float timer = 0;
    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    Activate();
        //}

        switch (CurrentPowerUpState)
        {
            case PowerupState.active:
                if (SpawnedPowerup == null)
                {
                    SetState(PowerupState.deactivating);
                }
                break;
            case PowerupState.activating:
                //AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.StoneScrape);
                Pedestal.localPosition = Vector3.MoveTowards(Pedestal.localPosition, ActivePowerupPosition, Time.deltaTime * 5);
                if (Pedestal.localPosition == ActivePowerupPosition)
                {
                    SetState(PowerupState.active);
                }
                break;
            case PowerupState.deactivating:
                Pedestal.localPosition = Vector3.MoveTowards(Pedestal.localPosition, DeactivePowerupPosition, Time.deltaTime * 10);
                //AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.StoneScrape);

                if (Pedestal.localPosition == DeactivePowerupPosition)
                {
                    if (reset)
                    {
                        SetState(PowerupState.deactive);
                    }
                    else
                    {
                        SetState(PowerupState.activatingTrap);
                    }
                }
                break;
            case PowerupState.deactive:
                break;
            case PowerupState.activatingTrap:
                Trap.localPosition = Vector3.MoveTowards(Trap.localPosition, ActiveTrapPosition, Time.deltaTime * 10);
                //AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.StoneScrape);
                if (Trap.localPosition == ActiveTrapPosition)
                {
                    LateSetState(PowerupState.deactivateTrap,1f);
                }
                break;
            case PowerupState.deactivateTrap:
                timer += Time.deltaTime;
                if (timer <1f)
                {
                    return;
                }
               
                //AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.StoneScrape);
                Trap.localPosition = Vector3.MoveTowards(Trap.localPosition, DeactiveTrapPosition, Time.deltaTime * 5);
                if (Trap.localPosition == DeactiveTrapPosition)
                {
                    timer = 0;
                    SetState(PowerupState.deactive);
                }
                break;
            default:
                break;
        }
    }

    void LateSetState(PowerupState pPowerupState, float pDelay)
    {
        StartCoroutine(LateSetStateCoroutine(pPowerupState,pDelay));
    }

    IEnumerator LateSetStateCoroutine(PowerupState pPowerupState, float pDelay)
    {
        CurrentPowerUpState = PowerupState.none;
        yield return new WaitForSeconds(pDelay);
        SetState(pPowerupState);
    }

    public GameObject Activate()
    {
        if (CurrentPowerUpState == PowerupState.deactive)
        {
            SetState(PowerupState.activating);

            var pu = Instantiate(PowerUpPrefabs[UnityEngine.Random.Range(0,PowerUpPrefabs.Length)], Pedestal.transform.position + Vector3.up, Quaternion.identity);
            SpawnedPowerup = pu;
            pu.transform.parent = Pedestal;
            var purb = pu.GetComponent<Rigidbody>();
            if (purb !=null)
            {
                purb.isKinematic = true;
            }
            return pu;
        }
        return null;
    }

    void SetState(PowerupState powerUpState)
    {
        switch (powerUpState)
        {
            case PowerupState.active:
                CurrentPowerUpState = PowerupState.active;
                break;
            case PowerupState.activating:
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.EnvironmentSound.Temple_PillarUp);
                
                CurrentPowerUpState = PowerupState.activating;
                break;
            case PowerupState.deactivating:
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.EnvironmentSound.Temple_PillarDown);
                CurrentPowerUpState = PowerupState.deactivating;
                break;
            case PowerupState.deactive:
                CurrentPowerUpState = PowerupState.deactive;
                break;
            case PowerupState.activatingTrap:
                CurrentPowerUpState = PowerupState.activatingTrap;
                break;
            case PowerupState.deactivateTrap:
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.TempleWallFire);
                //var ps1 = Instantiate(TrapActivateParticleEffect, transform.position + (transform.forward * 3), Quaternion.LookRotation(transform.forward));
                //var ps2 = Instantiate(TrapActivateParticleEffect, transform.position + (transform.right * 3), Quaternion.LookRotation(transform.right));
                //Destroy(ps2, 3);
                //Destroy(ps1, 3);
                //PlayersInTrap.RemoveAll(a => a == null);

                //foreach (var item in PlayersInTrap)
                //{
                //    item.Hookable.Push((transform.position).normalized + Vector3.up, 35f);
                //}
                FireTrap();
                //LateSetState(PowerupState.deactivateTrap,1f);
                CurrentPowerUpState = PowerupState.deactivateTrap;
                break;
            default:
                break;
        }
    }


    void FireTrap()
    {
        StartCoroutine(FireDarts());
    }

    IEnumerator FireDarts()
    {
        int count = 0;
        foreach (var list in ListOfListOfDarts)
        {
            foreach (var dart in list)
            {
                count++;
                dart.Item1.isKinematic = false;
                dart.Item1.AddForce(dart.Item1.transform.right * 50, ForceMode.Impulse);
                if (count >= 3)
                {
                    count = 0;
                    yield return new WaitForSeconds(.05f);
                }
            }
        }
        float scale = 1;
        while (scale > 0.1)
        {
            //reset
            foreach (var list in ListOfListOfDarts)
            {
                foreach (var dart in list)
                {
                    //item2 = startpos
                    scale = Mathf.MoveTowards(scale,0,Time.deltaTime / 2);
                    dart.Item1.transform.localScale = Vector3.one * scale;
                }
            }
            yield return null;
        }
        //reset
        foreach (var list in ListOfListOfDarts)
        {
            foreach (var dart in list)
            {
                //item2 = startpos
                dart.Item1.transform.localPosition = dart.Item2;
                dart.Item1.transform.localScale = Vector3.one;
                dart.Item1.isKinematic = true;
                dart.Item1.transform.localRotation = Quaternion.AngleAxis(-90,Vector3.right);
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        var PC = col.GetComponent<PlayerController>();
        if (PC != null)
        {
            PlayersInTrap.Add(PC);
        }
    }

    void OnTriggerExit(Collider col)
    {
        var PC = col.GetComponent<PlayerController>();
        if (PC != null)
        {
            PlayersInTrap.Remove(PC);
        }
    }
}
