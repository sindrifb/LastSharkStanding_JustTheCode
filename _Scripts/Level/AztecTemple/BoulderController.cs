using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderController : MonoBehaviour
{
    public GameObject GameObjectBreak;
    public GameObject LeftButton;
    public GameObject RightButton;
    public GameObject ButtonPushParticles;
    public BoxCollider[] ButtonColliders;
    private GameObject BrokenBoulder;
    private Rigidbody[] BoulderShards;
    private List<Vector3> ShardOriginalPos = new List<Vector3>();

    public List<Renderer> RenderersToSwapColor;
    public Color DisabledColor;

    int dir = 1;
    public float MoveForce;
    public float LineForce;
    public List<Transform> Targets;
    public Rigidbody Rigidbody;
    public bool move;
    private bool disabled = false;
    private Transform CurTarget;
    private Transform PrevTarget;
    private Animator LeftButtonAnimator;
    private Animator RightButtonAnimator;
    private Coroutine ResetCoroutine;
    private BoulderPush BoulderPush;
    // Start is called before the first frame update
    void Start()
    {
        BoulderPush = GetComponentInChildren<BoulderPush>();
        LeftButtonAnimator = LeftButton.GetComponentInChildren<Animator>();
        RightButtonAnimator = RightButton.GetComponentInChildren<Animator>();
        CurTarget = Targets[Targets.Count - 1];
        Rigidbody.isKinematic = true;
        BoulderPush.IsActive = false;
        PrepareBrokenBoulder();
    }

    // Update is called once per frame
    void Update()
    {
        if (move)
        {
            if (Rigidbody.transform.position.y <= -6f)
            {
                Crash();
            }
            //prevIndex = prevIndex < 0 ? Targets.Count - 1 : prevIndex;
            //prevIndex = prevIndex > Targets.Count - 1 ? 0 : prevIndex;

            Vector3 dirToNextPoint = CurTarget.position - PrevTarget.position;

            Rigidbody.AddForce((CurTarget.position - Rigidbody.transform.position).normalized * Time.deltaTime * MoveForce);

            var proj = (PrevTarget.position + Vector3.Project(Rigidbody.position - PrevTarget.position, dirToNextPoint)) - Rigidbody.transform.position;
            
            Rigidbody.AddForce(proj * Time.deltaTime * LineForce);
            CurrentMoveDir = dirToNextPoint.normalized;
            if ((Rigidbody.transform.position - CurTarget.transform.position).sqrMagnitude < 4f)
            {
                PrevTarget = CurTarget;
                int index = Targets.IndexOf(PrevTarget) + dir;
                if (index >= Targets.Count || index < 0)
                {
                    index = index < 0 ? Targets.Count - 1 : 0;
                }
                CurTarget = Targets[index];
            }
        }
    }

    Vector3 CurrentMoveDir = Vector3.zero;
    public void Crash(Vector3? pDir = null, float pForce = 0)
    {
        var vel = pDir != null ? pDir.Value * pForce : CurrentMoveDir * 100;
        //var GO = Instantiate(GameObjectBreak, Rigidbody.transform.position, Rigidbody.transform.rotation);
        //Destroy(GO, 4f);
        //var rigidbodies = GO.GetComponentsInChildren<Rigidbody>();
        //foreach (Rigidbody shard in rigidbodies)
        //{
        //    shard.isKinematic = false;
        //    shard.AddForce(vel, ForceMode.Impulse);
        //}
        StartCoroutine(BreakBoulder(vel));
        move = false;
        BoulderPush.IsActive = false;
        disabled = true;
        StartCoroutine(Reset());
        if (ResetCoroutine != null)
        {
            StopCoroutine(ResetCoroutine);
            ResetCoroutine = null;
        }
    }

    private void PrepareBrokenBoulder()
    {
        BrokenBoulder?.SetActive(false);
        if (BrokenBoulder == null)
        {
            BrokenBoulder = Instantiate(GameObjectBreak, Rigidbody.transform.position, Rigidbody.transform.rotation);
            BrokenBoulder.SetActive(false);
            BoulderShards = BrokenBoulder.GetComponentsInChildren<Rigidbody>();
            foreach (var shard in BoulderShards)
            {
                ShardOriginalPos.Add(shard.transform.localPosition);
            }
        }
        else
        {
            for (int i = 0; i < BoulderShards.Length; i++)
            {
                BoulderShards[i].transform.localPosition = ShardOriginalPos[i];
                BoulderShards[i].velocity = Vector3.zero;
            }
        }
    }

    private IEnumerator BreakBoulder(Vector3 pVelocity)
    {
        BrokenBoulder.transform.position = Rigidbody.position;
        BrokenBoulder.transform.rotation = Rigidbody.rotation;
        BrokenBoulder.SetActive(true);

        foreach (Rigidbody shard in BoulderShards)
        {
            shard.isKinematic = false;
            shard.AddForce(pVelocity, ForceMode.Impulse);
        }
        yield return new WaitForSeconds(4f);
        BrokenBoulder.SetActive(false);
        PrepareBrokenBoulder();
    }

    private IEnumerator Reset()
    {
        //yield return new WaitForSeconds(2f);
        //CurTarget = Targets[Targets.Count - 1];
        Rigidbody.gameObject.SetActive(false);
        Rigidbody.gameObject.transform.position = Targets[Targets.Count - 1].position;
        Rigidbody.isKinematic = true;
        Rigidbody.velocity = Vector3.zero;
        yield return new WaitForSeconds(4f);
        Rigidbody.gameObject.SetActive(true);
        disabled = false;
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.HazardSound.BeachBallLand);
        LeftButtonAnimator.SetTrigger("play");
        RightButtonAnimator.SetTrigger("play");
    }

   private IEnumerator TempSwapColor(Renderer rend)
    {
        var col = rend.materials[2].color;
        rend.materials[2].color = DisabledColor;
        while (move)
        {
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(4);
        rend.materials[2].color = col;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.GetComponent<PlayerController>() != null && !disabled && !move && Rigidbody.gameObject.activeInHierarchy)
        {
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.EnvironmentSound.StoneScrape);
            dir = System.Math.Sign(Vector3.Dot(Vector3.right, col.transform.position));
            Rigidbody.isKinematic = false;
            CurTarget = dir < 0 ? Targets[Targets.Count - 2] : Targets[0];
            PrevTarget = Targets[Targets.Count - 1];
            move = true;
            BoulderPush.IsActive = true;

            ResetCoroutine = StartCoroutine(MaxTimeReset());
            
            if (dir < 0)
            {
                LeftButtonAnimator.SetTrigger("play");
                PushButtonParticles(LeftButton.transform.position);
                StartCoroutine(LateDisable(RightButtonAnimator, false));
            }
            else
            {
                RightButtonAnimator.SetTrigger("play");
                PushButtonParticles(RightButton.transform.position);
                StartCoroutine(LateDisable(LeftButtonAnimator, true));
            }
        }
    }

    private void PushButtonParticles(Vector3 pPos)
    {
        if (ButtonPushParticles != null)
        {
            Instantiate(ButtonPushParticles, pPos, Quaternion.identity);
        }
    }

    IEnumerator MaxTimeReset()
    {
        yield return new WaitForSeconds(10f);
        if (move)
        {
            Crash();
        }
    }

    IEnumerator LateDisable(Animator animator, bool LeftSide)
    {
        foreach (var item in RenderersToSwapColor)
        {
            StartCoroutine(TempSwapColor(item));
        }

        yield return new WaitForSeconds(1f);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.EnvironmentSound.StoneScrape);
        animator.SetTrigger("play");
    }
}
