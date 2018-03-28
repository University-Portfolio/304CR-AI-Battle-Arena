using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnImpact : MonoBehaviour
{
    public GameObject target;

    void OnCollisionEnter(Collision collision)
    {
        Destroy(target);
    }
}
