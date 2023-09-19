using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

public class VRGlove : MonoBehaviour
{
    public float touchRadius = 0.001f;
    public float thermoRadius = 0.25f;
    public int thermoValue = 0;
    public int[] buzzerToggles = new int[] { 0, 0, 0, 0, 0 };

    public GameObject hand;

    // Start is called before the first frame update
    void Start()
    {
        XRHandSubsystem m_Subsystem =
            XRGeneralSettings.Instance?
                .Manager?
                .activeLoader?
                .GetLoadedSubsystem<XRHandSubsystem>();

        if (m_Subsystem != null)
            m_Subsystem.updatedHands += OnUpdatedHands;
    }

    void OnUpdatedHands(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
    {
        if (updateType != XRHandSubsystem.UpdateType.Dynamic) //only update on dynamic updates
            return;

        thermoValue = 0;

        var rightHand = subsystem.rightHand;
        if (!rightHand.isTracked) //only update when hand is being tracked
            return;

        var palmData = rightHand.GetJoint(XRHandJointID.Palm);
        if (palmData.TryGetPose(out UnityEngine.Pose palm))
        {
            Collider[] colliders = Physics.OverlapSphere(palm.position, thermoRadius);
            foreach (Collider collider in colliders)
            {
                var touchedObject = collider.gameObject;
                var objectToggle = touchedObject.GetComponent<ObjectToggles>();

                if(objectToggle == null)
                    continue;

                float distance = Vector3.Distance(palm.position, collider.transform.position);
                if (objectToggle.isHot)
                {
                    thermoValue += mapDistance(distance, 0, thermoRadius, 1000, 0);
                    Debug.Log("Close to a Hot Object!");
                    Debug.Log(thermoValue);
                }
                else if(objectToggle.isCold)
                {
                    thermoValue += mapDistance(distance, 0, thermoRadius, -1000, 0);
                    Debug.Log("Close to a Cold Object!");   
                    Debug.Log(thermoValue);
                }
            }

            XRHandJointID[] fingerTips = new XRHandJointID[] { XRHandJointID.IndexTip, XRHandJointID.MiddleTip, XRHandJointID.LittleTip, XRHandJointID.RingTip, XRHandJointID.ThumbTip };
            for (int i = 0; i < fingerTips.Length; i++)
            {
                var tipData = rightHand.GetJoint(fingerTips[i]);
                if (tipData.TryGetPose(out UnityEngine.Pose tip))
                {
                    if (Physics.CheckSphere(tip.position, touchRadius))
                    {
                        buzzerToggles[i] = 1;
                        Debug.Log("Touched Object with " + i);
                    }
                    else
                    {
                        buzzerToggles[i] = 0;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    int mapDistance(float distance, float minDistance, float maxDistance, float minValue, float maxValue)
    {
        if(distance < minDistance)
        {
            return (int)minValue;
        }

        if (distance > maxDistance)
        {
            return (int)maxValue;
        }

        return (int)Mathf.Lerp(minValue, maxValue, Mathf.InverseLerp(minDistance, maxDistance, distance));
    }
}


    /*
    Debug.Log("Right Hand Index Tip Pos:");
    Debug.Log(pose.position.x);
    Debug.Log(pose.position.y);
    Debug.Log(pose.position.z);

    if (Physics.CheckSphere(pose.position, touchRadius))
    {
        // The joint is touching an object
        Debug.Log("Is Touching");
    }
    */
