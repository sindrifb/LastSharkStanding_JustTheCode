using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Rewired;

public class PlayerController : MonoBehaviour
{
    public float CharacterDrag = .9f;
    public BotController AiShark { get; private set; }
    public Vector3 Velocity;
    public GameObject LandingParticleEffectPrefab;

    public Color PlayerColor = Color.red;
    //public GameObject Aim;
    //public GameObject AimAnchor;
    //public GameObject AimHarpoon;
    public bool IsAvailable { get; private set; }
    [HideInInspector]
    public bool IsTravelingByOwnHook { get; private set; }
    public int PlayerID; //{get; private set;}
    public int RewiredID; //{get; private set;}
    private Player RewiredPlayer;

    public Animator Animator { get; private set; }
    private string CurrentUpperBodyAnimation = "";
    private string CurrentLowerBodyAnimation = "";
    public MovementController MovementController { get; private set; }
    public UsableController UsableController { get; private set; }
    private CheckIfGrounded CheckIfGrounded;
    public Rigidbody Rigidbody { get; private set; }
    public Hookable Hookable { get; private set; }
    public GameObject Skin { get; private set; }
    private FMOD.Studio.EventInstance FootStepEvent;

    // timer to end ragdoll State
    private bool timerToEndRagdollOn = false;
    private float timePassed = 0; 
    public float timerEnd { get; set; } = 2;

    private Coroutine ImmobileCoroutine;
    public bool IsBot { get; private set; }

    public State CurrentState;
    public enum State
    {
        Idle, WindUp, Throwing, Reeling, Ragdoll, Hooked, Staggered, none
    }

    public void Initialize()
    {
        AiShark = GetComponent<BotController>();
        if (RewiredID != 5)
        {
            RewiredPlayer = ReInput.players.GetPlayer(RewiredID);
        }
        Rigidbody = GetComponent<Rigidbody>();
        GetComponent<CharacterController>().detectCollisions = false;
        Animator = GetComponentInChildren<Animator>();
        MovementController = GetComponent<MovementController>();
        UsableController = GetComponent<UsableController>();
        CheckIfGrounded = GetComponent<CheckIfGrounded>();
        Hookable = GetComponentInChildren<Hookable>();
        MovementController.Initialize(this);
        Hookable.Initialize();
        UsableController.Initialize(this);
        MovementController.InitializeStandardMovement();
        AudioManager.Instance.PlayEventWithParameter(AudioManager.Instance.PlayerSound.Footstep, "PlayerVelocity", 0f, out FootStepEvent);

        ChangeState(State.none);
    }

    public void SetUpPlayer(int pPlayerID, int pRewiredID, Color pPlayerColor, GameObject pSkin)
    {
        PlayerID = pPlayerID;
        RewiredID = pRewiredID;
        PlayerColor = pPlayerColor;
        IsBot = RewiredID == 5;

        if (pSkin != null)
        {
            var skin = Instantiate(pSkin, transform);
            Skin = skin;
        }

        var Lines = GetComponentsInChildren<LineRenderer>(true);
        var ParticleSystems = GetComponentsInChildren<ParticleSystem>(true);
        //Aim.GetComponent<MeshRenderer>().material.color = pPlayerColor;

        for (int j = 0; j < ParticleSystems.Length; j++)
        {
            ParticleSystem.MainModule main = ParticleSystems[j].main;
            main.startColor = pPlayerColor;
        }

        for (int i = 0; i < Lines.Length; i++)
        {
            Lines[i].startColor = pPlayerColor;
            Lines[i].endColor = pPlayerColor;
        }

        Initialize();
    }

