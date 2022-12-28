using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectileMotion : MonoBehaviour
{
    public GameObject HitPoint;
    public Transform Target;
    public float Radius = .5f;
    public float FiringAngle = 45.0f;
    public float GravityAmplifier = -40.81f; 

    public Transform Projectile;
    private Transform myTransform;

    public LineRenderer LineRenderer;

    private ProjectileData ProjectileData;

    void Awake()
    {
        myTransform = transform;
    }

    void Start()
    {
        //StartCoroutine(SimulateProjectile());
    }

    void Update()
    {
        

        if (Input.GetKeyDown(KeyCode.G))
        {
            SimulateProjectile();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Fire();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleAim();
        }

        if (aim)
        {
            DrawCurve();
        }

        //if (reseting)
        //{
        //    MoveHook();
        //}
    }

    void FixedUpdate()
    {
        Projectile.GetComponent<Rigidbody>().AddForce(Physics.gravity * GravityAmplifier);
    }

    void Fire()
    {
        if (ProjectileData != null)
        {
            reseting = false;
            Projectile.transform.position = ProjectileData.Origin;
            Projectile.rotation = Quaternion.LookRotation(ProjectileData.Target - ProjectileData.Origin);
            Projectile.GetComponent<Rigidbody>().velocity = ProjectileData.Velocity;
            StartCoroutine(Reset(ProjectileData.Duration + 1));
        }
        else
        {
            SimulateProjectile();
        }
    }


    bool aim;
    void ToggleAim()
    {
        if (ProjectileData != null)
        {
            aim = !aim;
            LineRenderer.enabled = aim;
        }
    }

    void DrawCurve()
    {
        LineRenderer.positionCount = ProjectileData.CurvePoints.Count;
        LineRenderer.SetPositions(ProjectileData.CurvePoints.ToArray());
        if (ProjectileData.IsCurveObstructed)
        {
            HitPoint.transform.position = ProjectileData.CurveObstructionPoint;
        }
    }

    void SimulateProjectile()
    {
        ProjectileData = new ProjectileData(transform.position,Target.transform.position,FiringAngle, Radius, GravityAmplifier);
        if (!aim)
        {
            ToggleAim();
        }
    }

    bool reseting = true;
    //void MoveHook()
    //{
    //    Projectile.GetComponent<Rigidbody>().AddForce((transform.position- Projectile.transform.position).normalized * 50);
    //}

    IEnumerator Reset(float dur)
    {
        yield return new WaitForSeconds(dur);
        reseting = true;
    }
}

public class ProjectileData
{
    public Vector3 Origin { get; private set; }
    public Vector3 Target { get; private set; }
    public Vector3 Velocity { get; private set;}
    public Vector3 CurveVelocity { get; private set; }
    public Vector3 CurveObstructionPoint { get; private set; }
    public bool IsCurveObstructed { get; private set; }
    public float FiringAngle { get; private set; }
    public float Duration { get; private set; }
    public float Distance { get; private set; }
    public float GravityAmplifier { get; private set; }
   
    public List<Vector3> CurvePoints
    { get
        {
            if (Curve != null)
            {
                return Curve;
            }
            else
            {
                return Curve = CreateCurve();
            }
        }
    }
    private List<Vector3> Curve;

