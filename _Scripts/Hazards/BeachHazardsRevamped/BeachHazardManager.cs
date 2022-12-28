using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class BeachHazardManager : MonoBehaviour
{
    public static BeachHazardManager Instance;
    public AudioMixerGroup Mixer;
    int Difficulty => GameManager.Instance.Difficulty;
    public int MaxBeachBalls = 5;
    public int MaxMines = 5;
    public int MaxWaves = 2;
    public int MaxConcurrentHazards = 10;

    public MeteorHazard MeteorHazard;
    public WaveHazard WaveHazard;
    public MineHazard MineHazard;

    private float TimeSinceLastHazard;
    private float TimeSinceLastWave;
    private float TimeSinceLastBeachball;
    private float TimeSinceLastMine;
    //distances from group center
    private float AverageSpread;
    private float TotalSpread;
    private float MinSpread;
    private float MaxSpread;

    bool shootWave;
    bool shootBeachball;
    bool shootMine;
    //for getting the center of the group
    private CameraFollow CameraFollow;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CameraFollow = Camera.main.GetComponent<CameraFollow>();
        StartCoroutine(HazardTest());
        EventManager.StartListening(EventManager.EventCodes.RoundEnd, CleanLevel);
        EventManager.StartListening(EventManager.EventCodes.GameEnd, CleanLevel);

    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    WaveHazard.FireWave();
        //    //MeteorHazard.FireMeteor();
        //    //MineHazard.FireMine();
        //}


        if (Difficulty == 0)
        {
            return;
        }
        TimeSinceLastHazard += Time.deltaTime;
        TimeSinceLastWave += Time.deltaTime;
        TimeSinceLastBeachball += Time.deltaTime;
        TimeSinceLastMine += Time.deltaTime;
    }

    void CleanLevel()
    {
        MineHazard.SweepMines();
        WaveHazard.RemoveActiveWaves();
        MeteorHazard.RemoveActiveMeteors();
    }

    IEnumerator HazardTest()
    {
        if (Difficulty > 0)
        {
            ChooseHazard();
            if (MeteorHazard.CurrentlyActive < MaxBeachBalls && shootBeachball)
            {
                MeteorHazard.FireMeteor();
                TimeSinceLastBeachball = 0f;
            }
            if (MineHazard.CurrentlyActive < MaxMines && shootMine)
            {
                MineHazard.FireMine();
                TimeSinceLastMine = 0;
            }
            if (WaveHazard.CurrentlyActive < MaxWaves && shootWave)
            {
                WaveHazard.FireWave();
                TimeSinceLastWave = 0;
            }
            shootBeachball = shootMine = shootWave = false;
        }
       
        
        yield return new WaitForSeconds(1f);
        StartCoroutine(HazardTest());
    }

    float GetTotalSpread()
    {
        if (!CameraFollow.Players.Any())
        {
            return 0;
        }
        var players = CameraFollow.Players.Where(a => a != null);
        Vector3 center = CameraFollow.GetGroupCenter();
        List<float> distances = players.Select(a => (a.transform.position - center).magnitude).OrderBy(a => a).ToList();
        MinSpread = distances.FirstOrDefault();
        MaxSpread = distances.LastOrDefault();
        AverageSpread = distances.Average();
        TotalSpread = distances.Sum();
        //PrintVals();
        return TotalSpread;
    }

    void ChooseHazard()
    {
        GetTotalSpread();

        float minMaxdiff = MinSpread / MaxSpread;
        float avgDiff = AverageSpread / MaxSpread;
        //print("min/max "+minMaxdiff);
        //print("avg/max" + avgDiff);
        if ((AverageSpread < 1.5f || Random.Range(0, 100) <= 10) && TimeSinceLastWave > Mathf.Clamp(13f - Difficulty, 0, 10))
        {
            shootWave = true;
        }

        if ((AverageSpread > 8f || Random.Range(0, 100) <= 10) && TimeSinceLastBeachball > Mathf.Clamp(11f - Difficulty, 0, 10))
        {
            shootBeachball = true;
        }

        if (Random.Range(0,100) <= 10 && TimeSinceLastMine > Mathf.Clamp(10f - Difficulty, 0, 10))
        {
            shootMine = true;
        }
    }

    public void PlaySound(AudioClip audioClip)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.PlayOneShot(audioClip);
        source.outputAudioMixerGroup = Mixer;
        Destroy(source, 4f);
    }

    void PrintVals()
    {
        //print("Min " + MinSpread);
        //print("Max " + MaxSpread);
        //print("Avg " + AverageSpread);
        //print("Total " + TotalSpread);
    }
}
