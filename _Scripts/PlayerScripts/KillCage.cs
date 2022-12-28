using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillCage : MonoBehaviour
{
    public GameObject ParticlePrefab;
    public bool DestroyOnEndOfRound = false;

    void PlayParticle(Vector3 pPos)
    {
        Vector3 dirToCenter = (-pPos).normalized;
        Vector2 viewportPoint = new Vector2(dirToCenter.x,dirToCenter.z);

        if (Mathf.Abs(viewportPoint.x) > Mathf.Abs(viewportPoint.y))
        {
            //print("Horizontal");
            viewportPoint.x = viewportPoint.x > 0 ? 0 : 1;
            viewportPoint.y = (((viewportPoint.y * -1) + 1) / 2f);
        }
        else
        {
            //print("Vertical");
            viewportPoint.y = viewportPoint.y > 0 ? 0 : 1;
            viewportPoint.x = (((viewportPoint.x * -1) + 1) / 2f) ;
        }
        var ray = Camera.main.ViewportPointToRay(viewportPoint);
        var plane = new Plane(Vector3.up,Vector3.zero);
        float enter = 0f;
        if (plane.Raycast(ray, out enter))
        {
            Vector3 point = ray.GetPoint(enter);
            //Debug.DrawRay(point, Vector3.up * 100, Color.green, 5f);
            var go = Instantiate(ParticlePrefab, point, Quaternion.LookRotation(-point));
            Destroy(go,3f);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (FindObjectOfType<SpaceStationGameRules>() != null || col.isTrigger)
        {
            return;
        }
        var death = col.GetComponent<Death>();
        if (death != null && !death.IsDead)
        {
            PlayParticle(death.transform.position);
            death.Die(null,DestroyOnEndOfRound);
            AudioManager.Instance.PlayEventWithParameter(AudioManager.Instance.PlayerSound.Death, Constants.FmodParameters.DeathIndex, 0, out EventInstance pEvent);
        }
        //col.GetComponent<Death>()?.Die(ParticlePrefab, DestroyOnEndOfRound);
    }
}
