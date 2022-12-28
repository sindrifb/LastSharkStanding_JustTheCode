using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CatapultController : MonoBehaviour
{
    public GameObject TargetHit;
    public GameObject ThrownPlayerParticle;
    public List<PlayerController> PlayersOnCatapult = new List<PlayerController>();
    private SphereCollider Collider;
    private Animator Animator;
    private bool IsActive;
    private Coroutine CatapultCoroutine;
    private CatapultThrower CatapultThrower;

    private void Start()
    {
        EventManager.StartListening(EventManager.EventCodes.GameEnd, ResetCatapult);
        EventManager.StartListening(EventManager.EventCodes.PlayersCleared, ResetCatapult);
        CatapultThrower = GetComponent<CatapultThrower>();
        Collider = GetComponentInChildren<SphereCollider>();
        Animator = GetComponent<Animator>();
        IsActive = false;
        StartCoroutine(MakeSureNoNullsInList());
    }

    private void ActivateCatapult()
    {
        IsActive = true;
        CatapultCoroutine = StartCoroutine(Catapult());
    }

    private void ResetCatapult()
    {
        if (CatapultCoroutine != null)
        {
            StopCoroutine(CatapultCoroutine);
        }
        CatapultThrower.ClearLists();
        IsActive = false;
        Collider.enabled = true;
        Animator.Play(Constants.AnimationParameters.CatapultIdle);
    }

    private IEnumerator Catapult()
    {
        Animator.SetTrigger(Constants.AnimationParameters.IsActive);
        yield return new WaitForSeconds(0.2f);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.GraterOpen);
        //var targetPush = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z - 35f) * Vector3.up;
        
        var targetPush = transform.right + Vector3.up;
        foreach (var player in PlayersOnCatapult)
        {
            if (player != null)
            {
                var rndDir = Random.onUnitSphere;
                rndDir.y = 0;
                rndDir.Normalize();
                var projectileData = new ProjectileData(player.transform.position, TargetHit.transform.position + rndDir, 45, 1f);
                if (ThrownPlayerParticle != null)
                {
                    var particle = Instantiate(ThrownPlayerParticle, player.transform);
                    Destroy(particle, 4f);
                }
                //Debug.Log(player.name + " Thrown");
                var rng = Random.Range(10, 10);
                player.Hookable.Push(targetPush.normalized, 0);
                player.Rigidbody.drag = 0;
                player.Rigidbody.angularDrag = 0;
                player.Rigidbody.velocity = projectileData.Velocity;
                StartCoroutine(TempIgnoreCollision(player.Rigidbody));
                CatapultThrower.AddThrownPlayer(player.gameObject, false);
            }
        }
        CatapultThrower.CheckWhenPlayerLands();
        PlayersOnCatapult.Clear();
        Collider.enabled = false;
        yield return new WaitForSeconds(3.5f);
        ResetCatapult();
    }

    private IEnumerator TempIgnoreCollision(Rigidbody pRB)
    {
        pRB.detectCollisions = false;
        yield return new WaitForSeconds(.6f);
        pRB.detectCollisions = true;
    }

    private IEnumerator LandingPush()
    {
        yield return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            PlayersOnCatapult.Add(player);
            if (!IsActive)
            {
                ActivateCatapult();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            PlayersOnCatapult.Remove(player);
        }
    }

    private IEnumerator MakeSureNoNullsInList()
    {
        while (true)
        {
            PlayersOnCatapult = PlayersOnCatapult.Where(a => a != null).ToList();
            yield return new WaitForSeconds(3);
        }
    }

    //private void OnDrawGizmos()
    //{
    //    var targetPush = transform.right + Vector3.up;
    //    Gizmos.DrawRay(transform.position, targetPush * 20f);
    //}
}
