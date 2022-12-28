using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

public class BotController : MonoBehaviour
{
    public enum BotDifficulty
    {
        Easy,
        Normal,
        Hard
    }
    public BotDifficulty Difficulty;
    private float DiffChance;
    [SerializeField]
    public List<ObjToAvoid> ObjectsToAvoid = new List<ObjToAvoid>();
    public Vector3 GetMoveInput { get { return new Vector3(MoveInput.x, 0, MoveInput.z); } }
    [SerializeField]
    private Vector3 MoveInput = new Vector3();
    public bool AttackInput { get; private set; }
    private CheckIfGrounded CheckGrounded;
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

    public Usable MyThrownUsable { get; private set; }

    public Hookable AttackTarget { get; private set; }
    private PlayerHookable Me;

    private MovementController MovementController;
    private PlayerController PlayerController;
    private UsableController UsableController;

    private Rigidbody Rigidbody;
    [SerializeField]
    private Vector3 MoveTarget;

    private float InputBuffer = 0.5f;
    private float TimeSinceAttackInputChange = 0.6f;
    private float MinAttackCoolDown;
    private float MaxAttackCoolDown;
    private float MinAvoidanceBuffer;
    private float MaxAvoidanceBuffer;
    private float ObstacleRayLength = 2f;
    private float WindupTimer = 0;
    private float WindupMaxTime;
    public List<Hookable> closeHookables;
    private float InputChangeSpeed = 6;
    public bool debugTargetAbove;

