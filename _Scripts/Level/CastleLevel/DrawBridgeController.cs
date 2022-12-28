using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawBridgeController : MonoBehaviour
{
    public GameObject BridgePlatform;
    public float TimeOpen = 10;
    //public float BridgeSpeed = 10;
    public Color AvailableColor;
    public Color UnAvailableColor;
    private bool IsActive;
    private Rigidbody Rb;
    private DrawBridgeThrower DrawBridgeThrower;
    private Animator BridgeAnimator;
    private Coroutine BridgeCoroutine;

    private void Start()
    {
        EventManager.StartListening(EventManager.EventCodes.RoundEnd, ResetBridge);
        BridgeAnimator = BridgePlatform.GetComponent<Animator>();
        Rb = BridgePlatform.GetComponent<Rigidbody>();
        DrawBridgeThrower = BridgePlatform.GetComponentInChildren<DrawBridgeThrower>();
        IsActive = false;
        GetComponent<MeshRenderer>().material.color = AvailableColor;
    }

    private void ActivateBridge()
    {
        //Debug.Log("Bridge Activated");
        var startRot = BridgePlatform.transform.eulerAngles;

        var targetPush = Quaternion.Euler(startRot.x, startRot.y, startRot.z - 40f) * Vector3.up;

        DrawBridgeThrower.ActivateBridge(targetPush.normalized);
        BridgeCoroutine = StartCoroutine(BridgeOpen(startRot));
    }

    private IEnumerator BridgeOpen(Vector3 pStartRot)
    {
        GetComponent<MeshRenderer>().material.color = UnAvailableColor;
        BridgeAnimator.SetBool(Constants.AnimationParameters.IsOpen, true);
        //while (BridgePlatform.transform.eulerAngles.z != 320)
        //{
        //    MoveBridge(Quaternion.Euler(pStartRot.x, pStartRot.y, -40f));
        //    yield return new WaitForEndOfFrame();
        //}
        //Debug.Log("Opened");
        yield return new WaitForSeconds(TimeOpen);
        BridgeAnimator.SetBool(Constants.AnimationParameters.IsOpen, false);
        //Debug.Log("Closing");
        //while (BridgePlatform.transform.eulerAngles.z != 0f)
        //{
        //    MoveBridge(Quaternion.Euler(pStartRot.x, pStartRot.y, 0f));
        //    yield return new WaitForEndOfFrame();
        //}
        //Debug.Log("Reset");

        ResetBridge();
    }

    //private void MoveBridge(Quaternion pTarget)
    //{
    //    Rb.rotation = Quaternion.Lerp(BridgePlatform.transform.rotation, Quaternion.Euler(pTarget.eulerAngles), Time.deltaTime * BridgeSpeed);
    //}

    private void ResetBridge()
    {
        if (BridgeCoroutine != null)
        {
            StopCoroutine(BridgeCoroutine);
        }
        GetComponent<MeshRenderer>().material.color = AvailableColor;
        IsActive = false;
        BridgeAnimator.SetBool(Constants.AnimationParameters.IsOpen, false);
        BridgeAnimator.Play(Constants.AnimationParameters.CatapultIdle);
    }

    private void OnTriggerEnter(Collider other)
    {
        var usable = other.GetComponent<Usable>();
        if (usable != null && !IsActive)
        {
            DrawBridgeThrower.CheckIfOwnerOnBridge(usable);
            IsActive = true;
            ActivateBridge();
            //***not sure why this is needed***
            //if (usable is Yeet)
            //{
            //    usable.OnFinishedThrow();
            //}
        }
    }

    //private void OnDrawGizmos()
    //{
    //    var startRot = BridgePlatform.transform.eulerAngles;

    //    var targetPush = Quaternion.Euler(startRot.x, startRot.y, startRot.z - 40f) * Vector3.up;
    //    Gizmos.DrawRay(BridgePlatform.transform.position, targetPush * 10f);
    //}
}
