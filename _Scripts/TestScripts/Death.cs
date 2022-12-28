using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : MonoBehaviour {
    //public bool hitScreen;
    public bool IsDead = false;
    public GameObject Respawn;
    private Vector3 RespawnPos;

    private float[] ScreenVals = { .2f, .8f };

    Vector2 screenpos;
	// Use this for initialization
	void Start () {
        if (Respawn == null) RespawnPos = gameObject.transform.position; else RespawnPos = Respawn.transform.position;
	}
	

	void Update () {

        //failsafe
        if (gameObject.transform.position.y < -100)
        {
            Die(null, false);
        }

        //if (IsDead && hitScreen)
        //{
        //    var target = Camera.main.transform.position + Camera.main.ViewportPointToRay(screenpos).direction * 6;
        //    transform.position = Vector3.MoveTowards(transform.position, target, 155 * Time.deltaTime);
        //    if (transform.position == target)
        //    {
        //        GetComponent<Hookable>().Push(Vector3.down, 10f);
        //        var go = Instantiate(GameManager.Instance.DestroyParticleEffect, transform.position, Quaternion.LookRotation(Camera.main.transform.up));
        //        Destroy(go, 2f);
        //        hitScreen = false;
        //    }
        //}
    }
    GameObject GameObject;
    public void Die(GameObject DeathParticleEffectPrefab, bool destroyOnEnd, Vector3? pos = null, bool pLocalParticleRotation = false, bool pAttachToPlayer = false)
    {
        if (IsDead)
        {
            return;
        }
        screenpos = new Vector2(ScreenVals[Random.Range(0, ScreenVals.Length)], ScreenVals[Random.Range(0, ScreenVals.Length)]);
        Destroy ( GetComponent<CharacterController>());
        //Vector3 hit = pos ?? transform.position;
        Quaternion rotation = pLocalParticleRotation ? transform.rotation : Quaternion.identity;
        IsDead = true;
        var pc = GetComponent<PlayerController>();
        GameManager.Instance.KillPlayer(pc, DeathParticleEffectPrefab,destroyOnEnd, pAttachToPlayer);
        pc.OnDeath();
    }
}