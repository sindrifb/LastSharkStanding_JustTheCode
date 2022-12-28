using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CatapultThrower : MonoBehaviour
{
    public List<PlayersThrown> ThrownPlayers = new List<PlayersThrown>();
    public List<PlayerController> PlayersInRange = new List<PlayerController>();
    public LayerMask LayerMask;
    public GameObject LandingParticles;
    private float RayGroundCheckLenght = 3f;
    private float PushForce;
    private float PushForceMultiplier;
    private float MaxDistance = 10f;
    private Vector3 PushDir;
    private Coroutine LandingCheckCoroutine;

    public void ClearLists()
    {
        if (LandingCheckCoroutine != null)
        {
            StopCoroutine(LandingCheckCoroutine);
        }
        ThrownPlayers.Clear();
        PlayersInRange.Clear();
    }

    public void AddThrownPlayer(GameObject pPlayerThrown, bool pHasLanded)
    {
        PlayersThrown playersThrown = new PlayersThrown(pPlayerThrown, pHasLanded);

        ThrownPlayers.Add(playersThrown);
    }

    public void CheckWhenPlayerLands()
    {
        LandingCheckCoroutine = StartCoroutine(LandingCheck());
    }
    
    private IEnumerator LandingCheck()
    {
        yield return new WaitForSeconds(1f);
        while (ThrownPlayers.Any(a => !a.HasLanded))
        {
            foreach (var player in ThrownPlayers)
            {
                var origin = player.PlayerThrown.transform.position;
                var dir = Vector3.down;
                RaycastHit rayData;
                var raycast = Physics.Raycast(origin, dir, out rayData, RayGroundCheckLenght, LayerMask, QueryTriggerInteraction.Ignore);

                //abort when hooked
                if (player.PlayerThrown.GetComponent<PlayerController>().CurrentState == PlayerController.State.Hooked)
                {
                    player.HasLanded =true;
                }

                if (raycast && !player.HasLanded)
                {
                    //Debug.Log("Racast Hit " + rayData.transform);
                    player.HasLanded = true;
                    PushPlayers(player.PlayerThrown);
                    player.PlayerThrown.GetComponent<Rigidbody>().velocity *= .01f;
                }
            }
            yield return new WaitForEndOfFrame();
        }
        ThrownPlayers.Clear();
    }

    private void GetPlayersInRange(GameObject pPlayerThrown)
    {
        PlayersInRange = FindObjectsOfType<PlayerController>().ToList().Where(player => player.gameObject != pPlayerThrown && Vector3.Distance(player.transform.position, pPlayerThrown.transform.position) <= MaxDistance).ToList();
    }

    private void PushPlayers(GameObject pPlayerThrown)
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.CatapultHeavyLanding);
        GetPlayersInRange(pPlayerThrown);
        if (LandingParticles != null)
        {
            var particles = Instantiate(LandingParticles, pPlayerThrown.transform.position, Quaternion.identity);
            Destroy(particles, 3f);
        }

        foreach (var player in PlayersInRange)
        {
            CalculatePushDirection(player.transform, pPlayerThrown.transform);
            player.Hookable.Push(PushDir, PushForce);
        }
        PlayersInRange.Clear();
    }

    private void CalculatePushDirection(Transform pPlayerPushed, Transform pPlayerThrown)
    {
        PushDir = (pPlayerPushed.position - pPlayerThrown.position).normalized;
        PushDir.y = 0;
        PushDir = (PushDir.normalized + Vector3.up).normalized;
        //Debug.Log("PushDir = " + PushDir);
        //Debug.Log(MaxDistance - Vector3.Distance(pPlayerThrown.position, pPlayerPushed.position));
        PushForceMultiplier = MaxDistance - Vector3.Distance(pPlayerThrown.position, pPlayerPushed.position);
        PushForce = Mathf.Clamp( 7 * PushForceMultiplier, 1f, 70);
        //Debug.Log("PushForce = " + PushForce);
    }

    public class PlayersThrown
    {
        public GameObject PlayerThrown;
        public bool HasLanded;

        public PlayersThrown(GameObject pPlayerThrown, bool pHasLanded)
        {
            PlayerThrown = pPlayerThrown;
            HasLanded = pHasLanded;
        }
    }
}
