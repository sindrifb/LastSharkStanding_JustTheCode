using System;
using System.Collections;
using UnityEngine;
using Rewired;
using System.Collections.Generic;

public class MovementController : MonoBehaviour
{
    public AudioClip DashSound;
    public GameObject DashParticles;
    public GameObject MovementParticle;
    private Animator Animator;
    private AudioSource AudioSource;
    private ParticleSystem.EmissionModule EmissionModule;

    public float YOffset = 1f;

    private float CurrentMoveSpeed;
    public float StandardMoveSpeed = 16;
    public float WindupMoveSpeed = 1.6f;
    public float StaggeredMoveSpeed = 1.6f;

    private float CurrentRotSpeed;
    public float StandardRotSpeed = 2000; //16 with slerp 2000 with rotTowards
    public float WindupStartRotSpeed = 120; //3.2 with slerp 120 with rotTowards
    public float StaggeredRotSpeed = 120; //1.6 with slerp 120 with rotTowards

    public float DashSpeedMultiplier = 9f;
    public float DashDuration = .2f;
    public float DashPushForceMultiplier = 30;

    public float StaggerDuration = .3f;

    private float Timer;
    private float MinRotMagnitude = 0.001f;
    private float MinMoveMagnitude = 0.2f;

    private int PlayerIndex { get { return PlayerController?.PlayerID ?? -1; }  }
    private Player RewiredPlayer;

    public bool IsStaggered { get; private set; }
    public bool IsDashing { get; private set; }

    private CheckIfGrounded OnMap;
    private bool OnFloor = false;
    private Rigidbody rb;
    private Coroutine DashCoroutine;

    public Vector3 Direction { get; private set; }

    private PlayerController PlayerController;
    private CharacterController CharacterController;
    private BotController AiShark;
    //players pushed during the ongoing dash
    private List<PlayerController> PushedPlayers = new List<PlayerController>();
    private Vector3 FallOffVelocity = new Vector3();
    // Immobility only happens after a player has been dash pushed
    public bool Immobile = false;
    public float ImmobilityDuration = 0.1f;

    public void Initialize(PlayerController pPlayerController)
    {
        PlayerController = pPlayerController;
        Animator = GetComponentInChildren<Animator>(true);
        AudioSource = GetComponent<AudioSource>();
        OnMap = GetComponent<CheckIfGrounded>();
        rb = GetComponent<Rigidbody>();
        if (PlayerController.RewiredID != 5)
        {
            RewiredPlayer = ReInput.players.GetPlayer(PlayerController.RewiredID);
        }
       
        CharacterController = GetComponent<CharacterController>();
        if (MovementParticle != null)
        {
            GameObject tempParticle = ((GameObject)Instantiate(MovementParticle, new Vector3(transform.position.x, transform.position.y - 0.8f, transform.position.z), transform.rotation, transform));
            EmissionModule = tempParticle.GetComponentInChildren<ParticleSystem>().emission;
            EmissionModule.enabled = false;
        }
        AiShark = GetComponent<BotController>();
    }

    public void UpdateNotGrounded()
    {
        if (OnFloor != OnMap.AboveGround)
        {
            OnFloor = OnMap.AboveGround;
            if (!OnFloor)
            {
                if (FallOffVelocity.magnitude < 15)
                {
                    if (FallOffVelocity == Vector3.zero)
                    {
                        FallOffVelocity = transform.forward;
                    }
                    FallOffVelocity = FallOffVelocity.normalized * 15;
                }

                PlayerController.Hookable.Push(FallOffVelocity);
            }
        }
    }

    public void UpdatePush()
    {
        if (CharacterController == null)
        {
            CharacterController = GetComponent<CharacterController>();
            return;
        }

        CharacterController.Move(PlayerController.Velocity * Time.deltaTime);
    }

    public void UpdateMovement()
    {
        if (CharacterController == null)
        {
            CharacterController = GetComponent<CharacterController>();
            return;
        }

       
        //Debug.Log(OnMap.onGround);

        Direction = RewiredPlayer != null ? new Vector3(RewiredPlayer.GetAxis("MoveHorizontal"), 0, RewiredPlayer.GetAxis("MoveVertical")) : AiShark.GetMoveInput;

        if (OnMap.AboveGround && Direction.magnitude >= MinRotMagnitude)
        {
            var targetRot = Quaternion.LookRotation(Direction.normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, CurrentRotSpeed * Time.deltaTime);
        }
        //Debug.Log("movment dir magnitude: " + Direction.magnitude);
        if (OnMap.AboveGround && Direction.magnitude >= MinMoveMagnitude)
        {
            
            if (!IsDashing && PlayerController.CurrentState != PlayerController.State.none && !Immobile)
            {
                CharacterController.Move(Vector3.ClampMagnitude(Direction, 1) * Time.deltaTime * (CurrentMoveSpeed));
            }

            //var targetRot = Quaternion.LookRotation(Direction.normalized);
            //transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, CurrentRotSpeed * Time.deltaTime);

            if (!EmissionModule.enabled)
            {
                EmissionModule.enabled = true;
            }
            if (!Animator.GetBool(Constants.AnimationParameters.Walk) && PlayerController.CurrentState != PlayerController.State.none)
            {
                //Animator.SetBool(Constants.AnimationParameters.Walk, true);
                PlayerController.PlayLowerBodyAnimation(Constants.AnimationParameters.Walk);
            }
        }
        else
        {
            EmissionModule.enabled = false;
            //if (Animator.GetBool(Constants.AnimationParameters.Walk))
            //{
            //    Animator.SetBool(Constants.AnimationParameters.Walk, false);
            //}
            PlayerController.PlayLowerBodyAnimation(Constants.AnimationParameters.Standing);
        }

        FallOffVelocity = CharacterController.velocity;
    }

