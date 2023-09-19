using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public abstract class BaseHand : MonoBehaviour
{
    // Neutral pose for the hand
    [SerializeField] protected Pose defaultPose = null;

    // Serialized so it can be used in editor by the preview hand
    [SerializeField] protected List<Transform> fingerRoots = new List<Transform>();

    // What kind of hand is this?
    [SerializeField] protected HandType handType = HandType.None;
    public HandType HandType => handType;

    public int[] fingerCurlAverages = new int[] {0, 0, 0, 0, 0};

    public List<Transform> Joints { get; protected set; } = new List<Transform>();

    protected virtual void Awake()
    {
        Joints = CollectJoints();
    }

    protected List<Transform> CollectJoints()
    {
        List<Transform> joints = new List<Transform>();

        foreach (Transform root in fingerRoots)
            joints.AddRange(root.GetComponentsInChildren<Transform>());

        return joints;
    }

    public List<Quaternion> GetJointRotations()
    {
        List<Quaternion> rotations = new List<Quaternion>();

        foreach (Transform joint in Joints)
            rotations.Add(joint.localRotation);

        return rotations;
    }

    public void ApplyDefaultPose()
    {
        ApplyPose(defaultPose);
    }

    public void ApplyPose(Pose pose)
    {
        // Get the proper info using hand's type
        HandInfo handInfo = pose.GetHandInfo(handType);

        // Apply rotations 
        ApplyFingerRotations(handInfo.fingerRotations);

        // Position, and rotate, this differs on the type of hand
        ApplyOffset(handInfo.attachPosition, handInfo.attachRotation);
    }

    public void SetForceFeedBack(Pose pose)
    {
        HandInfo skeletonHandInfo = pose.GetHandInfo(handType);

        Pose openHand = (Pose)Resources.Load("Poses/OpenHand");
        Pose closedHand = (Pose)Resources.Load("Poses/ClosedHand");

        HandInfo openHandInfo = openHand.GetHandInfo(handType);
        HandInfo closedHandInfo = closedHand.GetHandInfo(handType);

        List<float>[] fingerCurlValues = new List<float>[5];

        for (int i = 0; i < fingerCurlValues.Length; i++)
        {
            fingerCurlValues[i] = new List<float>();
        }

        for (int boneIndex = 0; boneIndex < skeletonHandInfo.fingerRotations.Count; boneIndex++)
        {
            //calculate open hand angle to poser animation
            float openToPoser = Quaternion.Angle(openHandInfo.fingerRotations[boneIndex], skeletonHandInfo.fingerRotations[boneIndex]);

            //calculate angle from open to closed
            float openToClosed =
                Quaternion.Angle(openHandInfo.fingerRotations[boneIndex], closedHandInfo.fingerRotations[boneIndex]);

            //get the ratio between open to poser and open to closed
            float curl = openToPoser / openToClosed;

            //get the finger for the current bone
            int finger = GetFingerForBone(boneIndex);

            if (!float.IsNaN(curl) && finger >= 0)
            {
                //Add it to the list of bone angles for averaging later
                fingerCurlValues[finger].Add(curl);
            }
        }
        //0-1000 averages of the fingers
        for (int i = 0; i < 5; i++)
        {
            float enumerator = 0;
            for (int j = 0; j < fingerCurlValues[i].Count; j++)
            {
                enumerator += fingerCurlValues[i][j];
            }

            // Check if fingerCurlValues[i].Count is not zero
            if (fingerCurlValues[i].Count != 0)
            {
                //The value we to pass is where 0 is full movement flexibility, so invert.
                fingerCurlAverages[i] = (int)(1000 - (Mathf.FloorToInt(enumerator / fingerCurlValues[i].Count * 1000)));
            }
            else
            {
                Debug.LogError("fingerCurlValues[" + i + "] is empty");
            }

            Debug.Log(fingerCurlAverages[i]);
        }
    }

    public int GetFingerForBone(int boneIndex)
    {
        int[] fingerBones = new int[] { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4 };
        //Index, Middle, Pinky, Ring, Thumb

        if (boneIndex >= 0 && boneIndex < fingerBones.Length)
        {
            return fingerBones[boneIndex];
        }

        return -1;
    }

    public void RestForceFeedback()
    {
        fingerCurlAverages = new int[] { 0, 0, 0, 0, 0 };
    }

    public void ApplyFingerRotations(List<Quaternion> rotations)
    {
        // Make sure we have the rotations for all the joints
        if (HasProperCount(rotations))
        {
            // Set the local rotation of each joint
            for (int i = 0; i < Joints.Count; i++)
                Joints[i].localRotation = rotations[i];
        }
    }

    private bool HasProperCount(List<Quaternion> rotations)
    {
        return Joints.Count == rotations.Count;
    }

    public abstract void ApplyOffset(Vector3 position, Quaternion rotation);
}
