using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FromUpHazard : BasicHazard
{
    // CURVES SHIT
    public GameObject CurvePrefab;
    public BansheeGz.BGSpline.Curve.BGCurve Curve;
    protected BansheeGz.BGSpline.Curve.BGCurvePointI[] Points;
    protected BansheeGz.BGSpline.Components.BGCcTrs TRS;

    protected override void CheckLanded()
    {
        Landed = (TRS && TRS.DistanceRatio == 1); // means it has ended
    }

    protected override void SpawnHaz()
    {

        var spawnedCurve = Instantiate(CurvePrefab);
        spawnedCurve.transform.position = transform.position;

        var curveLineRend = spawnedCurve.GetComponent<LineRenderer>();

        Curve = spawnedCurve.GetComponent<BansheeGz.BGSpline.Curve.BGCurve>();
        Curve.GetComponent<LineRenderer>().enabled = false;

        Points = Curve.Points;
        TRS = Curve.GetComponent<BansheeGz.BGSpline.Components.BGCcTrs>();
        ChangeStartEndOnCurve(transform.position, Hazard.EndPos);

        TRS.ObjectToManipulate = this.transform;
        TRS.Speed = Hazard.MovementMagnitude;

        base.SpawnHaz();
    }

    protected void ChangeStartEndOnCurve(Vector3 pStart, Vector3 pEnd)
    {
        if (Points != null)
        {
            Points[0].PositionWorld = pStart;
            Points[Points.Length - 1].PositionWorld = pEnd;
        }
    }

    public override void KillHazard()
    {
        base.KillHazard();
        Destroy(Curve.gameObject);
    }
}
