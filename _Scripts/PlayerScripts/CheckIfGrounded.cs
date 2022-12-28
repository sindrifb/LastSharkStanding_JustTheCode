using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckIfGrounded : MonoBehaviour {

    private Ray rayToFloor;
    private RaycastHit rayData;
    private float halfHeigth;
    public float distanceFromGround { get; private set; }
    public LayerMask LayerMask;
    private List<int> Layers = new List<int>();
    public bool AboveGround { get; private set; }
    [SerializeField]
    private bool GroundedAndStuff;
    private PlayerController PlayerController;
    public bool OnGround { get; private set; }

    // Use this for initialization
    void Start()
    {
        
        AboveGround = true;
        halfHeigth = transform.lossyScale.y;
        rayToFloor.origin = transform.position;
        rayToFloor.direction = Vector3.down;
        Layers.Add(LayerMask.NameToLayer("Ground"));
        Layers.Add(LayerMask.NameToLayer("PowerUps"));
        PlayerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    public void Update()
    {
        rayToFloor.origin = transform.position;

        var sphere = Physics.SphereCast(rayToFloor, 0.15f, out rayData, halfHeigth + 1f, LayerMask, QueryTriggerInteraction.Ignore);

        if (sphere)
        {
            var angle = Vector3.Angle(rayData.normal, -rayToFloor.direction);
            //Debug.Log("angle:" + angle + " normal:" + rayData.normal);
            if (angle < 60)
            {
                AboveGround = true;
                distanceFromGround = rayData.distance;

                if (distanceFromGround > (0.1f + halfHeigth) && PlayerController.CurrentState != PlayerController.State.Ragdoll && PlayerController.CurrentState != PlayerController.State.Hooked)
                {
                    //transform.position = new Vector3(transform.position.x, rayData.point.y + halfHeigth, transform.position.z);
                    var targetPos = new Vector3(transform.position.x, rayData.point.y + halfHeigth, transform.position.z);
                    transform.position = Vector3.Lerp(transform.position, targetPos, 10 * Time.deltaTime);
                    OnGround = false;
                }
                else if (distanceFromGround <= (0.1f + halfHeigth))
                {
                    OnGround = true;
                }
            }
            else
            {
                OnGround = false;
                //boveGround = false;
                distanceFromGround = -1;
            }
        }
        else
        {
            OnGround = false;
            AboveGround = false;
            distanceFromGround = -1;
        }
        GroundedAndStuff = OnGround;
    }
}
