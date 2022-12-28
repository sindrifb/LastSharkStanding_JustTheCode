using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushOnCollision : MonoBehaviour
{
    public enum HazardObject : int
    {
        Darts,
        Boulder,
        Pressureplate
    }
    public HazardObject HazardTypeObject;
    public List<Transform> HitPoints;
    //public bool CanHitScreen;
    public bool LandOnSpecificPoints;
    public GameObject ParticleEffect;
    public Animator Animator;
    private BoulderController BoulderController;
    bool usable = true;
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
        if (!usable)
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

            if (LandOnSpecificPoints)
            {
                var projData = new ProjectileData(colHookable.transform.position, HitPoints[Random.Range(0,HitPoints.Count)].position,50,2);
                if (colHookable is PlayerHookable)
                {
                    var player = colHookable.GetComponent<PlayerController>();
                    colHookable.Push(Vector3.up, 5);
                    player.Rigidbody.drag = 0;
                    player.Rigidbody.angularDrag = 0;
                    player.Rigidbody.velocity = projData.Velocity;
                }
                else
                {
                    colHookable.Push(projData.Velocity.normalized, projData.Velocity.magnitude);
                }
               
            }
            else
            {
                colHookable.Push((dir).normalized, force * 1.5f);
                HazardEvent hazInfo = new HazardEvent
                {
                    Description = "Dart hit event",
                    PlayerHit = (colHookable as PlayerHookable).gameObject,
                    HazardType = AchHazardType.None,
                    RewiredID = (colHookable as PlayerHookable).PlayerController.RewiredID,
                    Skin = (colHookable as PlayerHookable).PlayerController.Skin
                };
                switch (HazardTypeObject)
                {
                    case HazardObject.Darts:
                        hazInfo.HazardType = AchHazardType.Pillars;
                        break;
                    case HazardObject.Boulder:
                        hazInfo.HazardType = AchHazardType.Boulder;
                        break;
                    case HazardObject.Pressureplate:
                        hazInfo.HazardType = AchHazardType.Pressureplate;
                        break;
                    default:
                        break;
                }
                hazInfo.FireEvent();
                //usable = false;
                //StartCoroutine(UsableReset());
                bool kill = Random.Range(0, 3) < 1 ? false : true;
                //if (CanHitScreen && kill && colHookable is PlayerHookable)
                //{
                //    colHookable.GetComponent<Death>().Die(null, true);
                //    colHookable.GetComponent<Death>().hitScreen = true;
                //}
            }
            var ps = Instantiate(ParticleEffect, col.contacts[0].point, Quaternion.LookRotation(dir));
            Destroy(ps, 3);
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
            // Keeping old velocity when changing direction
            var oldVel = colRB.velocity.magnitude;
            colRB.velocity = dir.normalized * oldVel;

            colRB.AddForce((dir).normalized * force, ForceMode.Impulse);

            if (DisableBoulder)
            {
                boulder?.Crash(dir, force);
            }

            var ps = Instantiate(ParticleEffect, col.contacts[0].point, Quaternion.LookRotation(dir));
            Destroy(ps, 3);
            //usable = false;
            //StartCoroutine(UsableReset());
        }

       
    }

    IEnumerator UsableReset()
    {
        yield return new WaitForSeconds(.5f);
        usable = true;
    }
}