    private float MoveTargetMinDist = 7;
    private float MoveTargetMaxDist = 15;
    private Vector3 AttackTargetVelocity;
    private Vector3 AttackTargetPrevPos = Vector3.zero;

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        MovementController = GetComponent<MovementController>();
        PlayerController = GetComponent<PlayerController>();
        Me = GetComponent<PlayerHookable>();
        CheckGrounded = GetComponent<CheckIfGrounded>();
        UsableController = GetComponent<UsableController>();
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
        SetNewMoveTarget();
        StartCoroutine(CheckForAttackTargets());
        StartCoroutine(NavMeshSafetyCheck());
        StartCoroutine(ResetObjectsToAvoid());
        StartCoroutine(UpdateAttackTargetVelocity());
    }

    void Update()
    {
        TimeSinceAttackInputChange += Time.deltaTime;
        switch (PlayerController.CurrentState)
        {
            case PlayerController.State.Idle:
                UpdateMovement();
                UpdateObjectAvoidance();
                if (TargetIsAboveOrObstructed(MoveTarget) || AttackTarget != null)
                {
                    if (AttackTarget != null && AttackTarget is PowerUpHookable)
                    {
                        Vector3 targetPos = AttackTarget.transform.position;
                        float sqrDist = (targetPos - transform.position).sqrMagnitude;
                        if (sqrDist < 5 * 5 && !TargetIsAboveOrObstructed(targetPos))
                        {
                            MoveTarget = targetPos;
                            AttackTarget = null;
                        }
                        else
                        {
                            SetAttackInput(true);
                        }
                    }
                    else
                    {
                        SetAttackInput(true);
                    }
                }
                break;
            case PlayerController.State.WindUp:
                UpdateMovement();
                UpdateWindUp();
                UpdateObjectAvoidance();
                break;
            case PlayerController.State.Staggered:
                UpdateMovement();
                break;
            default:
                break;
        }
    }

    private void UpdateMovement()
    {
        Vector3 temp = new Vector3(MoveTarget.x, transform.position.y, MoveTarget.z);
        Vector3 dir = temp - transform.position;
        //Vector3 dir = new Vector3(temp.x, transform.position.y, temp.z);
        if (TargetIsAboveOrObstructed(MoveTarget) || AttackTarget != null)
        {
            if (!IsSafeDirection(dir, ObstacleRayLength))
            {
                //MoveInput = GetSafeDir(dir);
                //can rotate but not move
                Vector3 clamp = Vector3.ClampMagnitude(dir, 0.95f);
                dir = clamp;
            }
        }
        else
        {
            if (!IsSafeDirection(dir, ObstacleRayLength))
            {
                SetNewMoveTarget();
            }
            else if (dir.sqrMagnitude <= 1 * 1)
            {
                SetNewMoveTarget();
            }
            //dir.Normalize(); 
        }

        if (!IsSafeDirection(transform.forward, ObstacleRayLength))
        {
            InputChangeSpeed = 100000;
            Vector3 clamp = Vector3.ClampMagnitude(dir, 0.18f);
            dir = clamp;
        }
        else
        {
            InputChangeSpeed = 6;
        }

        MoveInput = Vector3.Lerp(MoveInput, dir, Time.deltaTime * InputChangeSpeed);
    }

    private void UpdateObjectAvoidance()
    {
        if (ObjectsToAvoid.Any())
        {
            foreach (var item in ObjectsToAvoid)
            {
                if (!item.HasBeenChecked)
                {
                    var rng =  Random.Range(0, 100);
                    if (rng > DiffChance)
                    {
                        item.HasBeenChecked = true;
                        break;
                    }

                    Vector3 projectionPoint = Vector3.zero;

                    if (item.usable == UsableController.StandardHook && item.usable != null)
                    {
                        projectionPoint = Vector3.Project((transform.position - item.usable.transform.position), -item.usable.transform.up);
                    }
                    else
                    {
                        if (item.usable != null)
                        {
                            projectionPoint = Vector3.Project((transform.position - item.usable.transform.position), item.usable.transform.forward);
                        }
                    }
                    Vector3 temp = new Vector3(projectionPoint.x, transform.position.y, projectionPoint.z);
                    var dirToDash = (transform.position - temp).normalized;

                    item.HasBeenChecked = true;
                    Dash(GetSafeDir(dirToDash));
                }
            }
        }
    }

    private void UpdateWindUp()
    {
        WindupTimer += Time.deltaTime;

        if (AttackTarget != null)
        {
            Vector3 predictedPos = AttackTarget.transform.position + AttackTargetVelocity;
           

            MoveTarget = predictedPos;

            var dir = predictedPos - transform.position;
            if (Random.Range(0, 100) > DiffChance)
            {
                var insideCircle = Random.insideUnitCircle;
                dir += new Vector3(insideCircle.x, 0, insideCircle.y) * 3;
            }
            bool correctDirection = Vector3.Dot(transform.forward, dir.normalized) > .95f;
            float dist = new Vector3(dir.x, 0, dir.z).magnitude;

            if (WindupTimer >= WindupMaxTime)
            {
                if (Random.Range(0, 100) >= DiffChance)
                {
                    SetAttackInput(false);
                }
                else
                {
                    Dash(GetSafeDir(transform.forward));
                }
            }
            else if (UsableController.CurrentUsable.AimModule.ThrowDistance >= dist + 2 && UsableController.CurrentUsable == UsableController.StandardHook)
            {
                Dash(GetSafeDir(transform.forward));
            }
            else if (UsableController.CurrentUsable.AimModule.ThrowDistance >= dist && correctDirection)
            {
                SetAttackInput(false);
            }
        }
        else
        {
            if (TargetIsAboveOrObstructed(MoveTarget))
            {
                var dir = MoveTarget - transform.position;
                dir.y = transform.position.y;
                bool correctDirection = Vector3.Dot(transform.forward, dir) > .95f;
                float dist = new Vector3(dir.x, 0, dir.z).magnitude;

                if (UsableController.CurrentUsable.AimModule.ThrowDistance >= dist && correctDirection)
                {
                    SetAttackInput(false);
                }
                else if (WindupTimer >= WindupMaxTime)
                {
                    SetAttackInput(false);
                }
            }
            else
            {
                Dash(GetSafeDir(transform.forward));
            }
        }
    }

    private void SetBotDifficulty(BotDifficulty pValue)
    {
        Difficulty = pValue;
        switch (pValue)
        {
            case BotDifficulty.Easy:
                DiffChance = 10f;
                MinAttackCoolDown = 3;
                MaxAttackCoolDown = 6;
                MinAvoidanceBuffer = 0.5f;
                MaxAvoidanceBuffer = 1f;
                MovementController.StandardMoveSpeed = 5;
                MovementController.StandardRotSpeed = 10;
                break;
            case BotDifficulty.Normal:
                DiffChance = 50f;
                MinAttackCoolDown = 2;
                MaxAttackCoolDown = 5;
                MinAvoidanceBuffer = 0.3f;
                MaxAvoidanceBuffer = 0.6f;
                MovementController.StandardMoveSpeed = 7.5f;
                MovementController.StandardRotSpeed = 15;
                break;
            case BotDifficulty.Hard:
                DiffChance = 90f;
                MinAttackCoolDown = 1.5f;
                MaxAttackCoolDown = 4;
                MinAvoidanceBuffer = 0f;
                MaxAvoidanceBuffer = 0.3f;
                MovementController.StandardMoveSpeed = 10;
                MovementController.StandardRotSpeed = 20;
                break;
            default:
                break;
        }
    }

    private void SetNewMoveTarget()
    {
        bool bla = Random.Range(0, 100) < 70;
        List<Hookable> desiredTargets = new List<Hookable>();

        if (bla)
        {
            desiredTargets = FindObjectsOfType<Hookable>().Where(a => a is PlayerHookable && a != Me || a is PowerUpHookable).ToList();
        }
        

        if (desiredTargets.Any())
        {
            Hookable target = desiredTargets[Random.Range(0, desiredTargets.Count)];
            Vector3 targetPos = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
            Vector3 pos = (Quaternion.AngleAxis(Random.Range(-70, 71), Vector3.up) * (targetPos - transform.position)).normalized * Random.Range(MoveTargetMinDist, MoveTargetMaxDist);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(pos, out hit, 4, NavMesh.AllAreas))
            {
                Vector3 temp = hit.position;
                temp.y = transform.position.y;
                if ((temp - transform.position).sqrMagnitude > (UsableController.CurrentUsable.AimModule.MaxDistance * UsableController.CurrentUsable.AimModule.MaxDistance))
                {
                    SetNewMoveTarget();
                }
                else
                {
                    //Debug.DrawRay(hit.position, Vector3.up * 5, Color.green, 0.5f);
                    MoveTarget = hit.position;
                }
            }
            else
            {
                SetNewMoveTarget();
            }
        }
        else
        {
            var insideCircle = Random.insideUnitCircle;
            Vector3 randomDir = new Vector3(insideCircle.x, 0, insideCircle.y) * Random.Range(MoveTargetMinDist, MoveTargetMaxDist);
            randomDir += transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDir, out hit, 4, NavMesh.AllAreas))
            {
                Vector3 temp = hit.position;
                temp.y = transform.position.y;
                if ((temp - transform.position).sqrMagnitude > (UsableController.CurrentUsable.AimModule.MaxDistance * UsableController.CurrentUsable.AimModule.MaxDistance))
                {
                    SetNewMoveTarget();
                }
                else
                {
                    //Debug.DrawRay(hit.position, Vector3.up * 5, Color.green, 0.5f);
                    MoveTarget = hit.position;
                }
            }
            else
            {
                SetNewMoveTarget();
            }
        }
    }

    private bool TargetIsAboveOrObstructed(Vector3 pTarget)
    {
        if ((pTarget.y - (transform.position.y - 0.5f)) >= 0.5f) 
        {
            debugTargetAbove = true;
            return true;
        }
        else
        {
            Vector3 temp = new Vector3(pTarget.x, transform.position.y, pTarget.z);
            Vector3 dir = temp - transform.position;
            bool isSafedir = IsSafeDirection(dir.normalized, dir.magnitude, true);
            debugTargetAbove = !isSafedir;
            return !isSafedir;
        }
    }

    private void SetAttackInput(bool pValue)
    {
        if (pValue)
        {
            if (TimeSinceAttackInputChange > InputBuffer)
            {
                TimeSinceAttackInputChange = 0;
                WindupTimer = 0;
                AttackInput = pValue;
            }
        }
        else
        {
            AttackTarget = null;
            SetNewMoveTarget();
            TimeSinceAttackInputChange = 0;
            AttackInput = pValue;
        }
    }

    private void Dash(Vector3 pDir)
    {
        MoveInput = pDir;

        mDashInput = true;
        SetAttackInput(false);
    }

    private Vector3 GetSafeDir(Vector3 pStartDir)
    {
        float dashDist = 5.5f;
        if (!IsSafeDirection(pStartDir, dashDist))
        {
            float angleIncrement = 15;
            Vector3 newDir;

            for (int i = 1; i < 7; i++)
            {
                newDir = Quaternion.AngleAxis(angleIncrement * i, Vector3.up) * pStartDir;
                if (IsSafeDirection(newDir, dashDist))
                {
                    return newDir;
                }
                else 
                {
                    newDir = Quaternion.AngleAxis(angleIncrement * (-i), Vector3.up) * pStartDir;
                    if (IsSafeDirection(newDir, dashDist))
                    {
                        return newDir;
                    }
                }
            }

            return pStartDir;
        }
        else
        {
            return pStartDir;
        }
    }

    private bool IsSafeDirection(Vector3 pDir, float pLength, /*bool pIgnoreSharks = false*/ bool pOnlyCheckGround = false)
    {
        List<RaycastHit> fhits;
        fhits = Physics.RaycastAll(transform.position, pDir.normalized, pLength, ~(1 << 12)/*everything except powerup layer*/, QueryTriggerInteraction.Ignore).ToList();
        fhits.RemoveAll(a => a.transform.parent == transform || a.transform == transform);
        //if (pIgnoreSharks)
        //{
        //    fhits.RemoveAll(a => a.transform.gameObject.layer == 1 << 9 /*"Sharks" layer*/ || a.transform.parent != null && a.transform.parent.gameObject.layer == 1 << 9);
        //}
        //Debug.DrawRay(transform.position, pDir.normalized * pLength, Color.blue, 0.5f);
        if (!pOnlyCheckGround && fhits.Any())
        {
            return false;
        }
        else
        {
            RaycastHit hit;
            RaycastHit hitHalf;
            Vector3 originHalf = transform.position + (pDir.normalized * pLength);
            Vector3 origin = transform.position + (pDir.normalized * pLength);
            //cast a ray downwards from the end point of the forward ray to check if there is ground beneath
            //Debug.DrawRay(origin, Vector3.down * 4, Color.red, 0.5f);
            bool raycastHalf = Physics.Raycast(originHalf, Vector3.down, out hitHalf, 4, 1 << 10/*Ground layer*/);
            bool raycast = Physics.Raycast(origin, Vector3.down, out hit, 4, 1 << 10/*Ground layer*/);
            if (raycastHalf && raycast)
            {
                bool hitGround;
                bool hitGroundHalf;

                if (!IsOnNavMesh())
                {
                    hitGround = true;
                    hitGroundHalf = true;
                }
                else
                {
                    NavMeshHit navHit;
                    NavMeshHit navHitHalf;
                    //return NavMesh.SamplePosition(hit.point, out navHit, 2, NavMesh.AllAreas);
                    hitGround = NavMesh.SamplePosition(hit.point, out navHit, 0.5f, NavMesh.AllAreas);
                    hitGroundHalf = NavMesh.SamplePosition(hitHalf.point, out navHit, 0.5f, NavMesh.AllAreas);
                }
                

                return hitGround && hitGroundHalf;
            }
            return false;
        }
    }

    private IEnumerator CheckForAttackTargets()
    {
        while (true)
        {
            float rnd = Random.Range(MinAttackCoolDown, MaxAttackCoolDown);
            yield return new WaitForSeconds(rnd);

            if (PlayerController.CurrentState != PlayerController.State.Idle)
            {
                yield return new WaitUntil(() => PlayerController.CurrentState == PlayerController.State.Idle);
            }

            FindAttackTarget();
        }
    }

    private void FindAttackTarget()
    {
        List<Hookable> hookables = FindObjectsOfType<Hookable>().Where(a => a != Me && a.IsAvailable && a.gameObject.activeInHierarchy).ToList();
        List<Hookable> orderedHookables = hookables.OrderBy(a => (a.transform.position - transform.position).sqrMagnitude).ToList();

        if (UsableController.CurrentUsable != UsableController.StandardHook)
        {
            orderedHookables.RemoveAll(a => !(a is PlayerHookable));
        }

        closeHookables = orderedHookables.Where(a => (a.transform.position - transform.position).sqrMagnitude <= (15 * 15)).ToList();

        if (closeHookables.Any())
        {
            if (Random.Range(0, 100) < DiffChance)
            {
                AttackTarget = closeHookables[0];
                AttackTargetPrevPos = AttackTarget.transform.position;
            }
            else
            {
                AttackTarget = closeHookables[Random.Range(0, closeHookables.Count)];
                AttackTargetPrevPos = AttackTarget.transform.position;
            }
        }
        else
        {
            AttackTarget = null;
            AttackTargetPrevPos = Vector3.zero;
        }
    }

    public void InitializeWindup()
    {
        WindupMaxTime = Random.Range(1f, 2f);
    }

    public void SetOwnUsableThrown()
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

    private bool IsOnNavMesh()
    {
        NavMeshHit hit;
        return NavMesh.SamplePosition(transform.position - new Vector3(0, 1, 0), out hit, 1f, NavMesh.AllAreas);
    }

    private IEnumerator NavMeshSafetyCheck()
    {
        while (true)
        {
            switch (PlayerController.CurrentState)
            {
                case PlayerController.State.Idle:
                    if (!IsOnNavMesh())
                    {
                        yield return new WaitForSeconds(Random.Range(MinAvoidanceBuffer, MaxAvoidanceBuffer));
                        if (!IsOnNavMesh())
                        {
                            ReturnToNavMesh();
                        }
                    }
                    break;
                case PlayerController.State.WindUp:
                    if (!IsOnNavMesh())
                    {
                        yield return new WaitForSeconds(Random.Range(MinAvoidanceBuffer, MaxAvoidanceBuffer));
                        if (!IsOnNavMesh())
                        {
                            ReturnToNavMesh();
                        }
                    }
                    break;
                default:
                    break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void ReturnToNavMesh()
    {
        NavMeshHit hit;
        NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas);
        if (TargetIsAboveOrObstructed(hit.position))
        {
            MoveTarget = hit.position;
        }
        else
        {
            Dash(hit.position - transform.position);
        }
    }

    private IEnumerator ResetObjectsToAvoid()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            ObjectsToAvoid = ObjectsToAvoid.Where(a => !a.HasBeenChecked).ToList();
        }
    }

    public void OnDeath()
    {
        StopAllCoroutines();
    }

    public void OnPowerupPickUp()
    {
        FindAttackTarget();
    }

    public void OnRagdoll()
    {
        SetAttackInput(false);
    }

    private IEnumerator UpdateAttackTargetVelocity()
    {
        while (true)
        {
            yield return null;

            if (AttackTarget != null)
            {
                AttackTargetVelocity = (AttackTarget.transform.position - AttackTargetPrevPos);
                AttackTargetPrevPos = AttackTarget.transform.position;
            }
            else
            {
                AttackTargetVelocity = Vector3.zero;
                yield return new WaitUntil(() => AttackTarget != null);
            }
        }
    }
}

[System.Serializable]
public class ObjToAvoid
{
    public Usable usable;
    public bool HasBeenChecked;

    public ObjToAvoid(Usable pUsable, bool pValue)
    {
        usable = pUsable;
        HasBeenChecked = pValue;
    }
}
