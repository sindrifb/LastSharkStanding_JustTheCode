using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillOnCollission : MonoBehaviour
{
    public bool TEMPFIX = true;
    public GameObject ParticlePrefab;
    public bool DestroyOnEndOfRound = false;
    private float SoundTimer = 0.5f;

    private void Update()
    {
        SoundTimer += Time.deltaTime;
    }

    void OnCollisionEnter(Collision col)
    {
        if (FindObjectOfType<SpaceStationGameRules>() != null || col.collider.isTrigger)
        {
            return;
        }
        if (TEMPFIX && col.collider.transform.position.x > -17)
        {
            return;
        }
        var death = col.collider.GetComponent<Death>();
        if (death != null && !death.IsDead)
        {
            death?.Die(null, DestroyOnEndOfRound, col.contacts[0].point);

            if (ParticlePrefab != null && death != null)
            {
                var contact = col.contacts[0];
                var ps = Instantiate(ParticlePrefab, contact.point, Quaternion.identity);
                ps.transform.up = -contact.normal;
                GameManager.Instance.SpawnedObjects.Add(ps);
            }

            if (SoundTimer >= 0.15f)
            {
                SoundTimer = 0;
                AudioManager.Instance.PlayEventWithParameter(AudioManager.Instance.PlayerSound.Death, Constants.FmodParameters.DeathIndex, 2, out FMOD.Studio.EventInstance pEvent);
            }
        }
    }
}
