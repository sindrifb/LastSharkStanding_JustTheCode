using System.Collections;
using System.Collections.Generic;
using BansheeGz.BGSpline.Curve;
using UnityEngine;
using UnityEngine.AI;

public class HazardHookable : Hookable
{
    //public HingeJoint[] Hinges;
    public float TimeOpen;
    [SerializeField]
    private List<GameObject> Graters;
    private List<Quaternion> GraterClosedRotation = new List<Quaternion>();
    public BoxCollider GraterGroundCollider;
    public GameObject GraterTriggerParticles;
    public GameObject GraterCloseParticles;
    private Vector3 OriginalPos;
    private GameObject HookableParticles;
    public Animator GraterAnimator;
    private Coroutine GraterCoroutine;
    public NavMeshObstacle NavObstacle;

    //public override void Activate(Usable pUsable, BGCurve pCurve = null)
    //{
    //    base.Activate(pUsable, pCurve);
    //    IsAvailable = false;
    //}

    private void Awake()
    {
        foreach (var grater in Graters)
        {
            GraterClosedRotation.Add(grater.transform.rotation);
        }
        OriginalPos = transform.position;
        EventManager.StartListening(EventManager.EventCodes.PlayersCleared, ResetHazard);
    }

    public override void Initialize()
    {
        base.Initialize();
        //SetGraterClosed(true);
        StartCoroutine(CloseGraters());
        HookableParticles = transform.GetChild(0).gameObject;
        GraterTriggerParticles.SetActive(false);
        GraterCloseParticles.SetActive(false);
        PlayeGraterAnim(false);
        NavObstacle.enabled = false;
    }


    public override void UpdateRagdoll()
    {
        
    }

    public override void OnHooked()
    {
        base.OnHooked();

        NavObstacle.enabled = true;
    }

    public override void OnFinishedBeingHooked()
    {
        base.OnFinishedBeingHooked();

        
        GraterTriggerParticles.SetActive(true);
        GraterCoroutine = StartCoroutine(OpenAndResetHinges());
    }

    public override void Push(Vector3 pDir, float pForce)
    {
        base.Push(pDir, pForce);
        GraterTriggerParticles.SetActive(true);
        GraterCoroutine = StartCoroutine(OpenAndResetHinges());
    }

    //private void SetGraterClosed(bool pValue)
    //{
    //    //for (int i = 0; i < Hinges.Length; i++)
    //    //{
    //    //    Hinges[i].useSpring = pValue;
    //    //}
    //    //GraterGroundCollider.enabled = pValue;
    //}

    private void OpenGraters()
    {
        foreach (var grater in Graters)
        {
            grater.GetComponent<Rigidbody>().isKinematic = false;
        }
        GraterGroundCollider.enabled = false;
    }

    private IEnumerator CloseGraters()
    {
        PlayeGraterAnim(false);
        List<Quaternion> startRot = new List<Quaternion>();
        foreach (var grater in Graters)
        {
            grater.GetComponent<Rigidbody>().isKinematic = true;
            startRot.Add(grater.transform.rotation);
        }

        float t = 0;

        while (Graters[Graters.Count - 1].transform.rotation != GraterClosedRotation[Graters.Count - 1])
        {
            yield return null;
            t += Time.deltaTime / 0.5f;

            for (int i = 0; i < Graters.Count; i++)
            {
                Graters[i].transform.rotation = Quaternion.Lerp(startRot[i], GraterClosedRotation[i], t);
            }
        }

        GraterGroundCollider.enabled = true;
    }

    private void PlayeGraterAnim(bool pValue)
    {
        GraterAnimator.SetBool("OpenGrater", pValue);
    }

    private void EnableColliderAndRenderer(bool pValue)
    {
        GetComponent<SphereCollider>().enabled = pValue;
        GetComponent<MeshRenderer>().enabled = pValue;
        HookableParticles.SetActive(pValue);
    }

    private void ResetVelocityAndPosition()
    {
        transform.rotation = Quaternion.identity;
        RagdollRigidbody.velocity = Vector3.zero;
        RagdollRigidbody.isKinematic = true;
        transform.position = OriginalPos;        
    }

    public void ResetHazard()
    {
        if (GraterCoroutine != null)
        {
            StopCoroutine(GraterCoroutine);
        }
        //SetGraterClosed(true);
        StartCoroutine(CloseGraters());
        ResetVelocityAndPosition();
        EnableColliderAndRenderer(true);
        IsAvailable = true;
        NavObstacle.enabled = false;
    }

    private IEnumerator OpenAndResetHinges()
    {
        
        PlayeGraterAnim(true);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.GraterOpen);
        yield return new WaitForSeconds(0.45f);
        //SetGraterClosed(false);
        OpenGraters();
        EnableColliderAndRenderer(false);

        yield return new WaitForSeconds(3f);
        
        ResetVelocityAndPosition();
        GraterTriggerParticles.SetActive(false);
        
        yield return new WaitForSeconds(TimeOpen-3);
        //SetGraterClosed(true);
        StartCoroutine(CloseGraters());
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.GraterClose);
        GraterCloseParticles.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        EnableColliderAndRenderer(true);
        NavObstacle.enabled = false;
        yield return new WaitForSeconds(1f);
        GraterCloseParticles.SetActive(false);
        IsAvailable = true;
    }
}
