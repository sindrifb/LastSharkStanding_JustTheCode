using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ParticleController : MonoBehaviour
{
    [Header("Put on particle system game object")]

    public bool IsAlwaysFacingUp;
    public bool EmissionOnWhenOnGround;
    public bool FollowXZ;

    ParticleSystem.EmissionModule emission;
    ParticleSystem.MainModule main;

    public Transform FollowTransform;

    private void Awake()
    {
        if (IsAlwaysFacingUp)
        {
            transform.rotation = Quaternion.identity;
        }
    }

    // Use this for initialization
    void Start () {
        emission = GetComponentInChildren<ParticleSystem>().emission;
        main = GetComponentInChildren<ParticleSystem>().main;
        if (FollowTransform == null)
        {
            FollowTransform = transform.root;
        }
    }
	
	// Update is called once per frame
	void LateUpdate () {
        //f off CIrcle
        if (transform.root.GetComponent<Death>()?.IsDead ?? false)
        {
            transform.position = Vector3.one * 10000;
            return;
        }
        if (FollowXZ)
        {
            FollowXZUpdate();
        }
        if (IsAlwaysFacingUp)
        {
            transform.rotation = Quaternion.identity;
        }
	}

    private void FollowXZUpdate()
    {
        float yPos;
        RaycastHit hit;
        Vector3 rayStart = transform.parent.position + new Vector3(0, 1, 0);
        var ray = Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity, 1 << 10);
        if (ray)
        {
            if (!transform.GetChild(0).gameObject.activeInHierarchy)
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(true);
                }
                //gameObject.SetActive(true);
                //print("should be active");
            }
            yPos = hit.point.y + 1;
        }
        else
        {
            if (EmissionOnWhenOnGround && transform.GetChild(0).gameObject.activeInHierarchy)
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }
                //gameObject.SetActive(false);
                //print("should not be active");
            }
            yPos = transform.root.position.y;
        }
        
        transform.position = new Vector3(FollowTransform.position.x, yPos, FollowTransform.position.z);
        transform.rotation = Quaternion.identity;
    }
}
