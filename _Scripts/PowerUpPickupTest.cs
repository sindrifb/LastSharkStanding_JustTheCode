using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PowerUpPickupTest : MonoBehaviour
{
    private GameObject UsableObject;
    private bool HasBeenPickedUp;
    private Usable.UsableType PowerupType;

    private void Start()
    {
        HasBeenPickedUp = false;
        var usable = GetComponentInChildren<Usable>(true);
        UsableObject = usable.gameObject;
        usable.DisplayObject.SetActive(false);
        PowerupType = UsableObject.GetComponent<Usable>().Type;

        if (usable.ThrownParticlesParent != null)
        {
            usable.ThrownParticlesParent?.SetActive(false);
        }
        StartCoroutine(CheckIfOutOfBounds());
    }

    private IEnumerator CheckIfOutOfBounds()
    {
        while (true)
        {
            if (transform.position.y <= -5)
            {
                DeletePowerUp();
            }
            yield return new WaitForSeconds(10);
        }
    }

    public void TimedDelete(float pTime)
    {
        Invoke("DeletePowerup", pTime);
    }

    private void DeletePowerUp()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!HasBeenPickedUp)
        {
            var player = other.gameObject.GetComponent<UsableController>();
            if ((player != null && player.GetComponent<PlayerController>().CurrentState == PlayerController.State.Idle) || (player != null && player.GetComponent<PlayerController>().CurrentState == PlayerController.State.Staggered))
            {
                //if (player.CurrentUsable == player.StandardHook)
                //{
                var usable = GetComponent<Hookable>().AttachedUsable;
                //if (usable != null && usable.UsableController != null && usable.UsableController != player)
                //{
                //    usable.Interrupt(usable.PlayerController.CurrentState);
                //}

                GivePowerUp(player);

                //}
            }
        }
    }

    public void GivePowerUp(UsableController pUsableController)
    {
        if ((pUsableController != null && pUsableController.GetComponent<PlayerController>().CurrentState == PlayerController.State.Idle) || (pUsableController != null && pUsableController.GetComponent<PlayerController>().CurrentState == PlayerController.State.Staggered))
        {
            if (!HasBeenPickedUp)
            {
                HasBeenPickedUp = true;
                transform.rotation = Quaternion.identity;
                pUsableController.PickUpPowerUp(UsableObject);
                if (pUsableController.PlayerController.IsBot)
                {
                    pUsableController.PlayerController.AiShark.OnPowerupPickUp();
                }
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.PowerupSound.PowerupPickup);
                //if any other usable is attached to the game object (like the hook or harpoon) unparent so they don't get deleted
                var intruder = GetComponentsInChildren<Usable>().ToList().Where(a => a.gameObject != UsableObject).ToList();
                intruder.ForEach(a => a.transform.parent = null);

                PowerupPickupEvent powerupPickupEvent = new PowerupPickupEvent
                {
                    Description = UsableObject.name,
                    RewiredID = pUsableController.PlayerController.RewiredID,
                    Usable = PowerupType
                };
                powerupPickupEvent.FireEvent();

                DeletePowerUp();
            }
        }
    }
}
