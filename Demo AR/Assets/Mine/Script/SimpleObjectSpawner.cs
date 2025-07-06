using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SimpleObjectSpawner : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARAnchorManager anchorManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private TMP_Text consoleText;
    private List<ARAnchor> placedAnchors = new List<ARAnchor>();
    private void Awake()
    {
        if (planeManager == null)
            planeManager = FindObjectOfType<ARPlaneManager>();
    }
    public bool isTouchingUI = false;

    private void Update()
    {

#if UNITY_EDITOR
        // For mouse click in editor
        if (Input.GetMouseButtonDown(0))
        {
            isTouchingUI = EventSystem.current.IsPointerOverGameObject();

            if (!isTouchingUI)
                TryPlaceObject(Input.mousePosition);
        }
#else
    // For touch input on mobile
    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
    {
        isTouchingUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);

        if (!isTouchingUI)
            TryPlaceObject(Input.GetTouch(0).position);
    }
#endif
    }
    private void TryPlaceObject(Vector2 touchPosition)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            ARPlane hitPlane = planeManager.GetPlane(hits[0].trackableId);

            if (hitPlane != null)
            {
                ARAnchor anchor = anchorManager.AttachAnchor(hitPlane, hitPose);
                if (anchor != null)
                {
                    GameObject spawned = Instantiate(objectPrefab, anchor.transform.position, anchor.transform.rotation);
                    spawned.transform.SetParent(anchor.transform);
                    placedAnchors.Add(anchor);
                }
            }
        }
    }
}
