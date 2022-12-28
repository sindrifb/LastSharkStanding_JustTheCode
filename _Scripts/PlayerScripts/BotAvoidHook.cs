using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BotAvoidHook : MonoBehaviour
{
    private BotController Ai;
    private UsableController UsableController;
    // Start is called before the first frame update
    void Start()
    {
        Ai = GetComponentInParent<BotController>();
        UsableController = GetComponentInParent<UsableController>();
        
    }

    private void OnTriggerEnter(Collider other)
    {
        var usable = other.GetComponent<Usable>();
        if (usable != null)
        {
            if (usable != UsableController.CurrentUsable && usable.IsActive && usable != Ai.MyThrownUsable)
            {
                ObjToAvoid add = new ObjToAvoid(usable, false);
                Ai.ObjectsToAvoid.Add(add);
            }
        }
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    var usable = other.GetComponent<Usable>();
    //    if (usable != null)
    //    {
    //        if (Ai.ObjectsToAvoid.FirstOrDefault(a => a.usable == usable) != null)
    //        {
    //            return;
    //        }
    //        if (usable != UsableController.CurrentUsable && usable.IsActive && usable != Ai.MyThrownUsable)
    //        {
    //            ObjToAvoid add = new ObjToAvoid(usable, false);
    //            Ai.ObjectsToAvoid.Add(add);
    //        }
    //    }
    //}
}
