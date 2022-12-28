using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcAim : AimModule 
{
    [SerializeField]
    protected GameObject AimCurvePrefab;
    [HideInInspector]
    public BansheeGz.BGSpline.Curve.BGCurve Curve;
    public BansheeGz.BGSpline.Curve.BGCurvePointI[] Points { private set; get; }
    [HideInInspector]
    public BansheeGz.BGSpline.Components.BGCcTrs TRS;
    [HideInInspector]
    public LineRenderer CurveLineRend;
    [SerializeField]
    protected LayerMask GroundLayer;
    [SerializeField]
    public LayerMask RayCastHitLayers;
    [SerializeField]
    private Transform CurvePointOfOrigin;

    public override void Initialize(Usable pUsable)
    {
        base.Initialize(pUsable);
        
        InitializeHookCurve(AimCurvePrefab);
    }

    public override void InitializeWindup()
    {
        base.InitializeWindup();
        //if (Curve == null)
        //{
        //    InitializeHookCurve(AimCurvePrefab);
        //}
        TargetPos = Usable.Owner.transform.position;
        ChangeStartEndOnCurve(CurvePointOfOrigin.position, TargetPos);
        CurveLineRend.enabled = true;
    }

    public override void UpdateWindup()
    {
        base.UpdateWindup();

        RaycastHit hit;
        Ray ray = new Ray();

        ray.origin = Usable.Owner.transform.position + (Usable.Owner.transform.forward * ThrowDistance) + (Vector3.up * 20f);
        ray.direction = Vector3.down;
        //var rayCast = Physics.Raycast(ray, out hit, 40f, RayCastHitLayers, QueryTriggerInteraction.Ignore);
        
        var sphereCast = Physics.SphereCast(ray, 0.5f, out hit, 40f, RayCastHitLayers, QueryTriggerInteraction.Ignore);

        if (sphereCast)
        {
            //Vector3 lastPos = new Vector3(TargetPos.x, 0, TargetPos.z);
            //Vector3 newPos = new Vector3(hit.point.x, 0, hit.point.z);
            //Vector3 OwnerPos = new Vector3(Usable.Owner.transform.position.x, 0, Usable.Owner.transform.position.z);
            //if ((lastPos - OwnerPos).magnitude > (newPos - OwnerPos).magnitude)
            //{
            //    Debug.Log("tried to reduce throw distance");
            //    return;
            //}

            TargetPos = hit.point;

            ChangeStartEndOnCurve(CurvePointOfOrigin.position, TargetPos);
            Aim.transform.position = Points[Points.Length - 1].PositionWorld + AimOffset;

            if (hit.transform.gameObject.layer == Mathf.Log(GroundLayer.value, 2))
            {
                Aim.SetActive(true);
            }
            else
            {
                //Debug.Log("spherecast. layer: " + hit.transform.gameObject.layer + " gameobject: " + hit.transform.gameObject.name);
                Aim.SetActive(false);
            }
            
        }
        else
        {
            var outsideRay = new Ray(Usable.Owner.transform.position + (Usable.Owner.transform.forward * ThrowDistance) + (Vector3.up * 10f), Vector3.down);
            if (Physics.Raycast(outsideRay, out hit, 20f))
            {
                //Vector3 lastPos = new Vector3(TargetPos.x, 0, TargetPos.z);
                //Vector3 newPos = new Vector3(hit.point.x, 0, hit.point.z);
                //Vector3 OwnerPos = new Vector3(Usable.Owner.transform.position.x, 0, Usable.Owner.transform.position.z);
                //if ((lastPos - OwnerPos).magnitude > (newPos - OwnerPos).magnitude)
                //{
                //    Debug.Log("tried to reduce throw distance");
                //    return;
                //}
                TargetPos = hit.point;
                ChangeStartEndOnCurve(CurvePointOfOrigin.position, TargetPos);
                Aim.transform.position = Points[Points.Length - 1].PositionWorld + AimOffset;
                Aim.SetActive(true);
            }
            else
            {
                TargetPos = outsideRay.origin + (Vector3.down * 10f);
                ChangeStartEndOnCurve(CurvePointOfOrigin.position, TargetPos);
                Aim.transform.position = Points[Points.Length - 1].PositionWorld + AimOffset;
                Aim.SetActive(false);
                //Debug.Log("outside ray. layer: " + hit.transform.gameObject.layer);
            }
        }
    }

    public virtual void InitializeHookCurve(GameObject pCurvePrefab)
    {
        if (Curve == null)
        {
            Curve = Instantiate(pCurvePrefab, Usable.Owner.transform).GetComponent<BansheeGz.BGSpline.Curve.BGCurve>();
        }
       
        CurveLineRend = Curve.GetComponent<LineRenderer>();
        CurveLineRend.enabled = false;
        if (CurveLineRend)
        {
            CurveLineRend.endColor = Usable.PlayerController.PlayerColor;
            CurveLineRend.startColor = Usable.PlayerController.PlayerColor;
        }

        Curve.transform.position = Usable.Owner.transform.position;
        //Curve = spawnedCurve.GetComponent<BansheeGz.BGSpline.Curve.BGCurve>();
        //if (Curve.GetComponent<LineRenderer>())
        //{
        //    Curve.GetComponent<LineRenderer>().enabled = false;
        //}

        Points = Curve.Points;
        TRS = Curve.GetComponent<BansheeGz.BGSpline.Components.BGCcTrs>();
        //ChangeStartEndOnCurve(Usable.Owner.transform.position, Usable.Owner.transform.position);
    }

    protected void ChangeStartEndOnCurve(Vector3 pStart, Vector3 pEnd)
    {
        if (Points != null)
        {
            Points[0].PositionWorld = pStart;
            Points[Points.Length - 1].PositionWorld = pEnd;
        }
    }

    public override void ResetAim()
    {
        base.ResetAim();

        if (Curve != null)
        {
            //Destroy(Curve.gameObject);
            CurveLineRend.enabled = false;
        }
    }
}
