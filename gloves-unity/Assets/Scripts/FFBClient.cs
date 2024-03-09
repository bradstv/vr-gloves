using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class FFBClient : MonoBehaviour
{
    private FFBManager _ffbManager;
    private void Awake()
    {
        _ffbManager = GameObject.FindObjectOfType<FFBManager>();
    }

    private void OnHandHoverBegin(Hand hand)
    {
        Debug.Log("Received Hand hover event");
        SteamVR_Skeleton_Pose_Hand skeletonPoseHand;
        if (hand.handType == SteamVR_Input_Sources.LeftHand)
        {
            skeletonPoseHand = GetComponent<Interactable>().skeletonPoser.skeletonMainPose.leftHand;
        }
        else
        {
            skeletonPoseHand = GetComponent<Interactable>().skeletonPoser.skeletonMainPose.rightHand;
        }

        _ffbManager.SetForceFeedbackFromSkeleton(hand, skeletonPoseHand);
    }

    private void OnHandHoverEnd(Hand hand)
    {
        if (!hand.currentAttachedObject)
        {
            _ffbManager.RelaxForceFeedback(hand);
        }
    }

    private void OnAttachedToHand(Hand hand)
    {
        Debug.Log("Received Hand attached event");

        var objectToggle = GetComponent<ObjectToggles>();
        if (objectToggle == null)
            return;

        if(objectToggle.grabbedTemp > 0)
        {
            if (objectToggle.isHot)
            {
                _ffbManager.SetThermoFeedbackFromObject(hand, objectToggle.grabbedTemp);
            }
            else if (objectToggle.isCold)
            {
                _ffbManager.SetThermoFeedbackFromObject(hand, (short)-objectToggle.grabbedTemp);
            }
        }

        if(objectToggle.grabbedHaptics)
        {
            _ffbManager.SetHapticFeedbackFromObject(hand, objectToggle.hapticTime);
        }
    }

    private void OnDetachedFromHand(Hand hand)
    {
        Debug.Log("Received Hand detach event");

        var objectToggle = GetComponent<ObjectToggles>();
        if (objectToggle == null)
            return;

        if (objectToggle.grabbedTemp > 0)
        {
            _ffbManager.SetThermoFeedbackFromObject(hand, 0);
        }

        if (objectToggle.grabbedHaptics)
        {
            _ffbManager.SetHapticFeedbackFromObject(hand, 0);
        }
    }
}