    public void Dash()
    {
        if (!IsDashing && !Immobile)
        {
            PlayerController.UsableController.CurrentUsable.Interrupt(PlayerController.State.WindUp);
            PushedPlayers.Clear();
            SetIsDashing(true);
            CharacterController charController = GetComponent<CharacterController>();

            DashCoroutine = StartCoroutine(DashTo(charController));
        }
    }

    public void InterruptDash()
    {
        StopCoroutine(DashCoroutine);
        SetIsDashing(false);
    }

    private IEnumerator DashTo(CharacterController pCharController)
    {
        PlayerController.OnDashing();

        var dir = Direction.normalized;
        if (dir.magnitude < 0.1)
        {
            dir = transform.forward;
        }
        transform.rotation = Quaternion.LookRotation(dir);

        GameObject particlesystem = null;
        if (DashSound != null)
        {
            AudioSource?.PlayOneShot(DashSound);
        }
        if (DashParticles != null)
        {
            particlesystem = (GameObject)Instantiate(DashParticles, transform.position, transform.rotation, transform);
            Destroy(particlesystem, 6f);
        }
        float dashTimer = 0;

        while (dashTimer < DashDuration)
        {
            float currentSpeedMultiplier = -Mathf.Pow((dashTimer / DashDuration) * (1 - (-1)) + (-1), 4) + 1; // -(ratio * (max+min) + min) + 1 // y = x^4 + 1
            pCharController.Move(dir * Time.deltaTime * (StandardMoveSpeed + (currentSpeedMultiplier * DashSpeedMultiplier)));
            dashTimer += Time.deltaTime;
            yield return null;
        }

        SetIsDashing(false);
        //PlayerController.OnDashingDone();
        //StaggerMovement();

        //yield return new WaitForSeconds(0.3f);

        //StandardMovement();
        //IsDashing = false;
    }

    public void InitializeStagger()
    {
        CurrentMoveSpeed = StaggeredMoveSpeed;
        CurrentRotSpeed = StaggeredRotSpeed;
        MinMoveMagnitude = 0.2f;

        StartCoroutine(Stagger());
    }

    public void StopParticles()
    {
        EmissionModule.enabled = false;
    }

    private IEnumerator Stagger()
    {
        yield return new WaitForSeconds(DashDuration + StaggerDuration);

        PlayerController.OnStaggerDone();
    }

    private void SetIsDashing(bool pDash)
    {
        IsDashing = pDash;

        if (pDash)
        {
            //GetComponent<PlayerController>().PlayAnimation(Constants.AnimationParameters.Dash);
            PlayerController.PlayLowerBodyAnimation(Constants.AnimationParameters.Dash);
        }
        //else
        //{
        //    if (OnMap.onGround && Direction.magnitude >= MinMoveMagnitude)
        //    {
        //        PlayerController.PlayLowerBodyAnimation(Constants.AnimationParameters.Walk);
        //    }
        //    else
        //    {
        //        PlayerController.PlayLowerBodyAnimation(Constants.AnimationParameters.Standing);
        //    }
        //}

        
    }

    public void ResetRotation()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        
        var rotation = Quaternion.LookRotation(new Vector3(transform.forward.x,0,transform.forward.z));
        transform.rotation = rotation;
    }

    public void InitializeWindupMovement()
    {
        var dir = Direction;
        if (dir.magnitude < 0.1)
        {
            dir = transform.forward;
        }
        transform.rotation = Quaternion.LookRotation(dir);
        CurrentMoveSpeed = WindupMoveSpeed;
        CurrentRotSpeed = WindupStartRotSpeed;
        MinMoveMagnitude = 1f;
    }

    public void InitializeStandardMovement()
    {
        CurrentMoveSpeed = StandardMoveSpeed;
        CurrentRotSpeed = StandardRotSpeed;
        MinMoveMagnitude = 0.2f;
    }

    public void SetRotSpeedBasedOnDistance(float pDist)
    {
        CurrentRotSpeed = WindupStartRotSpeed / pDist;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (IsDashing)
        {
            var other = collision.gameObject.GetComponentInParent<PlayerController>();
            if (other != null && !PushedPlayers.Contains(other))
            {
                InterruptDash();

                PushedPlayers.Add(other);
                StartCoroutine(DashPush(other));
            }
        }
    }

    private IEnumerator DashPush(PlayerController pPlayerToPush)
    {
        var temp = (transform.forward + (pPlayerToPush.transform.position - transform.position));
        var pushDir = new Vector3(temp.x, 0, temp.z).normalized;
        pPlayerToPush.MakeImmobile(ImmobilityDuration);
        pPlayerToPush.AddForce(pushDir, DashPushForceMultiplier);

        yield return new WaitForSeconds(ImmobilityDuration);
        
        //pPlayerToPush.ConstantPushforce -= pushForce;
    }
}