    public ProjectileData(Vector3 pOrigin, Vector3 pTarget, float pAngle,float pRadius, float pGravityAmplifier = 1f)
    {
        Vector3 originXZ = pOrigin;
        Vector3 targetXZ = pTarget;
        originXZ.y = targetXZ.y = 0;

        float R = Distance = (Vector3.Distance(originXZ, targetXZ) + pRadius);
        float G = Physics.gravity.y + (Physics.gravity.y * GravityAmplifier);
        float tanAlpha = Mathf.Tan(pAngle * Mathf.Deg2Rad);
        float H = pTarget.y - pOrigin.y;

        float Vz = Mathf.Sqrt(G*R*R / (2.0f * (H-R*tanAlpha)));
        float Vy = tanAlpha * Vz;
        float curveVz = Mathf.Sqrt(G * (R - pRadius) * (R - pRadius) / (2.0f * (H - R * tanAlpha)));
        float curveVy = tanAlpha * curveVz;


        //float projectileVelocity = targetDistance / (Mathf.Sin(2 * pAngle * Mathf.Deg2Rad) / Physics.gravity.magnitude);

        //float Vy = Mathf.Sqrt(projectileVelocity) * Mathf.Sin(pAngle * Mathf.Deg2Rad);
        //float Vz = Mathf.Sqrt(projectileVelocity) * Mathf.Cos(pAngle * Mathf.Deg2Rad);
        Velocity = Quaternion.LookRotation(targetXZ - originXZ) * new Vector3(0, Vy, Vz);
        CurveVelocity = Quaternion.LookRotation(targetXZ - originXZ) * new Vector3(0, curveVy, curveVz);
        GravityAmplifier = pGravityAmplifier;
        Duration = R/Vz;
        FiringAngle = pAngle;
        Origin = pOrigin;
        Target = pTarget;
    }

    private List<Vector3> CreateCurve()
    {
       
        var list = new List<Vector3>();
        var point = Origin;
        for (int i = 0; i < 40; i++)
        {   
            var deltaTime = (Duration / 40);
            var elapsedTime = deltaTime * i;
            point += new Vector3(CurveVelocity.x * deltaTime, (CurveVelocity.y - ((Physics.gravity.magnitude + (Physics.gravity.magnitude * GravityAmplifier)) * elapsedTime)) * deltaTime , CurveVelocity.z * deltaTime);
            list.Add(new Vector3(point.x,point.y,point.z));
        }
        Vector3 hitpoint;
        int indexOfHit;
        if (HitPoint(list, out hitpoint, out indexOfHit))
        {
            Debug.Log(indexOfHit);
            Debug.Log(hitpoint);
            list.RemoveRange(indexOfHit, list.Count - indexOfHit);
            list.Add(hitpoint);
            CurveObstructionPoint = hitpoint;
            IsCurveObstructed = true;
        }
        return list;
    }

    //check if curve is obstructed
    private bool HitPoint(List<Vector3> points, out Vector3 point, out int IndexOfHit)
    {
        int N = points.Count - 1;
        int N_2 = N/2;
        int N_S = 0;
        while (true)
        {
            Vector3 s = points[N_S];
            Vector3 m = points[N_2];
            Vector3 e = points[N];
            Ray ray = new Ray(s, m - s);
            Debug.DrawRay(ray.origin,ray.direction * (m - s).magnitude, (new Vector4(1, 1, 1, 1) - (Vector4)Random.onUnitSphere),5f);
            RaycastHit hit;
            //hit something from start to middle
            if (Physics.Raycast(ray,out hit, (m-s).magnitude))
            {
                Debug.Log("HIT");
                //precise enough or no more points
                if (hit.distance < .2f || N_S == N || N_S == N - 1)
                {
                    point = hit.point;
                    IndexOfHit = N_S;
                    return true;
                }
                N = N_2;
                N_2 = N / 2;
                continue;
            }
            ray = new Ray(m, e - m);
            Debug.DrawRay(ray.origin, ray.direction * (m - s).magnitude, (new Vector4(1, 1, 1, 1) - (Vector4)Random.onUnitSphere),5f);
            if (Physics.Raycast(ray.origin,ray.direction * (m - e).magnitude, out hit, (m-e).magnitude))
            {
                Debug.Log("HIT");
                if (hit.distance < .2f || N_S == N || N_S == N - 1)
                {
                    point = hit.point;
                    IndexOfHit = N_2;
                    return true;
                }

                N_S = N_2;
                N_2 = N_S + (N - N_S) / 2;
                continue;
            }
            //nothing was hit
            else
            {
                point = Vector3.zero;
                IndexOfHit = -1;
                return false;
            }
        }
    }
}