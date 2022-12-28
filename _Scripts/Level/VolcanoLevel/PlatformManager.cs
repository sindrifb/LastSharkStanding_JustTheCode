using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.AI;

public class PlatformManager : MonoBehaviour 
{
    [SerializeField]
    //private Transform StaticPlatform;
    //[SerializeField]
    private List<SinkingController> Platforms = new List<SinkingController>();
    [SerializeField]
    private GameObject PlatformsParent;

    public NavMeshSurface NavSurface;

    private List<SinkingController> SortedPlatforms;
    private Coroutine SinkCoroutine;

	private void Start() 
	{
        Platforms = PlatformsParent.GetComponentsInChildren<SinkingController>().ToList();
        CreateSortedList();
        RebuildNavMesh();

        EventManager.StartListening(EventManager.EventCodes.RoundStart, StartSinkSequence);
        //Debug.Log("RoundStart started listening");
        EventManager.StartListening(EventManager.EventCodes.RoundEnd, ResetPlatforms);
        //Debug.Log("RoundEnd started listening");
        EventManager.StartListening(EventManager.EventCodes.GameEnd, ResetPlatforms);
        //Debug.Log("GameEnd started listening");
    }

    private void StartSinkSequence()
    {
        RebuildNavMesh();
        SinkCoroutine = StartCoroutine(SinkSequence());
    }

    private IEnumerator SinkSequence()
    {
        while (SortedPlatforms.Count > 0)
        {
            yield return new WaitForSeconds(3f);

            //var chosenOne = PlatformToSinkFarthestToClosest();
            var chosenOne = PlatformClosestToAllPlayers();
            //var chosenOne = PlatformClosestToAPlayer();

            SortedPlatforms.Remove(chosenOne);
            chosenOne.Sink();
            RebuildNavMesh();

            yield return new WaitForSeconds(Random.Range(0f, 1f));
            //UpdateNavMesh(chosenOne);
            
            
        }
    }

    private void CreateSortedList()
    {
        //sort by ascending order (lowest to highest)
        SortedPlatforms = Platforms.OrderBy(a => (a.transform.position - transform.position).sqrMagnitude).ToList();
    }

    public void ResetPlatforms()
    {
        //Debug.Log("Reset level");
        StopCoroutine(SinkCoroutine);
        Platforms.Where(a => !SortedPlatforms.Contains(a)).ToList().ForEach(a => a.ResetPlatform());
        CreateSortedList();
    }

    /*finds the 3 platforms farthest away from the middle platform and picks the one 
     * that has the lowest accumulative distance to the players*/
    private SinkingController PlatformToSinkFarthestToClosest()
    {
        var candidates = new List<SinkingController>(SortedPlatforms.Skip(Mathf.Max(0, SortedPlatforms.Count() - 3)));
        List<PlayerController> players = FindObjectsOfType<PlayerController>().ToList();

        int indexOfLowest = 0;
        float lowestTotalMagnitude = Mathf.Infinity;

        for (int i = 0; i < candidates.Count; i++)
        {
            float totalMagnitude = 0;

            //adds the sqrmagnitude of the distance between each player and the platform together
            foreach (var player in players)
            {
                totalMagnitude += (candidates[i].ChildPos() - player.transform.position).sqrMagnitude;
            }

            if (totalMagnitude < lowestTotalMagnitude)
            {
                lowestTotalMagnitude = totalMagnitude;
                indexOfLowest = i;
            }
        }
        //return the candidate with the lowest collective magnitude
        return candidates[indexOfLowest];
    }

    //finds the platform the has the lowest accumulative distance to all players
    private SinkingController PlatformClosestToAllPlayers()
    {
        List<PlayerController> players = FindObjectsOfType<PlayerController>().Where(a => a.IsAvailable).ToList();

        int indexOfLowest = 0;
        float lowestTotalDistance = Mathf.Infinity;

        for (int i = 0; i < SortedPlatforms.Count; i++)
        {
            float totalDistance = 0;

            //adds the sqrmagnitude of the distance between each player and the platform together
            foreach (var player in players)
            {
                var platformPos = new Vector2(SortedPlatforms[i].ChildPos().x, SortedPlatforms[i].ChildPos().z);
                var playerPos = new Vector2(player.transform.position.x, player.transform.position.z);
                //totalDistance += (SortedPlatforms[0].transform.position - player.transform.position).sqrMagnitude;
                totalDistance += Vector2.Distance(platformPos, playerPos);
            }

            if (totalDistance < lowestTotalDistance)
            {
                lowestTotalDistance = totalDistance;
                indexOfLowest = i;
            }
        }
        //return the candidate with the lowest collective magnitude
        return SortedPlatforms[indexOfLowest];
    }

    //finds platform that is closest to a player
    private SinkingController PlatformClosestToAPlayer()
    {
        List<PlayerController> players = FindObjectsOfType<PlayerController>().ToList();

        int indexOfLowest = 0;
        float lowestDistance = Mathf.Infinity;

        for (int i = 0; i < SortedPlatforms.Count; i++)
        {
            foreach (var player in players)
            {
                Debug.Log(SortedPlatforms[0].ChildPos());
                var platformPos = new Vector2(SortedPlatforms[i].ChildPos().x, SortedPlatforms[i].ChildPos().z);
                var playerPos = new Vector2(player.transform.position.x, player.transform.position.z);
                var dist = Vector2.Distance(platformPos, playerPos);

                if (lowestDistance > dist)
                {
                    lowestDistance = dist;
                    indexOfLowest = i;
                }
                
            }

        }
        //return the candidate with the lowest collective magnitude
        return SortedPlatforms[indexOfLowest];
    }

    private void RebuildNavMesh()
    {
        //for (int i = 0; i < NavSurface.Length; i++)
        //{
        //    if (NavSurface[i].navMeshData != null)
        //    {
        //        NavSurface[i].RemoveData();
        //    }
        //    NavSurface[i].BuildNavMesh();
        //}
        if (NavSurface.navMeshData != null)
        {
            NavSurface.RemoveData();
        }
        NavSurface.BuildNavMesh();
    }

    private void UpdateNavMesh(SinkingController pPlatform)
    {
        var surface = pPlatform.GetComponentInChildren<NavMeshSurface>();
        surface.RemoveData();
        surface.BuildNavMesh();
    }
}
