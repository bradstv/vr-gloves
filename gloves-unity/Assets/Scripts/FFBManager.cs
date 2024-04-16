using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class FFBManager : MonoBehaviour
{
    private Interactable[] _interactables;
    
    private FFBProvider _ffbProviderLeft;
    private FFBProvider _ffbProviderRight;

    private short thermalValueLeft = 0;
    private short thermalValueRight = 0;

    private short[] fingerHapticsLeft = new short[] { -1, -1, -1, -1, -1 };
    private short[] fingerHapticsRight = new short[] { -1, -1, -1, -1, -1 };

    public SteamVR_Skeleton_Pose openHandReference;
    public SteamVR_Skeleton_Pose closeHandReference;

    //Whether to inject the FFBProvider script into all interactable game objects
    public bool injectFfbProvider = true;
    private void Awake()
    {
        _ffbProviderLeft = new FFBProvider(ETrackedControllerRole.LeftHand);
        _ffbProviderRight = new FFBProvider(ETrackedControllerRole.RightHand);

        if (injectFfbProvider)
        {
            _interactables = GameObject.FindObjectsOfType<Interactable>();

            foreach (Interactable interactable in _interactables)
            {
                interactable.gameObject.AddComponent<FFBClient>();
            }

            Debug.Log("Injected FFBClient into " + _interactables.Length + " Interactables");
        }
        
        if(!openHandReference)
        {
            openHandReference = ((SteamVR_Skeleton_Pose)Resources.Load("ReferencePose_OpenHand"));
        }

        if (!closeHandReference)
        {
            closeHandReference = ((SteamVR_Skeleton_Pose)Resources.Load("ReferencePose_Fist"));
        }
    }

    private void _SetForceFeedback(Hand hand, VRFFBInput input)
    {
        if (hand.handType == SteamVR_Input_Sources.LeftHand)
        {
            _ffbProviderLeft.SetFFB(input);
        }
        else
        {
            _ffbProviderRight.SetFFB(input);
        }

        Debug.Log("FFB: " + input.thumbCurl + ", " + input.indexCurl + ", " + input.middleCurl + ", " + input.ringCurl + ", " + input.pinkyCurl);
    }

    private void _SetThermalFeedback(Hand hand, VRTFBInput input)
    {
        if (hand.handType == SteamVR_Input_Sources.LeftHand)
        {
            _ffbProviderLeft.SetTFB(input);
        }
        else
        {
            _ffbProviderRight.SetTFB(input);
        }

        Debug.Log("TFB: " + input.value);
    }

    private void _SetHapticFeedback(Hand hand, VRHFBInput input)
    {
        if (hand.handType == SteamVR_Input_Sources.LeftHand)
        {
            _ffbProviderLeft.SetHFB(input);
        }
        else
        {
            _ffbProviderRight.SetHFB(input);
        }

        Debug.Log("HFB: " + input.thumbHaptic + ", " + input.indexHaptic + ", " + input.middleHaptic + ", " + input.ringHaptic + ", " + input.pinkyHaptic);
    }

    //This method (perhaps crudely) estimates the curl of each finger from a skeleton passed in in the skeleton poser.
    //This method is the default option for the FFBClient, which attaches itself to all interactables and calls this method when it receives a hover event.
    public void SetForceFeedbackFromSkeleton(Hand hand, SteamVR_Skeleton_Pose_Hand skeleton)
    {
        SteamVR_Skeleton_Pose_Hand openHand;
        SteamVR_Skeleton_Pose_Hand closedHand;

        if (hand.handType == SteamVR_Input_Sources.LeftHand)
        {
            openHand = openHandReference.leftHand;
            closedHand = closeHandReference.leftHand;
        }
        else
        {
            openHand = openHandReference.rightHand;
            closedHand = closeHandReference.rightHand;
        }

        List<float>[] fingerCurlValues = new List<float>[5];

        for (int i = 0; i < fingerCurlValues.Length; i++) 
            fingerCurlValues[i] = new List<float>();

        for (int boneIndex = 0; boneIndex < skeleton.bonePositions.Length; boneIndex++)
        {
            //get the finger for the current bone
            int finger = SteamVR_Skeleton_JointIndexes.GetFingerForBone(boneIndex);

            //max curl for free fingers
            SteamVR_Skeleton_FingerExtensionTypes extensionType = skeleton.GetMovementTypeForBone(boneIndex);
            if (extensionType == SteamVR_Skeleton_FingerExtensionTypes.Free && finger >= 0)
            {
                fingerCurlValues[finger].Add(1.0f);
                continue;
            }

            //calculate open hand angle to poser animation
            float openToPoser = Quaternion.Angle(openHand.boneRotations[boneIndex], skeleton.boneRotations[boneIndex]);

            //calculate angle from open to closed
            float openToClosed = Quaternion.Angle(openHand.boneRotations[boneIndex], closedHand.boneRotations[boneIndex]);

            //get the ratio between open to poser and open to closed
            float curl = Mathf.Clamp(openToPoser / openToClosed, 0.0f, 1.0f);

            if (!float.IsNaN(curl) && !float.IsInfinity(curl) && curl != 0 && finger >= 0)
            {
                //Add it to the list of bone angles for averaging later
                fingerCurlValues[finger].Add(curl);
            }
        }
        //0-1000 averages of the fingers
        short[] fingerCurlAverages = new short[5];

        for (int i = 0; i < 5; i++)
        {
            float enumerator = 0;
            for (int j = 0; j < fingerCurlValues[i].Count; j++)
            {
                enumerator += fingerCurlValues[i][j];           
            }

            //The value we to pass is where 0 is full movement flexibility, so invert.
            fingerCurlAverages[i] = Convert.ToInt16(1000 - (Mathf.FloorToInt(enumerator / fingerCurlValues[i].Count * 1000)));
        }

        _SetForceFeedback(hand, new VRFFBInput(fingerCurlAverages[0], fingerCurlAverages[1], fingerCurlAverages[2], fingerCurlAverages[3], fingerCurlAverages[4]));
    }

    public void SetThermalFeedbackFromSkeleton(Hand hand, SteamVR_Behaviour_Skeleton skeleton) 
    {
        short thermalValue = 0;
        Vector3 position = skeleton.GetBonePosition(0);
        Collider[] colliders = Physics.OverlapSphere(position, 2.5f);
        foreach (Collider collider in colliders)
        {
            var touchedObject = collider.gameObject;
            var objectToggle = touchedObject.GetComponent<ObjectToggles>();

            if (objectToggle == null)
            {
                objectToggle = touchedObject.GetComponentInParent<ObjectToggles>();
                if (objectToggle == null)
                    continue;
            }

            if (objectToggle.radiusTemp == 0 || objectToggle.grabbedTemp > 0)
                continue;

            float distance = Vector3.Distance(position, collider.transform.position + objectToggle.radiusOffset);
            if(distance > objectToggle.radiusTemp)
                continue;

            if (objectToggle.isHot)
            {
                thermalValue += mapDistance(distance, 0, objectToggle.radiusTemp, 1000, 0);
            }
            else if (objectToggle.isCold)
            {
                thermalValue += mapDistance(distance, 0, objectToggle.radiusTemp, -1000, 0);
            }
        }

        thermalValue = (short)Mathf.Clamp(thermalValue, -1000, 1000); //ensure final value is within thermal range

        if (hand.handType == SteamVR_Input_Sources.LeftHand)
        {
            if(thermalValue != thermalValueLeft)
            {
                _SetThermalFeedback(hand, new VRTFBInput(thermalValue));
                thermalValueLeft = thermalValue;
            }
        }
        else
        {
            if (thermalValue != thermalValueRight)
            {
                _SetThermalFeedback(hand, new VRTFBInput(thermalValue));
                thermalValueRight = thermalValue;
            }
        }
    }

    public void SetHapticFeedbackFromSkeleton(Hand hand, SteamVR_Behaviour_Skeleton skeleton)
    {
        short[] fingerHaptics = new short[5];
        bool anyFingerStartedTouching = false;
        HandSkeletonBone[] fingerTips = new HandSkeletonBone[] { HandSkeletonBone.eBone_Thumb3, HandSkeletonBone.eBone_IndexFinger4, HandSkeletonBone.eBone_MiddleFinger4, HandSkeletonBone.eBone_RingFinger4, HandSkeletonBone.eBone_PinkyFinger4 };
        for (int i = 0; i < fingerTips.Length; i++)
        {
            Vector3 position = skeleton.GetBonePosition((int)fingerTips[i]);
            Collider[] colliders = Physics.OverlapSphere(position, 0.02f);
            short hapticTime = -1;
            foreach (Collider collider in colliders)
            {
                var touchedObject = collider.gameObject;
                var objectToggle = touchedObject.GetComponent<ObjectToggles>();

                if (objectToggle == null)
                {
                    objectToggle = touchedObject.GetComponentInParent<ObjectToggles>();
                    if (objectToggle == null)
                        continue;
                }

                if (!objectToggle.radiusHaptics)
                    continue;

                hapticTime = objectToggle.hapticTime;
                break;
            }
            fingerHaptics[i] = hapticTime;

            if (hand.handType == SteamVR_Input_Sources.LeftHand)
            {
                if (fingerHaptics[i] != fingerHapticsLeft[i] && fingerHaptics[i] != -1)
                    anyFingerStartedTouching = true;

                fingerHapticsLeft[i] = fingerHaptics[i];
            }
            else
            {
                if (fingerHaptics[i] != fingerHapticsRight[i] && fingerHaptics[i] != -1)
                    anyFingerStartedTouching = true;

                fingerHapticsRight[i] = fingerHaptics[i];
            }
        }

        if(anyFingerStartedTouching)
        {
            _SetHapticFeedback(hand, new VRHFBInput(fingerHaptics[0], fingerHaptics[1], fingerHaptics[2], fingerHaptics[3], fingerHaptics[4]));
        }
    }

    public void SetForceFeedbackByCurl(Hand hand, VRFFBInput input)
    {
        _SetForceFeedback(hand, input);
    }

    public void RelaxForceFeedbackWithDelay(Hand hand, float delay)
    {
        StartCoroutine(InvokeAfterDelay(() => RelaxForceFeedback(hand), delay));
    }

    public void RelaxForceFeedback(Hand hand)
    {
        if(!hand.currentAttachedObject)
        {
            _SetForceFeedback(hand, new VRFFBInput(0, 0, 0, 0, 0));
        }   
    }

    public void SetThermalFeedbackFromObject(Hand hand, short thermalValue)
    {
        _SetThermalFeedback(hand, new VRTFBInput(thermalValue));
    }

    public void SetHapticFeedbackFromObject(Hand hand, short hapticTime)
    {
        _SetHapticFeedback(hand, new VRHFBInput(hapticTime, hapticTime, hapticTime, hapticTime, hapticTime));
    }

    private short mapDistance(float distance, float minDistance, float maxDistance, short minValue, short maxValue)
    {
        float mappedValue = (distance - minDistance) / (maxDistance - minDistance) * (maxValue - minValue) + minValue;
        return (short)Mathf.Clamp(mappedValue, Mathf.Min(minValue, maxValue), Mathf.Max(minValue, maxValue));
    }

    public enum HandSkeletonBone : int
    {
        eBone_Root = 0,
        eBone_Wrist,
        eBone_Thumb0,
        eBone_Thumb1,
        eBone_Thumb2,
        eBone_Thumb3,
        eBone_IndexFinger0,
        eBone_IndexFinger1,
        eBone_IndexFinger2,
        eBone_IndexFinger3,
        eBone_IndexFinger4,
        eBone_MiddleFinger0,
        eBone_MiddleFinger1,
        eBone_MiddleFinger2,
        eBone_MiddleFinger3,
        eBone_MiddleFinger4,
        eBone_RingFinger0,
        eBone_RingFinger1,
        eBone_RingFinger2,
        eBone_RingFinger3,
        eBone_RingFinger4,
        eBone_PinkyFinger0,
        eBone_PinkyFinger1,
        eBone_PinkyFinger2,
        eBone_PinkyFinger3,
        eBone_PinkyFinger4,
        eBone_Aux_Thumb,
        eBone_Aux_IndexFinger,
        eBone_Aux_MiddleFinger,
        eBone_Aux_RingFinger,
        eBone_Aux_PinkyFinger,
        eBone_Count
    }

    private IEnumerator InvokeAfterDelay(Action func, float delay)
    {
        yield return new WaitForSeconds(delay);
        func();
    }

    private void Stop()
    {
        _ffbProviderLeft.Close();
        _ffbProviderRight.Close();
    }
    private void OnApplicationQuit()
    {
        Stop();
    }

    private void OnDestroy()
    {
        Stop();
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct VRFFBInput
{
    //Curl goes between 0-1000
    public VRFFBInput(short thumbCurl, short indexCurl, short middleCurl, short ringCurl, short pinkyCurl)
    {
        this.thumbCurl = thumbCurl;
        this.indexCurl = indexCurl;
        this.middleCurl = middleCurl;
        this.ringCurl = ringCurl;
        this.pinkyCurl = pinkyCurl;
    }
    public short thumbCurl;
    public short indexCurl;
    public short middleCurl;
    public short ringCurl;
    public short pinkyCurl;
};

public struct VRTFBInput
{
    //Thermal goes between 0-1000
    public VRTFBInput(short value)
    {
        this.value = value;
    }
    public short value;
};

public struct VRHFBInput
{

    public VRHFBInput(short thumbHaptic, short indexHaptic, short middleHaptic, short ringHaptic, short pinkyHaptic)
    {
        this.thumbHaptic = thumbHaptic;
        this.indexHaptic = indexHaptic;
        this.middleHaptic = middleHaptic;
        this.ringHaptic = ringHaptic;
        this.pinkyHaptic = pinkyHaptic;
    }
    public short thumbHaptic;
    public short indexHaptic;
    public short middleHaptic;
    public short ringHaptic;
    public short pinkyHaptic;
};

class FFBProvider
{
    private NamedPipesProvider _namedPipeProvider;
    public ETrackedControllerRole controllerRole;
    
    public FFBProvider(ETrackedControllerRole controllerRole)
    {
        this.controllerRole = controllerRole;
        _namedPipeProvider = new NamedPipesProvider(controllerRole);
        
        _namedPipeProvider.Connect();
    }
   
    public bool SetFFB(VRFFBInput input)
    {
         return _namedPipeProvider.FFSend(input);
    }

    public bool SetTFB(VRTFBInput input)
    {
        return _namedPipeProvider.TFSend(input);
    }

    public bool SetHFB(VRHFBInput input)
    {
        return _namedPipeProvider.HFSend(input);
    }

    public void Close()
    {
        _namedPipeProvider.Disconnect();
    }
}

class NamedPipesProvider
{
    private NamedPipeClientStream forcePipe;
    private NamedPipeClientStream thermalPipe;
    private NamedPipeClientStream hapticPipe;
    private ETrackedControllerRole pipeControllerRole;
    public NamedPipesProvider(ETrackedControllerRole controllerRole)
    {
        pipeControllerRole = controllerRole;
        forcePipe = new NamedPipeClientStream("vrapplication/ffb/curl/" + (controllerRole == ETrackedControllerRole.RightHand ? "right" : "left"));
        thermalPipe = new NamedPipeClientStream("vrapplication/ffb/thermal/" + (controllerRole == ETrackedControllerRole.RightHand ? "right" : "left"));
        hapticPipe = new NamedPipeClientStream("vrapplication/ffb/haptic/" + (controllerRole == ETrackedControllerRole.RightHand ? "right" : "left"));
    }

    public void Connect()
    {
        try
        {
            forcePipe.Connect();   
            Debug.Log("Successfully connected to " + (pipeControllerRole == ETrackedControllerRole.RightHand ? "right" : "left") + " pipe: curl");
        }
        catch (Exception e)
        {
            Debug.Log("Unable to connect to " + (pipeControllerRole == ETrackedControllerRole.RightHand ? "right" : "left") + " curl pipe. Error: " + e);
        }

        try
        {
            thermalPipe.Connect();
            Debug.Log("Successfully connected to " + (pipeControllerRole == ETrackedControllerRole.RightHand ? "right" : "left") + " pipe: thermal");
        }
        catch (Exception e)
        {
            Debug.Log("Unable to connect to " + (pipeControllerRole == ETrackedControllerRole.RightHand ? "right" : "left") + " thermal pipe. Error: " + e);
        }

        try
        {
            hapticPipe.Connect();
            Debug.Log("Successfully connected to " + (pipeControllerRole == ETrackedControllerRole.RightHand ? "right" : "left") + " pipe: haptic");
        }
        catch (Exception e)
        {
            Debug.Log("Unable to connect to " + (pipeControllerRole == ETrackedControllerRole.RightHand ? "right" : "left") + " haptic pipe. Error: " + e);
        }

    }

    public void Disconnect()
    {
        if (forcePipe.IsConnected)
        {
            forcePipe.Dispose();
        }

        if (thermalPipe.IsConnected)
        {
            thermalPipe.Dispose();
        }

        if (hapticPipe.IsConnected)
        {
            hapticPipe.Dispose();
        }
    }

    public bool FFSend(VRFFBInput input)
    {
        if (forcePipe.IsConnected)
        {
            //Debug.Log("running task");
            int size = Marshal.SizeOf(input);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(input, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            forcePipe.Write(arr, 0, size);

            //Debug.Log("Sent force feedback message.");

            return true;
        }

        return false;
    }

    public bool TFSend(VRTFBInput input)
    {
        if (thermalPipe.IsConnected)
        {
            //Debug.Log("running task");
            int size = Marshal.SizeOf(input);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(input, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            thermalPipe.Write(arr, 0, size);

            //Debug.Log("Sent thermal feedback message.");

            return true;
        }

        return false;
    }

    public bool HFSend(VRHFBInput input)
    {
        if (hapticPipe.IsConnected)
        {
            //Debug.Log("running task");
            int size = Marshal.SizeOf(input);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(input, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            hapticPipe.Write(arr, 0, size);

            //Debug.Log("Sent haptic feedback message.");

            return true;
        }

        return false;
    }
}

