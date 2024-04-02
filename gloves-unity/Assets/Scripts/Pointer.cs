using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

public class Pointer : MonoBehaviour
{
    public float defaultLength = 5.0f;
    public GameObject dot;
    public VRInputModule inputModule;
    private LineRenderer lineRenderer;
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        updateLine();
    }

    private void updateLine()
    {
        PointerEventData data = inputModule.GetData();
        float targetLength = data.pointerCurrentRaycast.distance == 0 ? defaultLength : data.pointerCurrentRaycast.distance;
        RaycastHit hit = createRaycast(targetLength);

        Vector3 endPos = transform.position + (transform.forward * targetLength);

        if(hit.collider != null)
        {
            endPos = hit.point;
        }

        dot.transform.position = endPos;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, endPos);
    }

    private RaycastHit createRaycast(float length)
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);
        Physics.Raycast(ray, out hit, defaultLength);

        return hit;
    }
}
