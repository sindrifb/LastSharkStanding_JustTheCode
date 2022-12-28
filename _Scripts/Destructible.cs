using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Destructible : MonoBehaviour 
{
    [EventRef]
    public string DestructableSound;
    [SerializeField]
    private GameObject DestroyedObject;
    [SerializeField]
    private GameObject IntactObject;
    [SerializeField]
    private float MinVelocityMagnitude = 3;
    private List<Vector3> OriginalPos = new List<Vector3>();
    private List<Quaternion> OriginalRot = new List<Quaternion>();
    private List<Transform> Pieces = new List<Transform>();
    private List<Transform> ResetPieces = new List<Transform>();
    private List<Transform> UnResetPieces = new List<Transform>();
    private bool IsResetting;
    private float LerpTimer = 0;
    private bool CanBeDestroyed = false;
    private Vector3 ParentOriginalPos = new Vector3();
    private Quaternion ParentOriginalRot = new Quaternion();
    private Rigidbody ParentRB;
    private Collider ParentCol;
    [SerializeField]
    private bool BreakIfTipped;
    [SerializeField]
    private float MinExplosionForce = 20;
    [SerializeField]
    private float MaxExplosionForce = 22;


    public bool IsDestroyed { get; private set; } = false;

    private void Awake()
    {
        ParentOriginalPos = transform.position;
        ParentOriginalRot = transform.rotation;

        foreach (Transform child in DestroyedObject.transform)
        {
            Pieces.Add(child);
        }

        foreach (Transform piece in Pieces)
        {
            OriginalPos.Add(piece.position);
            OriginalRot.Add(piece.rotation);
        }
        
    }

    private void Start()
    {
        ParentRB = GetComponent<Rigidbody>();
        ParentCol = GetComponent<Collider>();
        EventManager.StartListening(EventManager.EventCodes.GameEnd, ResetDestructible);
        EventManager.StartListening(EventManager.EventCodes.PlayersCleared, ResetDestructible);
        EventManager.StartListening(EventManager.EventCodes.RoundStart, ActivateDestructable);
    }

    private void Update()
    {
        if (IsResetting)
        {
            UpdateReset();
        }
        else
        {
            if (!IsDestroyed && CanBeDestroyed && BreakIfTipped)
            {
                DestroyOnTipped();
            }
        }
    }

    private void OnCollisionEnter(Collision col)
    {
        if (!IsDestroyed && CanBeDestroyed)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("SharkPhysicsCollider"))
            {
                //var charCtrl = col.gameObject.GetComponentInParent<CharacterController>();
                var movCtrl = col.gameObject.GetComponentInParent<MovementController>();
                if (/*charCtrl.velocity.magnitude >= MinVelocityMagnitude || */movCtrl.IsDashing)
                {
                    DestroyDestructible(col.transform.position);
                }
            }
            else if (GetComponent<Rigidbody>().velocity.magnitude >= MinVelocityMagnitude)
            {
                DestroyDestructible(col.contacts.Last().point);
            }
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsDestroyed && CanBeDestroyed)
        {
            if (other.gameObject.layer == 28)
            {
                DestroyDestructible(other.transform.position);
            }
        }
    }

    public void DestroyDestructible(Vector3 pExpPos)
    {
        IsDestroyed = true;
        ParentRB.isKinematic = true;
        ParentCol.enabled = false;
        IntactObject.SetActive(false);
        DestroyedObject.SetActive(true);
        AudioManager.Instance.PlayOneShot(DestructableSound);

        foreach (Rigidbody childRig in DestroyedObject.GetComponentsInChildren<Rigidbody>())
        {
            childRig.isKinematic = false;
            childRig.AddExplosionForce(Random.Range(MinExplosionForce ,MaxExplosionForce), pExpPos, 2, 1, ForceMode.Impulse);
        }
    }

    private void ResetDestructible()
    {
        ResetPieces.Clear();
        UnResetPieces = new List<Transform>(Pieces);
        foreach (var childRig in DestroyedObject.GetComponentsInChildren<Rigidbody>())
        {
            childRig.isKinematic = true;
            childRig.drag = 0;
            childRig.velocity = Vector3.zero;
        }
        LerpTimer = 0;
        //ParentRB.isKinematic = true;
        ParentRB.drag = 0;
        CanBeDestroyed = false;
        if (IsDestroyed)
        {
            transform.position = ParentOriginalPos;
            transform.rotation = ParentOriginalRot;
        }
        ParentRB.velocity = Vector3.zero;
        IsResetting = true;
    }

    private void UpdateReset()
    {
        LerpTimer += Time.deltaTime;

        if (!IsDestroyed)
        {
            transform.position = Vector3.Lerp(transform.position, ParentOriginalPos, Mathf.Clamp01(LerpTimer/1.5f));
            transform.rotation = Quaternion.Lerp(transform.rotation, ParentOriginalRot, Mathf.Clamp01(LerpTimer/1.5f));

            if (Vector3.Distance(transform.position, ParentOriginalPos) <= 0.1)
            {
                transform.position = ParentOriginalPos;
                transform.rotation = ParentOriginalRot;
                OnResetDone();
            }
        }
        else
        {
            if (ResetPieces.Count != Pieces.Count)
            {
                for (int i = 0; i < UnResetPieces.Count; i++)
                {
                    var index = Pieces.IndexOf(UnResetPieces[i]);
                    if (Pieces[index].position != OriginalPos[index])
                    {
                        Pieces[index].position = Vector3.Lerp(Pieces[index].position, OriginalPos[index], Mathf.Clamp01(LerpTimer));
                        Pieces[index].rotation = Quaternion.Lerp(Pieces[index].rotation, OriginalRot[index], Mathf.Clamp01(LerpTimer));

                        if (Vector3.Distance(Pieces[index].position, OriginalPos[index]) <= 0.1)
                        {
                            Pieces[index].position = OriginalPos[index];
                            Pieces[index].rotation = OriginalRot[index];
                            ResetPieces.Add(Pieces[index]);
                            UnResetPieces.RemoveAt(i);
                        }
                    }
                    else
                    {
                        ResetPieces.Add(Pieces[index]);
                        UnResetPieces.RemoveAt(i);
                    }
                }
            }
            else
            {
                OnResetDone();
            }
        }
    }

    private void OnResetDone()
    {
        DestroyedObject.SetActive(false);
        ParentCol.enabled = true;
        IntactObject.SetActive(true);
        IsDestroyed = false;
        IsResetting = false;
    }

    private void ActivateDestructable()
    {
        CanBeDestroyed = true;
        ParentRB.isKinematic = false;
    }

    private void DestroyOnTipped()
    {
        var angleBetween = Vector3.Angle(transform.up, Vector3.up);
        if (angleBetween >= 45)
        {
            DestroyDestructible(transform.position);
        }
    }
}
