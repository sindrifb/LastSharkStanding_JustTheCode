using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HookController : MonoBehaviour {

    public Hook StandardHook;
    public Hook AnchorHook;
    public Hook HarpoonHook;
    public GameObject HookOnFishRod;
    public GameObject FishingRod;
    //public Hook FlyingFishHook;

    public Hook CurrentHook;
    private PlayerController PlayerController;
    public List<Hook> Hooks = new List<Hook>();

    protected Animator SharkAnimator;
    protected string CurrentHookAnimation = "";

    private void Awake()
    {
        PlayerController = GetComponent<PlayerController>();
        Hooks = GetComponentsInChildren<Hook>().ToList();
    }

    public void InitializeHooks()
    {
        SharkAnimator = PlayerController.Animator;
        Hooks.ForEach(hooks => hooks.Initialize(PlayerController, this));
        SetActiveHook(StandardHook);
    }

    public void SetActiveHook(Hook pHook)
    {
        CurrentHook = pHook;
        CurrentHook.SetActiveHook(true);

        if (!(pHook is StandardHook))
        {
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.HookSound.PowerUpPickup);
            FishingRod.SetActive(false);
            HookOnFishRod.SetActive(false);
            CurrentHook.SetHookRenderAndColliderActive(false);
        }
        else
        {
            FishingRod.SetActive(true);
            HookOnFishRod.SetActive(true);
            CurrentHook.SetHookRenderAndColliderActive(false);
        }

        
        SetHookIdleAnimation(CurrentHook);

        Hooks = GetComponentsInChildren<Hook>().ToList();

        var disableHooks = Hooks.Where(a => a != pHook).ToList();
        disableHooks.ForEach(a => a.SetActiveHook(false));
    }

    public void SetHookIdleAnimation(Hook pCurrentHook)
    {
        if (pCurrentHook is StandardHook)
        {
            PlayHookAnimation(Constants.AnimationParameters.HookIdle);
        }
        //else if (pCurrentHook is HarpoonHook)
        //{
        //    PlayHookAnimation(Constants.AnimationParameters.HarpoonIdle);
        //}
        else if (pCurrentHook is AnchorHook)
        {
            PlayHookAnimation(Constants.AnimationParameters.AnchorIdle);
        }
    }

    public void PlayHookAnimation(string pAnimationName)
    {
        if (CurrentHookAnimation != "")
        {
            SharkAnimator.SetBool(CurrentHookAnimation, false);
        }
        SharkAnimator.SetBool(pAnimationName, true);
        CurrentHookAnimation = pAnimationName;
    }
}
