using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AirlockButton : MonoBehaviour
{
    public List<AirlockOpener> Airlocks = new List<AirlockOpener>();
    private Animator Animator;
    private SphereCollider ButtonTrigger;
    private string PrevAnimBool;

    // Start is called before the first frame update
    void Start()
    {
        Animator = GetComponentInChildren<Animator>();
        ButtonTrigger = GetComponent<SphereCollider>();
    }

    void OnTriggerEnter(Collider col)
    {
        StartCoroutine(OpenAirlocks(col));
    }

    private IEnumerator OpenAirlocks(Collider pCol)
    {
        var PC = pCol.GetComponent<PlayerController>();
        var airlock = Airlocks[Random.Range(0, Airlocks.Count)];
        var playersAlive = GameManager.Instance.Players.Count;

        if ((pCol.GetComponent<Hook>() != null || PC != null) && !airlock.triggered && !airlock.unavailable)
        {
            //Animator.SetTrigger("ButtonDown");
            PlayButtonAnimation("ButtonDown");
            ButtonTrigger.enabled = false;

            yield return new WaitForSeconds(0.33f);

            airlock.OpenAirlock();
            if (playersAlive == 2)
            {
                var secondAirlock = Airlocks.Where(a => a != airlock).ToList();
                secondAirlock[Random.Range(0, secondAirlock.Count)].OpenAirlock();
            }
        }
    }

    public void Done()
    {
        //Animator.SetTrigger("Reset");
        PlayButtonAnimation("Reset");
        StartCoroutine(LateSetButtonEnabled());
    }

    private IEnumerator LateSetButtonEnabled()
    {
        yield return new WaitForSeconds(0.5f);
        ButtonTrigger.enabled = true;
    }

    private void PlayButtonAnimation(string pBoolName)
    {
        if (PrevAnimBool != "")
        {
            Animator.SetBool(PrevAnimBool, false);
        }
        
        Animator.SetBool(pBoolName, true);
        PrevAnimBool = pBoolName;
    }
}
