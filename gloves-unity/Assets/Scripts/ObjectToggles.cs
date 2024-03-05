using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectToggles : MonoBehaviour
{
    public bool isHot = false;
    public bool isCold = false;
    public bool triggerHaptics = false;
    public short hapticTime = 1000;
    public float heatRadius = 0.25f;
    public Vector3 radiusOffset = Vector3.zero;

    void OnDrawGizmosSelected()
    {
        if(isHot || isCold)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + radiusOffset, heatRadius);
        }
    }
}

