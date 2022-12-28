using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour {
    public List<AudioClip> HookThrow;
    public List<AudioClip> HookFly;
    public List<AudioClip> HookHit;
    public List<AudioClip> GetHit;
    public List<AudioClip> StandUp;
    public List<AudioClip> FallOff;
    public List<AudioClip> Dash;
    public List<AudioClip> Fling;
    public List<AudioClip> Land;
    public List<AudioClip> PowerUpPickup;
    public List<AudioClip> AnchorHit;
    public List<AudioClip> Spawn;
    private AudioSource AudioSource;
    // Use this for initialization
    void Awake () {
        AudioSource = GetComponent<AudioSource>();
	}

    private AudioClip FindRandomSound(List<AudioClip> pList)
    {
        return pList[Random.Range(0, pList.Count)];
    }

    public void PlayAnchorhit()
    {
        PlaySound(AnchorHit);
    }

    public void PlaySpawn()
    {
        PlaySound(Spawn);
    }

    public void PlayHookThrow()
    {
        PlaySound(HookThrow);
    }
    public void PlayHookFly()
    {
        PlaySound(HookFly);
    }
    public void PlayHookHit()
    {
        PlaySound(HookHit);
    }
    public void PlayLand()
    {
        PlaySound(Land);

    }
    public void PlayGetHit()
    {
        PlaySound(GetHit);
    }
    public void PlayStandUp()
    {
        PlaySound(StandUp);
    }
    public void PlayFallOff()
    {
        PlaySound(FallOff);
    }
    public void PlayDash()
    {
        PlaySound(Dash);
    }
    public void PlayFling()
    {
        PlaySound(Fling);
    }
    
    public void PlayPowerUpPickup()
    {
        PlaySound(PowerUpPickup);
    }

    private void PlaySound(List <AudioClip> pList)
    {
        if (pList == null || pList.Count == 0)
        {
            //print("No sounds to play");
            return;
        }

        AudioSource.PlayOneShot(FindRandomSound(pList));
    }
}
