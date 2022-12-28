using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstraintGetter : MonoBehaviour 
{
    public Transform Constraint
    {
        get { return transform; }
        private set { }
    }
}
