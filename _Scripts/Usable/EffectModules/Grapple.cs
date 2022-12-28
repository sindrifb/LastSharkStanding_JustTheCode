using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grapple : EffectModule
{
    public LineRenderer LineRenderer;
    private PlayerController Other;
    private GameObject DetachedOwner;
    private Rigidbody DetachedOwnerRigidbody;
    private bool hooked;
    private bool disabled;
    private float pullTimer;
    private float OverallTimer;
    public override void Initialize(Usable pUsable)
    {
        base.Initialize(pUsable);
        DetachedOwner = Usable.Owner;
        DetachedOwnerRigidbody = Usable.Rigidbody;
        LineRenderer.startColor = Usable.PlayerController.PlayerColor;
        LineRenderer.endColor = Usable.PlayerController.PlayerColor;
    }

    protected override void UpdateEffect()
    {
        base.UpdateEffect();

        LineRenderer.SetPosition(0, DetachedOwner.transform.position);
        LineRenderer.SetPosition(1, transform.position);
        pullTimer += Time.deltaTime;
        if (hooked && Other != null && pullTimer >= .5f)
        {
            pullTimer = 0;
            var dir = (DetachedOwner.transform.position - Other.transform.position);
            Other.AddForce(dir.normalized, 25f + dir.magnitude * 2);

            if (Other.CurrentState == PlayerController.State.Idle)
            {
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.ThrownOut);
            }
        }
        if (hooked)
        {
            if (Usable.Rigidbody?.velocity.magnitude < 5 * 5)
            {
                disabled = true;
            }

            transform.position = Other?.transform.position ?? Vector3.down * 10;
            OverallTimer += Time.deltaTime;
            if (OverallTimer > 3f)
            {
                Destroy(gameObject);
            }
        }
    }

    //no colliders when stuck
    void DisableAllColliders()
    {
        GetComponentsInChildren<Collider>().ToList().ForEach(a => a.enabled = false);
    }

    public void OnTriggerEnter(Collider col)
    {
        if (col.isTrigger || disabled)
        {
            return;
        }

        var pc = col.GetComponent<PlayerController>();
        if (pc != null)
        {
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.GroundBounce);
            DisableAllColliders();
            hooked = true;
            Other = pc;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (hooked)
        {
            return;
        }

        if (!bounced)
        {
            bounced = true;
            StartCoroutine(DestroyInactive());
        }
        

    }
    bool bounced;
    IEnumerator DestroyInactive()
    {
        yield return new WaitForSeconds(1.5f);
        if (!hooked)
        {
            Destroy(gameObject);
        }
    }
}
