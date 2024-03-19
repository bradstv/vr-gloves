using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UIElements;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class onHandUpdate : MonoBehaviour
{
    public float thermalWaitTime = 1.0f;
    public float hapticWaitTime = 0.5f;

    private Hand hand;
    private SteamVR_Behaviour_Skeleton skeleton;
    private bool isSubscribed = false;
    private FFBManager _ffbManager;

    private float thermalTimer = 0.0f;
    private float hapticTimer = 0.0f;
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
            Debug.Log("Subscribed to Hand Updates");
        }
    }

    private void OnTransformsUpdated(SteamVR_Behaviour_Skeleton skeleton, SteamVR_Input_Sources inputSource)
    {
        //only set radius thermal and haptics after specified amount of time (prevents spamming driver with fb packets)
        if (Time.time > thermalTimer + thermalWaitTime)
        {
            _ffbManager.SetThermalFeedbackFromSkeleton(hand, skeleton);
            thermalTimer = Time.time;
        }

        //only set haptics if not holding an object and after specified time
        if (hand.currentAttachedObject == null && Time.time > hapticTimer + hapticWaitTime)
        {
            _ffbManager.SetHapticFeedbackFromSkeleton(hand, skeleton);
            hapticTimer = Time.time;
        }  
    }
}


