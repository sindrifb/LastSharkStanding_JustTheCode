using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AiShark : MonoBehaviour
{
    public Vector3 GetMoveInput { get { return new Vector3(MoveInput.x, 0, MoveInput.z); } }
    private Vector3 MoveInput;
    public bool AttackInput;
    public List<Hookable> TargetPriority = new List<Hookable>();
    public bool InHazardDanger;
    
    private bool mDashInput;
    public bool DashInput
    {
        get
        {
            bool temp = mDashInput;
            mDashInput = false;
            return temp;
        }
    }

    private bool fuckedUp;
    //possible targets
    private List<Hookable> Hookables = new List<Hookable>();
    public Hookable AttackTarget;
    private PlayerHookable Me;

    private MovementController MovementController;
    private PlayerController PlayerController;
    
    private Rigidbody Rigidbody;
    private Vector3 MoveTarget;
    float time = 0f;
    public enum state{ moving, hookMove, attacking, dashing}
    public enum rayResult { hit, notHit, hitAbove }
    public state CurrentState = state.moving;
    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        MovementController = GetComponent<MovementController>();
        PlayerController = GetComponent<PlayerController>();
        Me = GetComponent<PlayerHookable>();
        //PlayerController.SetUpPlayer(-1, -1, Color.yellow, null);
        FindNewMoveTarget(transform.forward);
        StartCoroutine(UpdateOthers());
        StartCoroutine(LateStart());
        StartCoroutine(FindVel());
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForSeconds(3f);
        PlayerController.ChangeState(PlayerController.State.Idle);
    }

    private IEnumerator UpdateOthers()
    {
        Hookables = FindObjectsOfType<Hookable>().ToList();
        Hookables.RemoveAll(a => (a.transform.position - transform.position).sqrMagnitude < 1f * 1f);
        yield return new WaitForSeconds(1f);
        StartCoroutine(UpdateOthers());
        //CheckIfStuck();
    }
    //private Vector3 OldPos = Vector3.positiveInfinity;

    //private void CheckIfStuck()
    //{
    //    if ((OldPos - transform.position).sqrMagnitude < .2f * .2f && CurrentState == state.moving)
    //    {
    //        MoveTarget += -transform.forward;
    //    }
    //    OldPos = transform.position;
    //}

    private rayResult TestGround(Vector3 pPos, out Vector3 hitPos)
    {
        RaycastHit hit;
        bool DidHit = false;
        Debug.DrawRay(pPos + (Vector3.up * 2f), Vector3.down * 5f, Color.red, 1f);
        if (Physics.Raycast(pPos + (Vector3.up * 2f), Vector3.down, out hit, 5f, GetComponent<CheckIfGrounded>().LayerMask, QueryTriggerInteraction.Ignore))
        {
            DidHit = true;
            
            hitPos = hit.point;
            if (hit.distance < 1f || hit.point.y > transform.position.y)
            {
                return rayResult.hitAbove;
            }
        }
        hitPos = hit.point;
        return DidHit ? rayResult.hit : rayResult.notHit;
    }

    private Vector3 StartMoveByHook(Vector3 pPosition)
    {
        ChangeState(state.hookMove);
        return MoveTarget = pPosition;
    }

    private Hookable FindNewAttackTarget()
    {
        Hookables = FindObjectsOfType<Hookable>().Where(a => a != Me).ToList();
        Hookables.RemoveAll(a => (a.transform.position - transform.position).sqrMagnitude < 1f * 1f || !a.IsAvailable);
        var orderedHookables = Hookables.OrderBy(a => (a.transform.position - transform.position).sqrMagnitude).ToList();

        if (orderedHookables.Count >= 3)
        {
            return AttackTarget = orderedHookables[Random.Range(0, 3)];
        }
        else
        {
            return AttackTarget = orderedHookables.FirstOrDefault();
        }
    }

    private Vector3 FindNewMoveTarget(Vector3 pDirection)
    {
        float forwardDist = 4f;
        float sideDist = 2f;
        Vector3 hitPointOnGround;
        Vector3 closeHitPoint;
        var result = TestGround(transform.position + pDirection * 4f, out hitPointOnGround);
        var closeResult = TestGround(transform.position + pDirection * 1f, out closeHitPoint);
        if (closeResult != rayResult.hit)
        {
            result = closeResult;
            hitPointOnGround = closeHitPoint;
            sideDist = 1f;
            forwardDist = 1f;
        }
        switch (result)
        {
            case rayResult.hit:
                inputChangeSpeed = 6;
                //float amp = Random.Range(1, 4);
                var rightResult = TestGround(transform.position + transform.right * sideDist ,out Vector3 f);
                var leftResult = TestGround(transform.position + (-transform.right) * sideDist, out f);
                int turnOffset = leftResult == rayResult.hit && rightResult == rayResult.hit ? RndSign() : -1;

                if (rightResult == rayResult.hit && leftResult != rayResult.hit)
                {
                    turnOffset = 1;
                }
                else if (leftResult == rayResult.hit && rightResult != rayResult.hit)
                {
                    turnOffset = -1;
                }

                return MoveTarget = FindPointOnNavMesh(transform.position + (transform.forward * forwardDist) + ((sideDist * transform.right) * turnOffset));
            case rayResult.notHit:
                inputChangeSpeed = 1000f;
                //no ground in checked direction check 45 degrees clockwise
                return MoveTarget = transform.position + findSafeDirection(transform.forward * forwardDist);
            case rayResult.hitAbove:
                //we are probably sinking, might wanna hook ourself out of this one
                return StartMoveByHook(hitPointOnGround);
            default:
                return Vector3.zero;
        }
    }

    //finds closest point on navmesh
    private Vector3 FindPointOnNavMesh(Vector3 pPoint)
    {
        NavMeshHit hit;
        NavMesh.SamplePosition(pPoint, out hit, 10, NavMesh.AllAreas);
        return hit.position;
    }

    int RndSign()
    {
        int i = Random.Range(0,2);
        if (i == 0)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }

    float inputChangeSpeed = 6;

    private void DoMoving()
    {
        //something is in front of us, move left or right
        if (Physics.Raycast(transform.position,transform.forward,1f))
        {
            FindNewMoveTarget(transform.right * RndSign());
        }

        Vector3 dirToMoveTarget = new Vector3(MoveTarget.x, 0, MoveTarget.z) - new Vector3(transform.position.x, 0, transform.position.z);
        float lenght = dirToMoveTarget.magnitude;
        //MoveInput = Vector3.MoveTowards(MoveInput, dirToMoveTarget.normalized, Time.deltaTime);
        MoveInput = Vector3.Lerp(MoveInput, dirToMoveTarget.normalized, Time.deltaTime * inputChangeSpeed);
        time += Time.deltaTime;
        //close to the target? find new target
        if (lenght < .4f)
        {
            bool b = Random.Range(5, 10) + time > 11;
            /*b = false; *///AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
            if (!b || !Hookables.Any())
            {
                FindNewMoveTarget(transform.forward);
            }
            else
            {
                ChangeState(state.attacking);
            }
        }
        //to far away? find new place to go
        else if (lenght > 15f)
        {
            FindNewMoveTarget(transform.forward);
        }
    }

    Vector3 previousPos = Vector3.zero;
    Vector3 targetVelocity = Vector3.zero;
    IEnumerator FindVel()
    {
        yield return new WaitForSeconds(.1f);
        if (AttackTarget != null)
        {
            targetVelocity = (AttackTarget.transform.position - previousPos);
            previousPos = AttackTarget.transform.position;
        }
        else
        {
            targetVelocity = Vector3.zero;
        }
        StartCoroutine(FindVel());
    }


    private void DoAttacking()
    {
        if (AttackTarget != null)
        {
           
            MoveInput = (AttackTarget.transform.position + targetVelocity) - transform.position;
            Vector3 v;
            var result = TestGround(MoveInput.normalized, out v);
            if (result == rayResult.notHit)
            {
                Dash(transform.forward * -1);
            }
            MoveInput.y = 0;
            float dist = MoveInput.magnitude;
            time += Time.deltaTime;
            float ThrowDist = time * 15f;
            bool CorrectDirection = Vector3.Dot(transform.forward, MoveInput.normalized) > .95f;

            if (time >= 1.2f)
            {
                Dash(transform.forward);
            }
            else if (ThrowDist >= dist && CorrectDirection)
            {
                AttackInput = false;
                ChangeState(state.moving);
            }
            else if (ThrowDist >= dist + 1f)
            {
                Dash(transform.forward);
            }
        }
    }

    private Vector3 findSafeDirection(Vector3 startDir)
    {
        float dir = RndSign();
        Vector3 pos;
        for (int i = 0; i < 4; i++)
        {
            var result = TestGround(Quaternion.AngleAxis(45 * (i * dir), Vector3.up) * startDir + transform.position, out pos);
            if (result == rayResult.hit)
            {
                return Quaternion.AngleAxis(45 * (i * dir), Vector3.up) * startDir;
            }
            else if (result == rayResult.hitAbove)
            {
                return MoveInput = StartMoveByHook(pos);
            }
        }

        return Vector3.zero;
    }
    
    private void DoHookMove()
    {
        
        MoveInput = MoveTarget - transform.position;
        MoveInput.y = 0;
        float dist = MoveInput.magnitude;
        time += Time.deltaTime;
        float ThrowDist = time * 15f;

        if (ThrowDist >= dist)
        {
            AttackInput = false;
            ChangeState(state.moving);
        }
    }

    private void DoDashing()
    {
        if (!mDashInput)
        {
            ChangeState(state.moving);
        }
    }

    IEnumerator ThrowTest()
    {
        yield return new WaitForSeconds(1.5f);
        if (!AttackInput)
        {
            yield break;
        }
        AttackInput = false;
        ChangeState(state.moving);
    }

    private void Dash(Vector3 pDirection)
    {
        ChangeState(state.dashing);
        MoveInput = findSafeDirection(pDirection * 6);
        mDashInput = true;
    }

    public void ChangeState(state pState)
    {
        time = 0;
        switch (pState)
        {
            case state.moving:
                CurrentState = state.moving;
                FindNewMoveTarget(transform.forward);
                AttackInput = false;
                break;
            case state.attacking:
                CurrentState = state.attacking;
                FindNewAttackTarget();
                previousPos = AttackTarget? AttackTarget.transform.position : previousPos;
                AttackInput = true;
                StartCoroutine(ThrowTest());
                break;
            case state.hookMove:
                CurrentState = state.hookMove;
                AttackInput = true;
                StartCoroutine(ThrowTest());
                break;
            case state.dashing:
                CurrentState = state.dashing;
                AttackInput = false;
                break;
            default:
                break;
        }
    }

    void Update()
    {
        //if the ai has been fucked up, reset(reset from ragdoll)
        if (Rigidbody.isKinematic && fuckedUp)
        {
            fuckedUp = false;
        }
        else if (!Rigidbody.isKinematic && !fuckedUp)
        {
            fuckedUp = true;
            ChangeState(state.moving);
        }

        switch (CurrentState)
        {
            case state.moving:
                DoMoving();
                break;
            case state.attacking:
                DoAttacking();
                break;
            case state.hookMove:
                DoHookMove();
                break;
            case state.dashing:
                DoDashing();
                break;
            default:
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root == transform)
        {
            return;
        }

        //if incoming hook is in front of us dash backwards if it is behind us dash forwards
        if (other.GetComponent<Hook>())
        {
            if (RndSign() < 0)
            {
                if (Vector3.Dot(transform.forward, (other.transform.position - transform.position).normalized) < 0)
                {
                    Dash(transform.forward);
                }
                else
                {
                    Dash(transform.forward * -1);
                }
            }
        }

        if (other.GetComponent<PlayerController>())
        {
            MoveTarget += -transform.forward;
        }
    }
}
