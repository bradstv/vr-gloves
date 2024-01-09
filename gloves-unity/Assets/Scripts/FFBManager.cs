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
using static UnityEditor.PlayerSettings;

public class FFBManager : MonoBehaviour
{
    private Interactable[] _interactables;
    
    private FFBProvider _ffbProviderLeft;
    private FFBProvider _ffbProviderRight;

    private Coroutine hapticCoroutineLeft;
    private Coroutine hapticCoroutineRight;

    private short thermoValueLeft = 0;
    private short thermoValueRight = 0;

    private short[] fingerHapticsLeft = new short[] { 0, 0, 0, 0, 0 };
    private short[] fingerHapticsRight = new short[] { 0, 0, 0, 0, 0 };

    public float thermoLoopTime = 1.0f;

    //Whether to inject the FFBProvider script into all interactable game objects
    public bool injectFfbProvider = true;
    private void Awake()
    {
        _ffbProviderLeft = new FFBProvider(ETrackedControllerRole.LeftHand);
        _ffbProviderRight = new FFBProvider(ETrackedControllerRole.RightHand);
        StartCoroutine(thermoThread(ETrackedControllerRole.LeftHand));
        StartCoroutine(thermoThread(ETrackedControllerRole.RightHand));

        if (injectFfbProvider)
        {
            _interactables = GameObject.FindObjectsOfType<Interactable>();

            foreach (Interactable interactable in _interactables)
            {
                interactable.gameObject.AddComponent<FFBClient>();
            }
        }
        
        Debug.Log("Found: " + _interactables.Length + " Interactables");
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
    }

    private void _SetThermoFeedback(ETrackedControllerRole controllerRole, VRTFBInput input)
    {
        if (controllerRole == ETrackedControllerRole.LeftHand)
        {
            _ffbProviderLeft.SetTFB(input);
        }
        else
        {
            _ffbProviderRight.SetTFB(input);
        }
    }

    private void _SetHapticFeedback(ETrackedControllerRole controllerRole, VRHFBInput input)
    {
        if (controllerRole == ETrackedControllerRole.LeftHand)
        {
            _ffbProviderLeft.SetHFB(input);
        }
        else
        {
            _ffbProviderRight.SetHFB(input);
        }
    }

    private short getThermoValueForHand(ETrackedControllerRole controllerRole)
    {
        if (controllerRole == ETrackedControllerRole.LeftHand)
        {
            return thermoValueLeft;
        }
        else
        {
            return thermoValueRight;
        }
    }

    private IEnumerator thermoThread(ETrackedControllerRole controllerRole)
    {
        short currentValue = 0;
        while (true)
        {
            short thermoValue = getThermoValueForHand(controllerRole);
            if(currentValue != thermoValue)
            {
                _SetThermoFeedback(controllerRole, new VRTFBInput(thermoValue));
                currentValue = thermoValue;
            }
            yield return new WaitForSeconds(thermoLoopTime);
        }
    }

    //This method (perhaps crudely) estimates the curl of each finger from a skeleton passed in in the skeleton poser.
    //This method is the default option for the FFBClient, which attaches itself to all interactables and calls this method when it receives a hover event.
    public void SetForceFeedbackFromSkeleton(Hand hand, SteamVR_Skeleton_Pose_Hand skeleton)
    {
        SteamVR_Skeleton_Pose_Hand openHand;
        SteamVR_Skeleton_Pose_Hand closedHand;

        if (hand.handType == SteamVR_Input_Sources.LeftHand)
        {
            openHand = ((SteamVR_Skeleton_Pose)Resources.Load("ReferencePose_OpenHand")).leftHand;
            closedHand = ((SteamVR_Skeleton_Pose)Resources.Load("ReferencePose_Fist")).leftHand;
        }
        else
        {
            openHand = ((SteamVR_Skeleton_Pose)Resources.Load("ReferencePose_OpenHand")).rightHand;
            closedHand = ((SteamVR_Skeleton_Pose)Resources.Load("ReferencePose_Fist")).rightHand;
        }

        List<float>[] fingerCurlValues = new List<float>[5];

        for (int i = 0; i < fingerCurlValues.Length; i++) 
            fingerCurlValues[i] = new List<float>();

        for (int boneIndex = 0; boneIndex < skeleton.bonePositions.Length; boneIndex++)
        {
            //calculate open hand angle to poser animation
            float openToPoser = Quaternion.Angle(openHand.boneRotations[boneIndex], skeleton.boneRotations[boneIndex]);

            //calculate angle from open to closed
            float openToClosed =
                Quaternion.Angle(openHand.boneRotations[boneIndex], closedHand.boneRotations[boneIndex]);

            //get the ratio between open to poser and open to closed
            float curl = openToPoser / openToClosed;

            //get the finger for the current bone
            int finger = SteamVR_Skeleton_JointIndexes.GetFingerForBone(boneIndex);

            if (!float.IsNaN(curl) && curl != 0 && finger >= 0)
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

            //Debug.Log(fingerCurlAverages[i]);
        }

        _SetForceFeedback(hand, new VRFFBInput(fingerCurlAverages[0], fingerCurlAverages[1], fingerCurlAverages[2], fingerCurlAverages[3], fingerCurlAverages[4]));
    }

    public void RelaxForceFeedback(Hand hand)
    {
        VRFFBInput input = new VRFFBInput(0, 0, 0, 0, 0);
        _SetForceFeedback(hand, input);
    }

    public void SetForceFeedbackByCurl(Hand hand, VRFFBInput input)
    {
        _SetForceFeedback(hand, input);
    }

