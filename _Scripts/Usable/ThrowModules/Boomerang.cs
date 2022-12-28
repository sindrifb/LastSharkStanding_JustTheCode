using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Boomerang : ThrowModule
{
    public float maxAngle;
    public float angle = 1f;
    public bool withBounce;
    public float speed;
    bool moving;
    Vector3 moveDir;
    private EventInstance SoundEvent;
    [SerializeField]
    private GameObject InactiveCollider;
    private Collider ActiveCollider;
    private Animator Animator;
    public AnimationCurve AnimCurve;
    private float AnimTime;

    public override void Initialize(Usable pUsable)
    {
        base.Initialize(pUsable);
        Usable.IgnoreOwnerCollision();
        ActiveCollider = GetComponents<Collider>().FirstOrDefault(a => a.isTrigger == false);
        Animator = GetComponentInChildren<Animator>();
    }
    protected override void OverridableThrow()
    {
        base.OverridableThrow();
        Animator.SetBool("Spin", true);
        transform.parent = null;

        moving = true;
        moveDir = Quaternion.Euler(0, -55, 0) * Usable.Owner.transform.forward;

        Usable.transform.position = Usable.Owner.transform.position;


        Usable.UsableController.ResetToStandardHook();

        GameManager.Instance?.SpawnedObjects.Add(Usable.gameObject);
        AudioManager.Instance.PlayEvent(AudioManager.Instance.PowerupSound.BoomerangThrow, out SoundEvent);

        Usable.StopIgnoringColliders();
    }

    private void FixedUpdate()
    {
        if (moving)
        {
            //transform.Rotate(0, 1000 * Time.deltaTime, 0);
            AnimTime += Time.deltaTime;

            angle += AnimCurve.Evaluate(AnimTime) * (1 * Time.deltaTime);

            //angle = Mathf.Clamp((angle + (6 * Time.deltaTime)), 0, maxAngle);

            if (angle >= maxAngle - 0.1f && Usable.IsActive)
            {
                AnimTime = 0;
                Usable.IsActive = false;
                moving = false;
                ActiveCollider.enabled = false;
                InactiveCollider.SetActive(true);
                Usable.Rigidbody.isKinematic = false;
                Usable.Rigidbody.AddForce(moveDir.normalized * 0.5f, ForceMode.Impulse);
                Usable.Rigidbody.AddTorque(Vector3.left * 50);
                SoundEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                Animator.SetBool("Spin", false);
            }

            moveDir = Quaternion.AngleAxis(angle, Vector3.up) * moveDir;
            Usable.Rigidbody.MovePosition(transform.position + (moveDir * speed * Time.deltaTime));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (withBounce /*&& collision.gameObject.layer != 16*/)
        {
            //moveDir = Vector3.Reflect(moveDir, collision.contacts[0].normal);
            //moveDir.y = 0;
            //moveDir.Normalize();
            var v2MoveDir = new Vector2(moveDir.x, moveDir.z);
            var v2ColNormal = new Vector2(collision.contacts[0].normal.x, collision.contacts[0].normal.z);
            var reflect = Vector2.Reflect(v2MoveDir, v2ColNormal);
            moveDir = new Vector3(reflect.x, 0, reflect.y).normalized;
        }

        //if (collision.collider.GetComponent<PlayerController>() != null)
        //{
        //    Usable.IgnoreOwnerCollision(collision.collider.transform);
        //}
    }

    private void OnDestroy()
    {
        SoundEvent.getPlaybackState(out PLAYBACK_STATE state);
        if (state == PLAYBACK_STATE.PLAYING)
        {
            SoundEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }
}
