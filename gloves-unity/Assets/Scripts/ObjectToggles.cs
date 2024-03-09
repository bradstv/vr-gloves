using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectToggles : MonoBehaviour
{
    public bool isHot = false;
    public bool isCold = false;
    public short grabbedTemp = 0;
    public float radiusTemp = 0.0f; //max 2.0f
    public Vector3 radiusOffset = Vector3.zero;

    public bool radiusHaptics = false;
    public bool grabbedHaptics = false;
    public short hapticTime = 150;

    
    

    void OnDrawGizmosSelected()
    {
        if(radiusTemp > 0 && (isHot || isCold))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + radiusOffset, radiusTemp);
        }
    }
}

