using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[System.Serializable]
public class AnchorData
{
    public Vector3 position;
    public Quaternion rotation;
}

[System.Serializable]
public class AnchorDataList
{
    public List<AnchorData> anchors = new List<AnchorData>();
}

public class ARAnchorPlacementManager : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARAnchorManager anchorManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private TMP_Text consoleText;
    private List<ARAnchor> placedAnchors = new List<ARAnchor>();
    private static string saveFilePath => Path.Combine(Application.persistentDataPath, "anchors.json");
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
    public void SaveAnchorData()
    {
        AnchorDataList dataList = new AnchorDataList();

        foreach (var anchor in placedAnchors)
        {
            if (anchor != null)
            {
                dataList.anchors.Add(new AnchorData
                {
                    position = anchor.transform.position,
                    rotation = anchor.transform.rotation
                });
            }
        }

        string json = JsonUtility.ToJson(dataList, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"Saved {dataList.anchors.Count} anchor(s) to {saveFilePath}");
        StartCoroutine(ShowMessage($"Saved {dataList.anchors.Count} anchor(s) to {saveFilePath}"));

    }

    public void LoadAnchorData()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("No anchor data file found.");
            StartCoroutine(ShowMessage("No anchor data file found."));

            return;
        }

        string json = File.ReadAllText(saveFilePath);
        AnchorDataList dataList = JsonUtility.FromJson<AnchorDataList>(json);

        foreach (var data in dataList.anchors)
        {
            Pose pose = new Pose(data.position, data.rotation);
            // Find a plane to attach to
            ARPlane plane = FindNearestPlane(pose.position);
            if (plane != null)
            {
                ARAnchor anchor = anchorManager.AttachAnchor(plane, pose);
                if (anchor != null)
                {
                    GameObject spawned = Instantiate(objectPrefab, anchor.transform.position, anchor.transform.rotation);
                    spawned.transform.SetParent(anchor.transform);
                    placedAnchors.Add(anchor);
                }
            }
        }

        Debug.Log($"Loaded {dataList.anchors.Count} anchor(s).");
        StartCoroutine(ShowMessage($"Loaded {dataList.anchors.Count} anchor(s)."));

    }
    private ARPlane FindNearestPlane(Vector3 position)
    {
        if (planeManager == null)
        {
            Debug.LogError("ARPlaneManager not assigned!");
            StartCoroutine(ShowMessage("ARPlaneManager not assigned!"));

            return null;
        }

        ARPlane nearestPlane = null;
        float minDistance = float.MaxValue;

        foreach (var plane in planeManager.trackables)
        {
            float distance = Vector3.Distance(position, plane.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPlane = plane;
            }
        }

        return nearestPlane;
    }

    IEnumerator ShowMessage(string message)
    {
        if (consoleText != null)
        {
            consoleText.text = message;
            yield return new WaitForSeconds(3f);
            consoleText.text = "";
        }
    }

}
