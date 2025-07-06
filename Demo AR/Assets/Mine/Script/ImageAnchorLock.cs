using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageAnchorLock : MonoBehaviour
{
    public GameObject objectToPlace;
    private bool isLocked = false;
    private ARTrackedImageManager trackedImageManager;

    void Awake()
    {
        trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.updated)
        {
            if (!isLocked && trackedImage.trackingState == TrackingState.Tracking)
            {

                ARAnchor anchor = trackedImage.gameObject.GetComponent<ARAnchor>();
                if (anchor == null)
                    anchor = trackedImage.gameObject.AddComponent<ARAnchor>();

                if (anchor != null)
                {
                    objectToPlace.transform.SetPositionAndRotation(anchor.transform.position, anchor.transform.rotation);
                    objectToPlace.transform.parent = anchor.transform;

                    isLocked = true;
                }
                else
                {
                    Debug.LogWarning("Anchor Lock");
                }
            }
        }
    }
}