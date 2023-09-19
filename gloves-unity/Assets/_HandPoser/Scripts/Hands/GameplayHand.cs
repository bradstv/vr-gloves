using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.OpenXR.Input;

public class GameplayHand : BaseHand
{
    // The interactor we react to
    [SerializeField] private XRBaseInteractor targetInteractor = null;

    private void OnEnable()
    {
        // Subscribe to selected events
        targetInteractor.onSelectEntered.AddListener(TryApplyObjectPose);
        targetInteractor.onSelectExited.AddListener(TryApplyDefaultPose);

        //targetInteractor.onHoverEntered.AddListener(TryApplyObjectPose);
        //targetInteractor.onHoverExited.AddListener(TryApplyDefaultPose);
    }

    private void OnDisable()
    {
        // Unsubscribe to selected events
        targetInteractor.onSelectEntered.RemoveListener(TryApplyObjectPose);
        targetInteractor.onSelectExited.RemoveListener(TryApplyDefaultPose);

        //targetInteractor.onHoverEntered.RemoveListener(TryApplyObjectPose);
        //targetInteractor.onHoverExited.RemoveListener(TryApplyDefaultPose);
    }

    private void TryApplyObjectPose(XRBaseInteractable interactable)
    {
        // Try and get pose container, and apply
        if(interactable.TryGetComponent(out PoseContainer poseContainer))
        {
            Debug.Log("Tried to Apply FFB");
            SetForceFeedBack(poseContainer.pose);
            //ApplyPose(poseContainer.pose);
        }
        else
        {
            Debug.Log("Failed to Apply FFB");

        }
    }

    private void TryApplyDefaultPose(XRBaseInteractable interactable)
    {
        // Try and get pose container, and apply
        if (interactable.TryGetComponent(out PoseContainer poseContainer))
        {
            //ApplyDefaultPose();
            RestForceFeedback();
        }
    }

    public override void ApplyOffset(Vector3 position, Quaternion rotation)
    {
        // Invert since the we're moving the attach point instead of the hand
        Vector3 finalPosition = position * -1.0f;
        Quaternion finalRotation = Quaternion.Inverse(rotation);

        // Since it's a local position, we can just rotate around zero
        finalPosition = finalPosition.RotatePointAroundPivot(Vector3.zero, finalRotation.eulerAngles);

        // Set the position and rotach of attach
        targetInteractor.attachTransform.localPosition = finalPosition;
        targetInteractor.attachTransform.localRotation = finalRotation;
    }

    private void OnValidate()
    {
        // Let's have this done automatically, but not hide the requirement
        if (!targetInteractor)
        {
            targetInteractor = GetComponentInParent<XRBaseInteractor>();
        }
    }
}