    private void Update()
    {
        UpdatePhysics();
        //if (Input.GetKeyDown(KeyCode.P) /*&& PlayerID == 1*/)
        //{
        //    //Hookable.Push(Vector3.right + Vector3.up, 10f);
        //    Hookable.Push(Vector3.right/2f + Vector3.up,40);
        //}

        //if (RewiredPlayer?.GetButtonDown("PauseGame") ?? false)
        //{
        //    GameManager.Instance.SetGamePaused(RewiredID, true);
        //}

        

        switch (CurrentState)
        {
            case State.Idle:
                MovementController.UpdateMovement();
                MovementController.UpdateNotGrounded();
                MovementController.UpdatePush();
                AudioManager.Instance.ChangeEventParameter("PlayerVelocity", MovementController.Direction.magnitude, FootStepEvent);
                if (RewiredPlayer?.GetButtonDown("Dash") ?? AiShark.DashInput)
                {
                    MovementController.Dash();
                    //if (Animator.GetBool(Constants.AnimationParameters.Walk))
                    //{
                    //    Animator.SetBool(Constants.AnimationParameters.Walk, false);
                    //}
                }
                else if (RewiredPlayer?.GetButtonDown("Hook") ?? AiShark.AttackInput)
                {
                    ChangeState(State.WindUp);
                }

                break;
            case State.WindUp:
                UsableController.CurrentUsable.UpdateWindUp();
                MovementController.UpdateMovement();
                MovementController.UpdateNotGrounded();
                MovementController.UpdatePush();
                AudioManager.Instance.ChangeEventParameter("PlayerVelocity", Mathf.Clamp(MovementController.Direction.magnitude, 0f, 0.3f), FootStepEvent);
                if (RewiredPlayer?.GetButtonDown("Dash") ?? AiShark.DashInput)
                {
                    //InterruptWindUp();
                    //UsableController.CurrentUsable.Interrupt(CurrentState);
                    MovementController.Dash();
                    return;
                }
                else if (RewiredPlayer?.GetButtonUp("Hook") ?? !AiShark.AttackInput)
                {
                    //ChangeState(State.Throwing);
                    UsableController.CurrentUsable.Throw();
                }

                break;
            case State.Throwing:
                MovementController.UpdatePush();
                MovementController.UpdateNotGrounded();
                AudioManager.Instance.ChangeEventParameter("PlayerVelocity", 0, FootStepEvent);
                //UsableController.CurrentUsable.UpdateThrowing();
                break;
            case State.Reeling:
                MovementController.UpdatePush();
                MovementController.UpdateNotGrounded();
                AudioManager.Instance.ChangeEventParameter("PlayerVelocity", 0, FootStepEvent);
                //if (UsableController.CurrentUsable is Yoink)
                //{
                //    var yoink = UsableController.CurrentUsable as Yoink;
                //    yoink.UpdateReeling();
                //}
                break;
            case State.Ragdoll:
                Hookable.UpdateRagdoll();
                AudioManager.Instance.ChangeEventParameter("PlayerVelocity", 0, FootStepEvent);
                break;
            case State.Hooked:
                AudioManager.Instance.ChangeEventParameter("PlayerVelocity", 0, FootStepEvent);
                //if (IsTravelingByOwnHook)
                //{
                //    IsAvailable = true;
                //    //if (UsableController.CurrentUsable is Yoink)
                //    //{
                //    //    var yoink = UsableController.CurrentUsable as Yoink;
                //    //    yoink.UpdateTravelByOwnHook();
                //    //}
                //}
                //else
                //{
                //    Hookable.UpdateHooked();
                //}
                break;
            case State.Staggered:
                MovementController.UpdatePush();
                MovementController.UpdateMovement();
                MovementController.UpdateNotGrounded();
                AudioManager.Instance.ChangeEventParameter("PlayerVelocity", 0, FootStepEvent);
                break;
            case State.none:
                MovementController.UpdateMovement();
                AudioManager.Instance.ChangeEventParameter("PlayerVelocity", 0, FootStepEvent);
                break;
            default:
                break;
        }
    }

