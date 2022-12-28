using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseGun : MonoBehaviour
{
    public GameObject PulsePrefab;
    bool IsRotating;
    bool IsActive;
    Vector3 ActivePos;
    Vector3 InactivePos;
    int Shots = 0;

    Quaternion TargetRotation;
    // Start is called before the first frame update
    void Start()
    {
        ActivePos = transform.position;
        InactivePos = ActivePos + Vector3.down;
        OnRoundStart();
        EventManager.StartListening(EventManager.EventCodes.RoundEnd,OnRoundStart);
    }

    void OnRoundStart()
    {
        StartCoroutine(DeActivate(6));
    }

    IEnumerator Activate(float pStartDelay)
    {
        yield return new WaitForSeconds(pStartDelay);
        while (transform.position != ActivePos)
        {
            transform.position = Vector3.MoveTowards(transform.position,ActivePos,Time.deltaTime * 5);
            yield return null;
        }

        IsActive = true;
        yield return new WaitForSeconds(.5f);
        StartCoroutine(FireVolley());
    }

    IEnumerator DeActivate(float pRestartDelay = 0)
    {
        while (transform.position != InactivePos)
        {
            transform.position = Vector3.MoveTowards(transform.position, InactivePos, Time.deltaTime * 5);
            yield return null;
        }

        IsActive = false;
        Shots = Random.Range(7,13 + GameManager.Instance.Difficulty);
        StartCoroutine(Activate(pRestartDelay + Random.Range(4,7)));
    }

    IEnumerator FireVolley()
    {
        if (!IsActive)
        {
            yield break;
        }
        yield return new WaitForSeconds(.2f);
        if (!IsActive)
        {
            yield break;
        }
        for (int i = 0; i < 3; i++)
        {
            if (!IsActive)
            {
                yield break;
            }
            yield return new WaitForSeconds(.1f);
            Instantiate(PulsePrefab, transform.position, transform.rotation);
            var obj = Instantiate(PulsePrefab,transform.position, transform.rotation);
            obj.transform.forward = obj.transform.forward * -1;
        }

        Shots--;

        if (Shots <= 0)
        {
            StartCoroutine(DeActivate());
        }
        else if (Random.Range(1,3) < 2)
        {
            TargetRotation = Quaternion.AngleAxis(45, Vector3.up) * transform.rotation;
            IsRotating = true;
            IsActive = false;
        }
        else
        {
            yield return new WaitForSeconds(1f);
            StartCoroutine(FireVolley());
        }
    }

    void RotateTower()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, TargetRotation, 140f * Time.deltaTime);

        if (transform.rotation == TargetRotation)
        {
            IsActive = true;
            IsRotating = false;
            StartCoroutine(FireVolley());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsRotating)
        {
            RotateTower();
        }
    }
}
