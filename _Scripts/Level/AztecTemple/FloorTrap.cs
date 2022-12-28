using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorTrap : MonoBehaviour
{
    public LayerMask GroundLayer;
    public Animator ButtonAnimator;
    public GameObject Latch;
    public GameObject Wall;
    private float Timer = 0f;
    //public float TrapMoveOutTime = 1f;
    //public float TrapMoveinTime = 1f;
    public float Speed = 20;
    public AnimationCurve MoveOutSpeed;
    public AnimationCurve MoveInSpeed;
    public Vector3 EndPos;
    public Vector3 StartPos;
    Rigidbody Rigidbody;
    PushOnCollision pushOnCollision;
    public bool IsMoving;
    Quaternion StartRotLatch;
    void Start()
    {
        Rigidbody = Wall.GetComponent<Rigidbody>();
        pushOnCollision = Wall.GetComponent<PushOnCollision>();
        StartRotLatch = Latch.transform.rotation;
        autoswapper = IsMoving;
    }
    bool autoswapper;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Activate();
        }

        Timer += Time.deltaTime;
        if (IsMoving)
        {
            pushOnCollision.force = 80;
            float currentDistance = Vector3.Distance(Rigidbody.transform.position, transform.TransformPoint(EndPos));
            float startDist = Vector3.Distance(transform.TransformPoint(StartPos), transform.TransformPoint(EndPos));
            float diff = currentDistance != 0 ? (currentDistance / startDist) : 1;
            if (currentDistance <= .1f)
            {
                IsMoving = false;

            }

            Latch.transform.rotation = Quaternion.Slerp(Latch.transform.rotation, StartRotLatch * Quaternion.AngleAxis(-100,Vector3.right), 500 * Time.deltaTime);
            
            Rigidbody.MovePosition(Vector3.MoveTowards(Rigidbody.transform.position, transform.TransformPoint(EndPos), Time.deltaTime * Speed * MoveOutSpeed.Evaluate(diff)));
           
        }
        else
        {
            if (autoswapper != IsMoving)
            {
                autoswapper = IsMoving;
                ButtonAnimator?.SetTrigger("play");
            }
            pushOnCollision.force = 0;
            float currentDistance = Vector3.Distance(Rigidbody.transform.position, transform.TransformPoint(StartPos));
            float startDist = Vector3.Distance(transform.TransformPoint(EndPos), transform.TransformPoint(StartPos));
            float diff = currentDistance != 0 ? (currentDistance / startDist) : 1;

            Rigidbody.MovePosition(Vector3.MoveTowards(Rigidbody.transform.position, transform.TransformPoint(StartPos), Time.deltaTime * Speed * MoveInSpeed.Evaluate(diff)));
            if (currentDistance <= 1)
            {
                Latch.transform.rotation = Quaternion.Slerp(Latch.transform.rotation, StartRotLatch, 5 * Time.deltaTime);
            }
        }
    }

    void Activate()
    {
        if (!IsMoving)
        {
            ButtonAnimator?.SetTrigger("play");
            IsMoving = true;
            autoswapper = true;
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.CannonFire);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null && Wall != other.gameObject && other.gameObject.layer != Mathf.Log(GroundLayer.value, 2))
        {
            Activate();
        }
    }
}
