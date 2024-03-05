using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UIElements;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class onHandUpdate : MonoBehaviour
{
    private Hand hand;
    private SteamVR_Behaviour_Skeleton skeleton;
    private bool isSubscribed = false;
    private FFBManager _ffbManager;
    private void Awake()
    {
        hand = GetComponent<Hand>();
        _ffbManager = GameObject.FindObjectOfType<FFBManager>();
    }

    private void FixedUpdate()
    {
        if (isSubscribed)
        {
            return;
        }

        if (!isSubscribed && hand != null && hand.skeleton != null)
        {
            skeleton = hand.skeleton;
            skeleton.onBoneTransformsUpdatedEvent += OnTransformsUpdated;
            isSubscribed = true;
            Debug.Log("Subscribed");
        }
    }

    private void OnTransformsUpdated(SteamVR_Behaviour_Skeleton skeleton, SteamVR_Input_Sources inputSource)
    {
        _ffbManager.SetThermoFeedbackFromSkeleton(hand, skeleton);

        //only set haptics from hand updates if not holding an object
        if (hand.currentAttachedObject == null)
        {
            _ffbManager.SetHapticFeedbackFromSkeleton(hand, skeleton);
        }

       
    }
}


