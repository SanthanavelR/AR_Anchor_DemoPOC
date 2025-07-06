using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.IO;

public class ARAnchorManagerController : MonoBehaviour
{
    [SerializeField] GameObject movablePrefab;   // Prefab to move before placing
    [SerializeField] GameObject placedPrefab;    // Prefab to place and save
    [SerializeField] ARRaycastManager raycastManager;
    [SerializeField] ARAnchorManager anchorManager;
    [SerializeField] Camera arCamera;

    private GameObject currentMovableObject;
    private static string savePath => Path.Combine(Application.persistentDataPath, "anchors.json");
    private List<GameObject> placedObjects = new List<GameObject>();
    private List<Vector3> savedPositions = new List<Vector3>();

    void Start()
    {
        LoadAnchors();
    }

    void Update()
    {
        if (currentMovableObject != null)
        {
            MoveToTouch();
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                PlaceAnchor();
            }
        }
        else
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                EnableMovableObject();
            }
        }
    }

    void EnableMovableObject()
    {
        Vector3 forward = arCamera.transform.position + arCamera.transform.forward * 0.5f;
        currentMovableObject = Instantiate(movablePrefab, forward, Quaternion.identity);
    }

    void MoveToTouch()
    {
        Vector2 screenPos = new Vector2(Screen.width / 2, Screen.height / 2);
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;
            currentMovableObject.transform.position = pose.position;
        }
    }

    void PlaceAnchor()
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.Planes);

        if (hits.Count > 0)
        {
            var hitPose = hits[0].pose;
            var trackable = hits[0].trackable as ARPlane;

            ARAnchor anchor = anchorManager.AttachAnchor(trackable, hitPose);
            if (anchor != null)
            {
                GameObject placedObj = Instantiate(placedPrefab, anchor.transform);
                placedObjects.Add(placedObj);
                savedPositions.Add(anchor.transform.position);
                SaveAnchors();
            }

            Destroy(currentMovableObject);
            currentMovableObject = null;
        }

    }

    void SaveAnchors()
    {
        AnchorSaveData saveData = new AnchorSaveData();
        saveData.positions = savedPositions;
        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(savePath, json);
    }

    void LoadAnchors()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            AnchorSaveData saveData = JsonUtility.FromJson<AnchorSaveData>(json);

            if (saveData != null && saveData.positions != null)
            {
                foreach (var pos in saveData.positions)
                {
                    GameObject placedObj = Instantiate(placedPrefab, pos, Quaternion.identity);
                    placedObjects.Add(placedObj);
                }
                savedPositions = saveData.positions;
            }
        }
        else
        {
            EnableMovableObject();
        }
    }

    [System.Serializable]
    public class AnchorSaveData
    {
        public List<Vector3> positions;
    }
}
