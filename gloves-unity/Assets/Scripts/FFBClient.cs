using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class FFBClient : MonoBehaviour
{
    private FFBManager _ffbManager;
    private ObjectToggles objectToggle;
    private void Awake()
    {
        _ffbManager = GameObject.FindObjectOfType<FFBManager>();
        objectToggle = GetComponent<ObjectToggles>();
    }

    private void OnAttachedToHand(Hand hand)
    {
        Debug.Log("Received Hand attached event");

        FFBOnAttach(hand);
        TFBOnAttach(hand);
        HFBOnAttach(hand);
    }

    private void OnHandHoverEnd(Hand hand)
    {
        /*
        if (!hand.currentAttachedObject)
        {
            FFBOnDetach(hand);
        }
        */
    }

    private void OnDetachedFromHand(Hand hand)
    {
        Debug.Log("Received Hand detach event");

        FFBOnDetach(hand);
        TFBOnDetach(hand);
        HFBOnDetach(hand);
    }

    private void FFBOnAttach(Hand hand)
    {
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

    private void TFBOnAttach(Hand hand)
    {
        if (objectToggle == null)
            return;

        if (objectToggle.grabbedTemp > 0)
        {
            if (objectToggle.isHot)
            {
                _ffbManager.SetThermalFeedbackFromObject(hand, objectToggle.grabbedTemp);
            }
            else if (objectToggle.isCold)
            {
                _ffbManager.SetThermalFeedbackFromObject(hand, (short)-objectToggle.grabbedTemp);
            }
        }
    }

    private void HFBOnAttach(Hand hand)
    {
        if (objectToggle == null)
            return;

        if (objectToggle.grabbedHaptics)
        {
            _ffbManager.SetHapticFeedbackFromObject(hand, objectToggle.hapticTime);
        }
    }

    private void FFBOnDetach(Hand hand)
    {
        _ffbManager.RelaxForceFeedback(hand);
    }

    private void TFBOnDetach(Hand hand)
    {
        if (objectToggle == null)
            return;

        if (objectToggle.grabbedTemp > 0)
        {
            _ffbManager.SetThermalFeedbackFromObject(hand, 0);
        }
    }

    private void HFBOnDetach(Hand hand)
    {
        //do nothing for haptics
    }
}