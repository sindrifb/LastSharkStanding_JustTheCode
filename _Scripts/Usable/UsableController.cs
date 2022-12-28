using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using System.Linq;

public class UsableController : MonoBehaviour 
{
    public Usable CurrentUsable { get; private set; }
    public Usable StandardHook;
    public GameObject FishingRod;
    public GameObject HookOnFishingRod;
    public PlayerController PlayerController { get; private set; }
    private Animator SharkAnimator;
    private string CurrentUsableAnimation = "";
    public Transform DisplayObjectConstraint { get; private set; }
    [SerializeField]
    private GameObject PowerUpPickupParticles;

    public void Initialize(PlayerController pPCntrl)
    {
        PlayerController = pPCntrl;
        SharkAnimator = pPCntrl.Animator;
        SetActiveUsable(StandardHook);
        DisplayObjectConstraint = GetComponentInChildren<ConstraintGetter>().Constraint;
    }

    public void ResetToStandardHook(bool pChangeToIdle = true)
    {
        SetActiveUsable(StandardHook);
        ResetHookOnFishingPoleVelocity();
        if (pChangeToIdle)
        {
            PlayerController.ChangeState(PlayerController.State.Idle);
        }
    }

    public void ResetHookOnFishingPoleVelocity()
    {
        StartCoroutine(LateChangeVelocityOnFishingPoleHook());
    }

    private IEnumerator LateChangeVelocityOnFishingPoleHook()
    {
        yield return new WaitForSeconds(0.05f);
        HookOnFishingRod.transform.position = HookOnFishingRod.GetComponent<SpringJoint>().connectedBody.position;
        HookOnFishingRod.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    public void SetActiveUsable(Usable pUsable)
    {
        if (CurrentUsable != null && CurrentUsable != StandardHook)
        {
            Destroy(CurrentUsable.DisplayObject);
            Destroy(CurrentUsable.AimModule.Aim);
            if (CurrentUsable.transform.IsChildOf(transform))
            {
                Destroy(CurrentUsable.gameObject);
            }
        }

        //if (CurrentUsable != pUsable)
        //{
        CurrentUsable = pUsable;
        pUsable.Initialize(PlayerController, this);          

        bool isStandardHook = pUsable == StandardHook;

        FishingRod.SetActive(isStandardHook);
        HookOnFishingRod.SetActive(isStandardHook);

        StandardHook.gameObject.SetActive(isStandardHook);
        if (!isStandardHook)
        {
            ConstraintSource source = new ConstraintSource();
            source.sourceTransform = DisplayObjectConstraint;
            source.weight = 1;
            var pCon = CurrentUsable.DisplayObject.GetComponent<ParentConstraint>();
            pCon.SetSource(0, source);
            pCon.constraintActive = true;
            CurrentUsable.DisplayObject.SetActive(true);
        }

        SetUsableIdleAnimation();
        //}
    }

    //public void PlayUsableAnimation(string pAnimationName)
    //{
    //    if (CurrentUsableAnimation != "")
    //    {
    //        SharkAnimator.SetBool(CurrentUsableAnimation, false);
    //    }
    //    SharkAnimator.SetBool(pAnimationName, true);
    //    CurrentUsableAnimation = pAnimationName;
    //}

    public void SetUsableIdleAnimation()
    {
        //PlayUsableAnimation(CurrentUsable.IdleAnimParameter);
        PlayerController.PlayUpperBodyAnimation(CurrentUsable.IdleAnimParameter);
    }

    public void PickUpPowerUp(GameObject pPowerUp)
    {
        pPowerUp.transform.SetParent(transform);
        pPowerUp.transform.localPosition = Vector3.zero;
        pPowerUp.SetActive(true);
        SetActiveUsable(pPowerUp.GetComponent<Usable>());

        if (PowerUpPickupParticles != null)
        {
            var particleSystem = Instantiate(PowerUpPickupParticles, transform.position, Quaternion.identity, transform);

            ParticleSystem[] PS = particleSystem.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < PS.Length; i++)
            {
                ParticleSystem.MainModule main = PS[i].main;
                main.startColor = PlayerController.PlayerColor;
            }
            Destroy(particleSystem, 3f);
        }
    }
}
