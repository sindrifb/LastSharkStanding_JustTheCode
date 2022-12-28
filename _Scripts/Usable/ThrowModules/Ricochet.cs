using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Ricochet : ThrowModule
{
    public float speed;
    [SerializeField]
    private float StartPosForwardOffset;
    public float StartPosYOffset;
    bool moving;
    Vector3 moveDir;
    [SerializeField]
    private bool TurnOffOverTime;
    [SerializeField]
    private bool DestroyOverTime;
    private Animator Anim;
    [SerializeField]
    private GameObject InactiveCollider;
    private Collider ActiveCollider;

    public override void Initialize(Usable pUsable)
    {
        base.Initialize(pUsable);
        Usable.IgnoreOwnerCollision();
        Anim = GetComponent<Animator>();
        ActiveCollider = GetComponents<Collider>().FirstOrDefault(a => a.isTrigger == false);
    }
    protected override void OverridableThrow()
    {
        base.OverridableThrow();
        transform.parent = null;

        moving = true;
        moveDir = Usable.Owner.transform.forward;
        Usable.transform.up = Vector3.up;
        //Usable.transform.position = Usable.Owner.transform.position;

        float approxRadius = 1.5f;
        Ray ray = new Ray(Usable.Owner.transform.position + (Usable.Owner.transform.up * StartPosYOffset), Usable.Owner.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, StartPosForwardOffset + approxRadius);
        List<RaycastHit> notSelfHits = new List<RaycastHit>();
        if (hits.ToList().Any())
        {
            notSelfHits = hits.Where(a => !a.transform.IsChildOf(Usable.Owner.transform)).ToList();
        }

        if (notSelfHits.Any())
        {
            //Debug.Log("something in the way");
            Vector3 dir = (Usable.Owner.transform.position - notSelfHits[0].point).normalized;
            Usable.transform.position = notSelfHits[0].point + (dir * approxRadius);
        }
        else
        {
            //Debug.Log("nothing in the way");
            Usable.transform.position = Usable.Owner.transform.position + (Usable.Owner.transform.forward * (StartPosForwardOffset * 2)) + (Vector3.up * StartPosYOffset);
        }

        if (TurnOffOverTime)
        {
            StartCoroutine(TurnOffOverTimeCoroutine());
        }
        else if (DestroyOverTime)
        {
            StartCoroutine(DestroyOverTimeCoroutine());
        }
        
        Usable.UsableController.ResetToStandardHook();

        GameManager.Instance?.SpawnedObjects.Add(Usable.gameObject);

        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HookSound.HookThrow);

        if (Anim != null)
        {
            Anim.SetTrigger(Constants.AnimationParameters.Play);
        }
        
        Usable.StopIgnoringColliders();
    }

    //protected override void IndependentUpdate()
    //{
    //    base.IndependentUpdate();
    //    if (moving)
    //    {
    //        Usable.Rigidbody.MovePosition(transform.position + (moveDir * speed * Time.deltaTime));
    //    }
    //}

    private void FixedUpdate()
    {
        if (moving)
        {
            Usable.Rigidbody.MovePosition(transform.position + (moveDir * speed * Time.deltaTime));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
            Vector3 referencePoint = -moveDir * 3;
            ContactPoint closestContact = collision.contacts[0];
            foreach (var contact in collision.contacts)
            {
                float oldDist = (referencePoint - closestContact.point).sqrMagnitude;
                float newDist = (referencePoint - contact.point).sqrMagnitude;
                if (newDist < oldDist)
                {
                    closestContact = contact;
                }
            }

            var v2MoveDir = new Vector2(moveDir.x, moveDir.z);
            var v2ColNormal = new Vector2(closestContact.normal.x, closestContact.normal.z);
            var reflect = Vector2.Reflect(v2MoveDir, v2ColNormal);
            moveDir = new Vector3(reflect.x, 0, reflect.y).normalized;
    }

    private IEnumerator TurnOffOverTimeCoroutine()
    {
        yield return new WaitForSeconds(3f);
        Usable.ThrownParticlesParent.SetActive(false);
        ActiveCollider.enabled = false;
        InactiveCollider.SetActive(true);
        moving = false;
        Usable.Rigidbody.isKinematic = false;
        //Usable.Rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionY;
        Usable.IsActive = false;
        
        Usable.Rigidbody.AddForce(moveDir * speed * Time.deltaTime,ForceMode.Impulse);
    }

    private IEnumerator DestroyOverTimeCoroutine()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
        if (Usable.OnDestroyParticlesPrefab != null)
        {
            var go = Instantiate(Usable.OnDestroyParticlesPrefab, transform.position, Quaternion.identity);
            Destroy(go, 3f);
        }
    }
}