    public void SetThermoFeedbackFromSkeleton(Hand hand, SteamVR_Behaviour_Skeleton skeleton) 
    {
        short tempThermoValue = 0;
        Vector3 position = skeleton.GetBonePosition(0);
        Collider[] colliders = Physics.OverlapSphere(position, 0.5f);
        foreach (Collider collider in colliders)
        {
            var touchedObject = collider.gameObject;
            var objectToggle = touchedObject.GetComponent<ObjectToggles>();

            if (objectToggle == null)
                continue;

            float distance = Vector3.Distance(position, collider.transform.position);

            if(distance > objectToggle.heatRadius)
                continue;

            if (objectToggle.isHot)
            {
                tempThermoValue += mapDistance(distance, 0, objectToggle.heatRadius, 1000, 0);
                Debug.Log("Close to a Hot Object!");
                Debug.Log(tempThermoValue);
            }
            else if (objectToggle.isCold)
            {
                tempThermoValue += mapDistance(distance, 0, objectToggle.heatRadius, -1000, 0);
                Debug.Log("Close to a Cold Object!");
                Debug.Log(tempThermoValue);
            }
        }

        if (hand.handType == SteamVR_Input_Sources.LeftHand)
        {
            thermoValueLeft = tempThermoValue;
        }
        else
        {
            thermoValueRight = tempThermoValue;
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
            Collider[] colliders = Physics.OverlapSphere(position, 0.05f);
            short hapticTime = 0;
            foreach (Collider collider in colliders)
            {
                var touchedObject = collider.gameObject;
                var objectToggle = touchedObject.GetComponent<ObjectToggles>();

                if (objectToggle == null)
                    continue;

                if (!objectToggle.triggerHaptics)
                    continue;

                hapticTime = objectToggle.hapticTime;
                break;
            }
            fingerHaptics[i] = hapticTime;

            if (hand.handType == SteamVR_Input_Sources.LeftHand)
            {
                if (fingerHaptics[i] != fingerHapticsLeft[i] && fingerHaptics[i] != 0)
                    anyFingerStartedTouching = true;

                fingerHapticsLeft[i] = fingerHaptics[i];
            }
            else
            {
                if (fingerHaptics[i] != fingerHapticsRight[i] && fingerHaptics[i] != 0)
                    anyFingerStartedTouching = true;

                fingerHapticsRight[i] = fingerHaptics[i];
            }
        }

        if(anyFingerStartedTouching)
        {
            Debug.Log("Haptics set: " + fingerHaptics[0] + ", " + fingerHaptics[1] + ", " + fingerHaptics[2] + ", " + fingerHaptics[3] + ", " + fingerHaptics[4]);
            _SetHapticFeedback(hand.handType == SteamVR_Input_Sources.LeftHand ? ETrackedControllerRole.LeftHand : ETrackedControllerRole.RightHand,
                new VRHFBInput(fingerHaptics[0], fingerHaptics[1], fingerHaptics[2], fingerHaptics[3], fingerHaptics[4]));
        }
    }

    private short mapDistance(float distance, float minDistance, float maxDistance, short minValue, short maxValue)
    {
        if (distance < minDistance)
        {
            return minValue;
        }

        if (distance > maxDistance)
        {
            return maxValue;
        }

        return (short)Mathf.Lerp(minValue, maxValue, Mathf.InverseLerp(minDistance, maxDistance, distance));
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

    public enum HandFingers : int
    {
        thumb = 0,
        index, 
        middle, 
        ring, 
        pinky
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
    //Thermo goes between 0-1000
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
    private NamedPipeClientStream thermoPipe;
    private NamedPipeClientStream hapticPipe;
    private ETrackedControllerRole pipeControllerRole;
    public NamedPipesProvider(ETrackedControllerRole controllerRole)
    {
        pipeControllerRole = controllerRole;
        forcePipe = new NamedPipeClientStream("vrapplication/ffb/curl/" + (controllerRole == ETrackedControllerRole.RightHand ? "right" : "left"));
        thermoPipe = new NamedPipeClientStream("vrapplication/ffb/thermo/" + (controllerRole == ETrackedControllerRole.RightHand ? "right" : "left"));
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
            thermoPipe.Connect();
            Debug.Log("Successfully connected to " + (pipeControllerRole == ETrackedControllerRole.RightHand ? "right" : "left") + " pipe: thermo");
        }
        catch (Exception e)
        {
            Debug.Log("Unable to connect to " + (pipeControllerRole == ETrackedControllerRole.RightHand ? "right" : "left") + " thermo pipe. Error: " + e);
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

        if (thermoPipe.IsConnected)
        {
            thermoPipe.Dispose();
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
            Debug.Log("running task");
            int size = Marshal.SizeOf(input);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(input, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            forcePipe.Write(arr, 0, size);

            Debug.Log("Sent force feedback message.");

            return true;
        }

        return false;
    }

    public bool TFSend(VRTFBInput input)
    {
        if (thermoPipe.IsConnected)
        {
            Debug.Log("running task");
            int size = Marshal.SizeOf(input);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(input, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            thermoPipe.Write(arr, 0, size);

            Debug.Log("Sent thermo feedback message.");

            return true;
        }

        return false;
    }

    public bool HFSend(VRHFBInput input)
    {
        if (hapticPipe.IsConnected)
        {
            Debug.Log("running task");
            int size = Marshal.SizeOf(input);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(input, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            hapticPipe.Write(arr, 0, size);

            Debug.Log("Sent haptic feedback message.");

            return true;
        }

        return false;
    }
}

