using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEnterHookable : MonoBehaviour {
    public float Force = 30f;
	private void OnTriggerEnter(Collider col)
    {
        var hookedObject = col.GetComponentInChildren<Hookable>();

        if (hookedObject != null && hookedObject.IsAvailable)
        {
            Vector3 dir = (col.transform.position - transform.position).normalized / 4f;
            if (hookedObject is PlayerHookable)
            {
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.HookSound.AnchorHit);
                hookedObject.Push((transform.forward + (Vector3.up) + dir).normalized, Force);
                HazardEvent hazInfo = new HazardEvent
                {
                    Description = "Cannon hit event",
                    PlayerHit = (hookedObject as PlayerHookable).gameObject,
                    RewiredID = (hookedObject as PlayerHookable).PlayerController.RewiredID,
                    HazardType = AchHazardType.Cannons,
                    Skin = (hookedObject as PlayerHookable).PlayerController.Skin
                };
                hazInfo.FireEvent();
            }
            //else
            //{
            //    hookedObject.Push((transform.forward + (Vector3.up / 2f) + dir).normalized, 5f);
            //}
        }
    }
}