    public void ChangeState(State pState, bool legalRagdollReset = false)
    {
        switch (pState)
        {
            case State.Idle:
                UsableController.CurrentUsable.AimModule.ResetAim();

                Hookable.RagdollSetActive(false);
                MovementController.ResetRotation();
                MovementController.InitializeStandardMovement();
                UsableController?.SetUsableIdleAnimation();
                IsAvailable = true;
                break;
            case State.WindUp:
                if (IsBot)
                {
                    AiShark.InitializeWindup();
                }
                MovementController.InitializeWindupMovement();
                if (UsableController == null)
                {
                    //print("usable controller null");
                }
                else if (UsableController.CurrentUsable == null)
                {
                    //print("current usable null");
                }
                UsableController?.CurrentUsable?.InitializeWindUp();
                PlayUpperBodyAnimation(Constants.AnimationParameters.Charge);
                IsAvailable = true;
                break;
            case State.Throwing:
                //UsableController.CurrentUsable.Throw();
                if (IsBot)
                {
                    AiShark.SetOwnUsableThrown();
                }
                MovementController.StopParticles();
                PlayLowerBodyAnimation(Constants.AnimationParameters.Standing);
                PlayUpperBodyAnimation(Constants.AnimationParameters.Throw);
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.HookSound.HookThrow);
                IsAvailable = true;
                break;
            case State.Reeling:
                MovementController.StopParticles();
                PlayLowerBodyAnimation(Constants.AnimationParameters.Standing);
                IsAvailable = true;
                PlayUpperBodyAnimation(Constants.AnimationParameters.Pull);
                break;
            case State.Ragdoll:
                UsableController?.SetUsableIdleAnimation();
                MovementController.StopParticles();
                PlayLowerBodyAnimation(Constants.AnimationParameters.Standing);
                IsAvailable = true;
                if (IsBot)
                {
                    AiShark.OnRagdoll();
                }
                break;
            case State.Hooked:
                MovementController.StopParticles();
                PlayLowerBodyAnimation(Constants.AnimationParameters.Standing);
                IsAvailable = true;
                if (IsTravelingByOwnHook)
                {
                    PlayUpperBodyAnimation(Constants.AnimationParameters.TravelByOwnHook);
                }
                else
                {
                    UsableController?.SetUsableIdleAnimation();
                }
                break;
            case State.Staggered:
                IsAvailable = true;
                UsableController.SetUsableIdleAnimation();
                MovementController.InitializeStagger();
                break;
            case State.none:
                UsableController.SetUsableIdleAnimation();
                MovementController.StopParticles();
                PlayLowerBodyAnimation(Constants.AnimationParameters.Standing);
                //IsAvailable = false;
                IsAvailable = true;
                //rotate only
               
                break;
            default:
                break;
        }
        CurrentState = pState;
    }

    private void UpdatePhysics()
    {
        Velocity *= CharacterDrag/*Mathf.Clamp(1 - GetComponent<Rigidbody>().drag, .95f, 1)*/;
        if (Velocity.magnitude < .1f)
        {
            Velocity = Vector3.zero;
        }
        //print(Velocity);
    }

    public void AddForce(Vector3 dir, float force)
    {
        Velocity += dir * force;
    }

    public void AddForce(Vector3 pVelocity)
    {
        Velocity += pVelocity;
    }

    public void Push(Vector3 pDir, float pForce)
    {
        Push(pDir * pForce);
    }

    public void Push(Vector3 pVelocity)
    {
        if (MovementController.IsDashing)
        {
            MovementController.InterruptDash();
        }

        IsTravelingByOwnHook = false;
        //if (CurrentState == State.WindUp)
        //{
        //    UsableController.CurrentUsable.ResetStandardHook();
        //}       
        UsableController.CurrentUsable.Interrupt(CurrentState);

        CurrentState = State.none;
        StartCoroutine(LateSet(State.Ragdoll, 0.1f));

    }

    //to avoid reset of ragdoll before it has gained velocity
    public IEnumerator LateSet(State pState, float pDelay)
    {
        yield return new WaitForSeconds(pDelay);
        if (CurrentState == State.none)
        {
            CurrentState = pState;
        }
    }

    //public void InterruptWindUp()
    //{
    //    ChangeState(State.Idle);
    //    UsableController.CurrentUsable.ResetStandardHook();
    //    IsTravelingByOwnHook = false;
    //}

    //my hookable getting hit NOT my hook hitting someone else
    public void OnBeingHit()
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.PlayerSound.GetHit);
        UsableController.CurrentUsable.Interrupt(CurrentState);
        Velocity = Vector3.zero;
        ChangeState(State.Hooked);
        //HookController.CurrentHook.ConnectHook(false,null);

        IsTravelingByOwnHook = false;
        UsableController.SetUsableIdleAnimation();
        if (MovementController.IsDashing)
        {
            MovementController.InterruptDash();
        }
    }

    public void OnMiss()
    {
        //if (LandingParticleEffectPrefab != null)
        //{
        //    var ps = Instantiate(LandingParticleEffectPrefab, UsableController.CurrentUsable.transform.position, Quaternion.identity);
        //    Destroy(ps, 3f);
        //}
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HookSound.HookHit);
        
        IsTravelingByOwnHook = true;
        ChangeState(State.Hooked);
    }

    public void OnMissDone()
    {
        ChangeState(State.Idle);
    }

    // starts the timer and sets ending time of it
    public void SetTimerRagdolling(float pEndTime = 2f)
    {
        timerEnd = pEndTime;
        timerToEndRagdollOn = true;
    }

    public void OnRagdollDone()
    {
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.PlayerSound.StandUp);
        MovementController.ResetRotation();
        ChangeState(State.Idle);
    }

    public void TravelingByHookDone()
    {
        //transform.position += new Vector3(0, 1, 0);
        if (LandingParticleEffectPrefab != null)
        {
            var ps = Instantiate(LandingParticleEffectPrefab, transform.position + (Vector3.down / 1.3f), Quaternion.identity);
            Destroy(ps, 3f);
        }
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.PlayerSound.StandUp);
        IsTravelingByOwnHook = false;
        UsableController.ResetHookOnFishingPoleVelocity();
        ChangeState(State.Idle);
    }

    public void OnThrowingDone()
    {
        ChangeState(State.Reeling);
    }

    public void OnReelingDone()
    {
        UsableController.ResetHookOnFishingPoleVelocity();
        ChangeState(State.Idle);
    }

    public void OnStaggerDone()
    {
        if (CurrentState == State.Staggered)
        {
            ChangeState(State.Idle);
            Animator.speed = 1;
        }
    }

    public void OnDashing()
    {
        if (CurrentState == State.Idle)
        {
            //AudioController.PlayDash();
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.PlayerSound.Dash);
            //DashParticleSystem.Play();
            ChangeState(State.Staggered);
        }
    }

    public void PlayUpperBodyAnimation(string pAnimationName)
    {

        if (CurrentUpperBodyAnimation != "")
        {
            if (CurrentUpperBodyAnimation == pAnimationName)
            {
                return;
            }
            Animator.SetBool(CurrentUpperBodyAnimation,false);
        }
        Animator.SetBool(pAnimationName,true);
        CurrentUpperBodyAnimation = pAnimationName;
    }

    public void PlayLowerBodyAnimation(string pAnimationName)
    {
        if (CurrentLowerBodyAnimation != "")
        {
            Animator.SetBool(CurrentLowerBodyAnimation, false);
        }
        Animator.SetBool(pAnimationName, true);
        CurrentLowerBodyAnimation = pAnimationName;
    }

    private IEnumerator ResetAnimation(string pAnimationName)
    {
        yield return new WaitForSeconds(.1f);
        Animator.SetBool(pAnimationName, false);
    }

    //private void OnDestroy()
    //{
    //    Destroy(UsableController.CurrentUsable.gameObject);
    //}
    public void OnDeath()
    {
        IsAvailable = false;
        Hookable.IsAvailable = false;
        if (UsableController.CurrentUsable == UsableController.StandardHook)
        {
            UsableController.CurrentUsable.ResetStandardHook();
        }
        if (IsBot)
        {
            AiShark.OnDeath();
        }
        PlayLowerBodyAnimation(UsableController.CurrentUsable.IdleAnimParameter);
        PlayUpperBodyAnimation(UsableController.CurrentUsable.IdleAnimParameter);
        FootStepEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        Animator.enabled = false;
    }

    public void MakeImmobile(float pDuration)
    {
        if (ImmobileCoroutine != null)
        {
            StopCoroutine(ImmobileCoroutine);
        }
        
        ImmobileCoroutine = StartCoroutine(Immobility(pDuration));
    }

    private IEnumerator Immobility(float pDuration)
    {
        MovementController.Immobile = true;

        yield return new WaitForSeconds(pDuration);

        MovementController.Immobile = false;
    }

    private void OnDestroy()
    {
        FootStepEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }
}