using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AiSharkWithNav : MonoBehaviour
{
    public enum BotDifficulty
    {
        Easy,
        Normal,
        Hard
    }
    public BotDifficulty Difficulty;
    public List<ObjToAvoid> ObjectsToAvoid = new List<ObjToAvoid>();
    public Vector3 GetMoveInput { get { return new Vector3(MoveInput.x, 0, MoveInput.z); } }
    public Vector3 MoveInput;
    public bool AttackInput;
    public List<Hookable> orderedHookables = new List<Hookable>();
    private CheckIfGrounded CheckGrounded;
    private bool mDashInput;
    public Usable MyThrownUsable;
    private float DiffChance;

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
    private UsableController UsableController;

    private Rigidbody Rigidbody;
    private Vector3 MoveTarget;
    float time = 0f;
    public enum state { moving, hookMove, attacking, dashing }
    public enum rayResult { hit, notHit, hitAbove }
    public state CurrentState = state.moving;
    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        MovementController = GetComponent<MovementController>();
        PlayerController = GetComponent<PlayerController>();
        Me = GetComponent<PlayerHookable>();
        CheckGrounded = GetComponent<CheckIfGrounded>();
        UsableController = GetComponent<UsableController>();
        //PlayerController.SetUpPlayer(-1, -1, Color.yellow, null);
        switch (GameManager.Instance.BotDifficulty)
        {
            case 0:
                SetBotDifficulty(BotDifficulty.Hard);
                break;
            case 1:
                SetBotDifficulty(BotDifficulty.Normal);
                break;
            case 2:
                SetBotDifficulty(BotDifficulty.Easy);
                break;
        }
        FindNewMoveTarget(transform.forward);
        StartCoroutine(UpdateOthers());
        StartCoroutine(LateStart());
        StartCoroutine(FindVel());
        StartCoroutine(NavMeshSafetyCheck());
        StartCoroutine(AvoidIncomingHooks());
        StartCoroutine(ResetObjectsToAvoid());
        
    }

    public void SetBotDifficulty(BotDifficulty pValue)
    {
        Difficulty = pValue;
        switch (pValue)
        {
            case BotDifficulty.Easy:
                DiffChance = 10f;
                break;
            case BotDifficulty.Normal:
                DiffChance = 50f;
                break;
            case BotDifficulty.Hard:
                DiffChance = 100f;
                break;
            default:
                break;
        }
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForSeconds(3f);

        PlayerController.ChangeState(PlayerController.State.Idle);
        ChangeState(state.moving);
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
        //Debug.DrawRay(pPos + (Vector3.up * 2f), Vector3.down * 5f, Color.red, 1f);

        if (Physics.Raycast(pPos + (Vector3.up * 2f), Vector3.down, out hit, 5f, 1<<10/*ground layer*/, QueryTriggerInteraction.Ignore))
        {
            DidHit = true;
            //hitPos = hit.point;
            hitPos = FindPointOnNavMesh(hit.point);
            //if (hit.distance < 1f || hitPos.y > transform.position.y)
            //{
            //    return rayResult.hitAbove;
            //}
            if (CheckGrounded.OnGround && (hitPos.y - (transform.position.y - 1)) > 0.3f)
            {
                //Debug.Log("TestGround.HitAbove; Height diff: " + (hitPos.y - (transform.position.y - 1)));
                return rayResult.hitAbove;
            }
        }
        //hitPos = hit.point;
        hitPos = FindPointOnNavMesh(hit.point);
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
        orderedHookables = Hookables.OrderBy(a => (a.transform.position - transform.position).sqrMagnitude).ToList();

        if (UsableController.CurrentUsable != UsableController.StandardHook)
        {
            orderedHookables.RemoveAll(a => !(a is PlayerHookable));
        }

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
                var rightResult = TestGround(transform.position + transform.right * sideDist, out Vector3 f);
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
                //return StartMoveByHook(hitPointOnGround);
                return hitPointOnGround;
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
        int i = Random.Range(0, 2);
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

    private bool CheckIfOnNavMeshGround(Vector3 pPos)
    {
        pPos = new Vector3(pPos.x, 0f, pPos.z);
        NavMeshHit hit;
        return NavMesh.SamplePosition(pPos, out hit, 2f, NavMesh.AllAreas);
    }

    private void DoMoving()
    {
        //something is in front of us, move left or right
        if (Physics.Raycast(transform.position, transform.forward, 1f))
        {
            FindNewMoveTarget(transform.right * RndSign());
            //FindNewMoveTarget(findSafeDirection(transform.forward * 6));
        }
        Vector3 dirToMoveTarget = new Vector3(MoveTarget.x, 0, MoveTarget.z) - new Vector3(transform.position.x, 0, transform.position.z);
        float length = dirToMoveTarget.magnitude;
        MoveInput = Vector3.Lerp(MoveInput, dirToMoveTarget.normalized, Time.deltaTime * inputChangeSpeed);
        //time += Time.deltaTime;

        //Debug.DrawRay(MoveTarget + Vector3.up * 2, Vector3.down * 2, Color.magenta);
        if (CheckGrounded.OnGround && (MoveTarget.y - (transform.position.y - 1)) > 0.3f)
        {
            //Debug.Log("DoMoving.HitAbove; Height diff: " + (MoveTarget.y - (transform.position.y - 1)));
            ChangeState(state.hookMove);
        }

        //close to the target? find new target
        if (length < .4f)
        {
            bool b = Random.Range(5, 10) + time > 11;
            /*b = false; */
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
        else if (length > 15f)
        {
            FindNewMoveTarget(transform.forward);
        }

        // Sometimes after being in the catapult on the castle level, MoveInput gets messed up
        if (float.IsNaN(MoveInput.x) || float.IsNaN(MoveInput.y) || float.IsNaN(MoveInput.z))
        {
            MoveInput = Vector3.zero;
            ChangeState(state.moving);
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
            var rand = Random.Range(0, 99);
            if (rand > DiffChance)
            {
                MoveInput = ((AttackTarget.transform.position + targetVelocity) - transform.position) + Random.insideUnitSphere * 3;

            }
            else
            {
                MoveInput = ((AttackTarget.transform.position + targetVelocity) - transform.position) /*+ Random.insideUnitSphere * 3*/;
            }
            //Debug.DrawRay(MoveInput + Vector3.up * 3, Vector3.down, Color.blue);
            Vector3 v;
            var result = TestGround(MoveInput.normalized, out v);
            if (result == rayResult.notHit)
            {
                Dash(transform.forward * -1);
            }
            MoveInput.y = 0;
            float dist = MoveInput.magnitude;
            //time += Time.deltaTime;
            float ThrowDist = time * 15f;
            bool CorrectDirection = Vector3.Dot(transform.forward, MoveInput.normalized) > .95f;

            // If holding powerup, will rather throw it at a longer range than dash to cancel like normal hook if out of range
            if (UsableController.CurrentUsable != UsableController.StandardHook)
            {
                dist = MoveInput.magnitude * 1.5f;
                //Debug.DrawRay(transform.position, transform.forward * dist, Color.magenta);
                float rng = Random.Range(0.5f, 1.5f);
                if (time >= rng && CorrectDirection)
                {
                    SetOwnUsableThrown();
                    AttackInput = false;
                    ChangeState(state.moving);
                }
            }

            if (time >= 1.2f)
            {
                Dash(transform.forward, false);
            }
            else if (ThrowDist >= dist && CorrectDirection)
            {
                AttackInput = false;
                ChangeState(state.moving);
            }
            else if (ThrowDist >= dist + 1f)
            {
                Dash(transform.forward, false);
            }

        }
        else
        {
            FindNewAttackTarget();
        }
    }

    private Vector3 findSafeDirection(Vector3 startDir)
    {
        float dir = RndSign();
        Vector3 pos;
        for (int i = 0; i < 4; i++)
        {
            var result = TestGround(Quaternion.AngleAxis(45 * (i * dir), Vector3.up) * startDir + transform.position, out pos);
            //Debug.DrawRay(pos + (Vector3.up * 2f), Vector3.down * 5f, Color.red, 1f);
            if (result == rayResult.hit)
            {
                return Quaternion.AngleAxis(45 * (i * dir), Vector3.up) * startDir;
            }
            else if (result == rayResult.hitAbove)
            {
                return MoveInput = StartMoveByHook(pos);
                //return findSafeDirection(Quaternion.AngleAxis(45 * (i * dir), Vector3.up) * startDir);
            }
        }
        // If no safe direction, v3.zero is middle of the map
        return Vector3.zero;
    }

    /// <summary>
    /// Hook move is update for when bot is trying to move somewhere but has to use the hook to reach it
    /// </summary>
    private void DoHookMove()
    {
        MoveInput = MoveTarget - transform.position;
        MoveInput.y = 0;
        //float dist = MoveInput.magnitude * 1.3f; // Multiply the magnitude to increase the range (wasn't able to hook up to level above in temple)
        float dist = (new Vector3(MoveTarget.x, 0, MoveTarget.z) - new Vector3(transform.position.x, 0, transform.position.z)).magnitude;
        //time += Time.deltaTime;
        float ThrowDist = (time * 15f);

        //Debug.Log("Throwdist = " + ThrowDist + " dist = " + dist + " time = " + time);
        if (ThrowDist >= dist || dist <= 0.95f)
        {
            //Debug.Log("supposed to throw hook");
            AttackInput = false;
            ChangeState(state.moving);
        }

        // If he doesn't manage to hook himself out and has no attack input, he finds new attack target so he doesn't get stuck in HookMove
        if (AttackInput == false)
        {
            FindNewAttackTarget();
            ChangeState(state.attacking);
        }

        //Debug.DrawRay(MoveInput + Vector3.up * 5, Vector3.down * 5, Color.magenta, 1f);
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

    private void Dash(Vector3 pDirection, bool pIsSafeDir = false)
    {
        if (MovementController.IsDashing || MovementController.Immobile)
        {
            return;
        }

        if (pIsSafeDir)
        {
            MoveInput = pDirection;
        }
        else
        {
            MoveInput = findSafeDirection(pDirection * 8);
        }

        if (MoveInput == Vector3.zero)
        {
            MoveInput = Vector3.zero - transform.position;
        }
        Debug.DrawRay(transform.position + (Vector3.up * 2), MoveInput + (Vector3.up * 2), Color.green, 1f);
        //mDashInput = true;
        ChangeState(state.dashing);
    }

    public void ChangeState(state pState)
    {
        //time = 0;
        //if (UpdateStateCoroutine != null)
        //{
        //    StopCoroutine(UpdateStateCoroutine);
        //}
        CurrentState = pState;
        switch (pState)
        {
            case state.moving:
                //CurrentState = state.moving;
                AttackInput = false;
                FindNewMoveTarget(transform.forward);
                //UpdateStateCoroutine = StartCoroutine(DoMoving());
                break;
            case state.attacking:
                //CurrentState = state.attacking;
                FindNewAttackTarget();
                previousPos = AttackTarget ? AttackTarget.transform.position : previousPos;

                if (AttackTarget != null)
                {
                    var dist = (AttackTarget.transform.position - transform.position).sqrMagnitude;

                    if (!(AttackTarget is PowerUpHookable && dist <= 4f))
                    {
                        time = 0;
                        AttackInput = true;
                        StartCoroutine(ThrowTest());
                    }
                    else
                    {
                        ChangeState(state.moving);
                    }
                }
                //UpdateStateCoroutine = StartCoroutine(DoAttacking());
                break;
            case state.hookMove:
                //CurrentState = state.hookMove;
                time = 0;
                AttackInput = true;
                StartCoroutine(ThrowTest());
                //UpdateStateCoroutine = StartCoroutine(DoHookMove());
                break;
            case state.dashing:
                //CurrentState = state.dashing;
                mDashInput = true;
                AttackInput = false;
                //UpdateStateCoroutine = StartCoroutine(DoDashing());
                break;
            default:
                break;
        }
        //CurrentState = pState;
    }

    private IEnumerator NavMeshSafetyCheck()
    {
        while (true)
        {
            switch (CurrentState)
            {
                case state.moving:
                    if (!CheckIfOnNavMeshGround(transform.position))
                    {
                        var point = FindPointOnNavMesh(transform.position);
                        var dir = point - transform.position;
                        //Debug.DrawRay(transform.position + Vector3.up * 2, Vector3.down, Color.blue, 2f);
                        //Debug.DrawRay(point + Vector3.up * 2, Vector3.down, Color.blue, 2f);
                        //Debug.DrawRay(transform.position + Vector3.up * 2, dir, Color.blue, 2f);
                        Dash(dir, true);
                        FindNewMoveTarget(transform.forward);
                    }
                    break;
                case state.hookMove:
                    break;
                case state.attacking:
                    if (!CheckIfOnNavMeshGround(transform.position))
                    {
                        AttackInput = false;
                        var point = FindPointOnNavMesh(transform.position);
                        var dir = point - transform.position;
                        Debug.DrawRay(transform.position + Vector3.up * 2, Vector3.down, Color.blue, 2f);
                        Debug.DrawRay(point + Vector3.up * 2, Vector3.down, Color.blue, 2f);
                        Debug.DrawRay(transform.position + Vector3.up * 2, dir, Color.blue, 2f);
                        Dash(dir, true);
                        ChangeState(state.moving);
                    }
                    break;
                case state.dashing:
                    break;
                default:
                    break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    void Update()
    {
        //if the ai has been fucked up, reset(reset from ragdoll)
        //if (Rigidbody.isKinematic && fuckedUp)
        //{
        //    fuckedUp = false;
        //}
        //else if (!Rigidbody.isKinematic && !fuckedUp)
        //{
        //    fuckedUp = true;
        //    ChangeState(state.moving);
        //}
        time += Time.deltaTime;

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

    public void OnPowerupPickup()
    {
        FindNewAttackTarget();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root == transform)
        {
            return;
        }

        //if incoming hook is in front of us dash backwards if it is behind us dash forwards
        

        if (other.GetComponent<PlayerController>())
        {
            MoveTarget += -transform.forward;
        }
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    var usable = other.GetComponent<Usable>();
    //    if (usable != null)
    //    {
    //        ObjectsToAvoid = ObjectsToAvoid.Where(a => !a.HasBeenChecked).ToList();
    //    }
    //}

    private IEnumerator AvoidIncomingHooks()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            //Collider[] cols = Physics.OverlapSphere(transform.position, 4f);

            //for (int i = 0; i < cols.Length; i++)
            //{

            //    var usable = cols[i].GetComponent<Usable>();
            //    if (usable != null && usable != UsableController.CurrentUsable && usable.IsActive && usable != MyThrownUsable)
            //    { 
            //        ObjToAvoid add = new ObjToAvoid(usable, false);

            //    }
            //}

            if (ObjectsToAvoid.Any())
            {
                foreach (var item in ObjectsToAvoid)
                {
                    if (!item.HasBeenChecked)
                    {

                        //var rng = Random.Range(0, 100);
                        //if (rng > DiffChance)
                        //{
                        //    item.HasBeenChecked = true;
                        //    break;
                        //}
                        //var dir = transform.position - item.usable.transform.position;
                        //var left = Vector3.Cross(dir, Vector3.up).normalized;
                        //// Needs to find safe direction to dash away from hooks/powerups
                        //Vector3 dirtodash;
                        Vector3 projectionPoint = Vector3.zero;

                        if (item.usable == UsableController.StandardHook)
                        {
                            projectionPoint = Vector3.Project((transform.position - item.usable.transform.position), -item.usable.transform.up);
                        }
                        else
                        {
                            projectionPoint = Vector3.Project((transform.position - item.usable.transform.position), item.usable.transform.forward);

                        }
                        var dirToDash = (transform.position - projectionPoint).normalized;

                        //if (TestGround(left, out dirtodash) == rayResult.hit)
                        //{
                        //    Dash(dirtodash);
                        //}
                        //else if (TestGround(-left, out dirtodash) == rayResult.hit)
                        //{
                        //    Dash(dirtodash);
                        //}
                        //else
                        //{
                        //    Dash(findSafeDirection(transform.forward));
                        //}
                        Dash(dirToDash);
                        item.HasBeenChecked = true;
                    }
                }
            }
        }
        
        //StartCoroutine(AvoidIncomingHooks());
    }

    private IEnumerator ResetObjectsToAvoid()
    {
        yield return new WaitForSeconds(2f);
        ObjectsToAvoid = ObjectsToAvoid.Where(a => !a.HasBeenChecked).ToList();
    }

    private void SetOwnUsableThrown()
    {
        // Bot's dash to avoid incoming usables, this is to keep track at the one the shark just threw so he doesn't dash straight after throwing
        MyThrownUsable = UsableController.CurrentUsable;
        StartCoroutine(OwnUsableAvoidanceReset());
    }

    private IEnumerator OwnUsableAvoidanceReset()
    {
        yield return new WaitForSeconds(0.5f);
        MyThrownUsable = null;
    }

    private void OnDrawGizmos()
    {
        //for (int i = 0; i < Nav.path.corners.Count() - 1; i++)
        //{
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawLine(Nav.path.corners[i], Nav.path.corners[i + 1]);
        //}
        
        //Gizmos.DrawWireSphere(transform.position, 4f);
    }
}

//public class ObjToAvoid
//{
//    public Usable usable;
//    public bool HasBeenChecked;

//    public ObjToAvoid(Usable pUsable, bool pValue)
//    {
//        usable = pUsable;
//        HasBeenChecked = pValue;
//    }
//}


