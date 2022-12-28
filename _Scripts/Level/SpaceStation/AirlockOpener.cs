using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AirlockOpener : MonoBehaviour
{
    public bool unavailable;
    public Color TriggeredColor;
    public Color OkColor;
    public List<Renderer> RenderersToSwapWhenTriggered;
    public List<Renderer> RenderersToFlicker;
    public List<GameObject> LightsToRotate;
    public Gradient RandomColors;
    public bool triggered;
    private bool opening;
    public List<ParticleSystem> ParticleSuckingSystems = new List<ParticleSystem>();
    private List<ParticleSystem.EmissionModule> Emissions = new List<ParticleSystem.EmissionModule>();
    List<PlayerController> Players = new List<PlayerController>();
    public float Openness = 0;
    // Start is called before the first frame update
    float speedModifier = 1;
    private AirlockVac Airlock;
    private Coroutine OpenCoroutine = null;
    private Coroutine CloseCoroutine = null;
    private AirlockButton AirlockBtn;
    private NavMeshObstacle NavObstacle;
    private FMOD.Studio.EventInstance SuctionEvent;

    void Awake()
    {
        EventManager.StartListening(EventManager.EventCodes.RoundStart, OnRoundStart);
        EventManager.StartListening(EventManager.EventCodes.PlayersCleared, OnRoundEnd);
        ParticleSuckingSystems.ForEach(a => Emissions.Add(a.emission));
        Emissions.ForEach(a => a.enabled = false);
        ParticleSuckingSystems.ForEach(a => a.gameObject.SetActive(true));
        Airlock = GetComponent<AirlockVac>();
        AirlockBtn = FindObjectOfType<AirlockButton>();
        NavObstacle = GetComponentInChildren<NavMeshObstacle>();
        NavObstacle.enabled = false;
    }

    void OnRoundStart()
    {
        Openness = 0;
    }

    private void OnRoundEnd()
    {
        if (OpenCoroutine != null)
        {
            StopCoroutine(OpenCoroutine);
            OpenCoroutine = null;
            CloseCoroutine = StartCoroutine(Close());
        }  
    }

    // Update is called once per frame
    void Update()
    {
        if (Openness < .35f)
        {
            speedModifier = 1f / Mathf.Clamp(Openness,.1f,float.MaxValue);
        }
        else
        {
            speedModifier = .75f;
        }

        if (opening)
        {
            Openness = Mathf.MoveTowards(Openness,1,Time.deltaTime * speedModifier);
        }
        else
        {
            Openness = Mathf.MoveTowards(Openness, 0, Time.deltaTime * speedModifier);
        }

        if (triggered)
        {
            foreach (var item in LightsToRotate)
            {
                item.transform.Rotate(transform.up, 300 * Time.deltaTime);
            }
        }
    }

    //void OnTriggerEnter(Collider col)
    //{
    //    var PC = col.GetComponent<PlayerController>();
        
    //    if ((col.GetComponent<Hook>() != null || PC != null) && !triggered && !unavailable)
    //    {
    //        AudioManager.Instance.PlayEventWithParameter(AudioManager.Instance.HazardSound.AirlockWarning, Constants.FmodParameters.AirlockIndex, Airlock.DoorIndex, out FMOD.Studio.EventInstance pEvent);
    //        Animator.SetTrigger("play");
    //        triggered = true;
    //        unavailable = true;
    //        OpenCoroutine = StartCoroutine(Open());
    //    }
    //}

    public void OpenAirlock()
    {
        AudioManager.Instance.PlayEventWithParameter(AudioManager.Instance.HazardSound.AirlockWarning, Constants.FmodParameters.AirlockIndex, Airlock.DoorIndex, out FMOD.Studio.EventInstance pEvent);
        triggered = true;
        unavailable = true;
        OpenCoroutine = StartCoroutine(Open());
    }

    IEnumerator Open()
    {
        NavObstacle.enabled = true;
        StartCoroutine(FlickerButtons());
        SetLightsActive(true);
        foreach (var item in RenderersToSwapWhenTriggered)
        {
            ChangeColor(item, TriggeredColor);
        }
        yield return new WaitForSeconds(1f);
        AudioManager.Instance.PlayEventWithParameter(AudioManager.Instance.HazardSound.AirlockOpen, Constants.FmodParameters.AirlockIndex, Airlock.DoorIndex, out FMOD.Studio.EventInstance pEvent);
        AudioManager.Instance.PlayEvent(AudioManager.Instance.HazardSound.AirlockSuction, out SuctionEvent);
        opening = true;
        Emissions.ForEach(a => a.enabled = true);
        //Emissions.enabled = true;
        yield return new WaitForSeconds(6f);
        CloseCoroutine = StartCoroutine(Close());
        OpenCoroutine = null;
    }

    void ChangeColor(Renderer pRend,Color pColor)
    {
        pRend.material.color = pColor;
    }

    void SetLightsActive(bool pValue)
    {
        foreach (var item in LightsToRotate)
        {
            item.SetActive(pValue);
        }
    }

    IEnumerator FlickerButtons()
    {
        for (int i = 0; i < 10; i++)
        {
            foreach (var item in RenderersToFlicker)
            {
                ChangeColor(item,RandomColors.Evaluate(Random.Range(.1f,1f)));
            }
            yield return new WaitForSeconds(Random.Range(.05f,.1f));
        }
    }

    IEnumerator Close()
    {
        if (opening != false)
        {
            AudioManager.Instance.PlayEventWithParameter(AudioManager.Instance.HazardSound.AirlockClose, Constants.FmodParameters.AirlockIndex, Airlock.DoorIndex, out FMOD.Studio.EventInstance pEvent);
        }
        Emissions.ForEach(a => a.enabled = false);
        //Emissions.enabled = false;
        opening = false;
        triggered = false;
       
        foreach (var item in RenderersToFlicker)
        {
            ChangeColor(item, OkColor);
        }
       
        yield return new WaitForSeconds(1f);
        SuctionEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        NavObstacle.enabled = false;
        SetLightsActive(false);
        foreach (var item in RenderersToSwapWhenTriggered)
        {
            ChangeColor(item, OkColor);
        }
        
        CloseCoroutine = null;
        yield return new WaitForSeconds(1f);
        //RandomAirlockbutton.Instance.Done();
        AirlockBtn.Done();
        unavailable = false;
    }

    //void OnTriggerExit(Collider col)
    //{
    //    var PC = col.GetComponent<PlayerController>();
    //    if (PC != null && Players.Contains(PC))
    //    {
    //        Players.Remove(PC);
    //    }
    //}
}
