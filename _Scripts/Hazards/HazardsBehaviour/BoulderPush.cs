using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderPush : MonoBehaviour
{

    public GameObject ParticleEffect;
    public Animator Animator;
    private BoulderController BoulderController;
    public bool IsActive = true;
    public float force = 80;
    public bool UseForward;
    public bool DisableBoulder;
    public Vector3 OffsetForceDirection = Vector3.up;
    public Vector3 boulderForceDirectionOffset = -Vector3.forward;
    void Start()
    {
        BoulderController = transform.root.GetComponent<BoulderController>();
    }

    void OnCollisionEnter(Collision col)
    {
        if (!IsActive)
        {
            return;
        }

        var dir = UseForward ? transform.forward + OffsetForceDirection : (-col.contacts[0].normal + OffsetForceDirection);
        var colRB = col.transform.GetComponent<Rigidbody>();
        var colHookable = col.transform.GetComponent<Hookable>();
        if (colHookable != null)
        {
            
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.BeachBallLand);
            if (Animator != null)
            {
                Animator.SetTrigger("play");
            }
            colHookable.Push((dir).normalized, force * 1.5f);
            var ps = Instantiate(ParticleEffect, col.contacts[0].point, Quaternion.LookRotation(dir));
            Destroy(ps, 3);
            BoulderController.Crash();
            HazardEvent hazInfo = new HazardEvent
            {
                Description = "Boulder hit event",
                PlayerHit = (colHookable as PlayerHookable).gameObject,
                RewiredID = (colHookable as PlayerHookable).PlayerController.RewiredID,
                HazardType = AchHazardType.Boulder,
                Skin = (colHookable as PlayerHookable).PlayerController.Skin
            };
            hazInfo.FireEvent();
            //usable = false;
            //StartCoroutine(UsableReset());
        }
        else if (colRB != null && !colRB.isKinematic)
        {
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.BeachBallLand);
            if (Animator != null)
            {
                Animator.SetTrigger("play");
            }
            var boulder = col.transform.root.GetComponent<BoulderController>();
            float tempForce = force;
            if (boulder != null)
            {
                tempForce *= 10;
                dir -= OffsetForceDirection;
                dir += boulderForceDirectionOffset;
            }

            colRB.AddForce((dir).normalized * force, ForceMode.Impulse);

            var ps = Instantiate(ParticleEffect, col.contacts[0].point, Quaternion.LookRotation(dir));
            Destroy(ps, 3);
            //usable = false;
            //StartCoroutine(UsableReset());
        }


    }
}
