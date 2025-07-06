using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.IO;
using UnityEngine.EventSystems;

[System.Serializable]
public class AnnotationDatas
{
    public Vector3 localPosition;
    public Quaternion localRotation;
}

[System.Serializable]
public class AnnotationSaveData
{
    public string imageName;
    public List<AnnotationDatas> annotations = new List<AnnotationDatas>();
}

public class ImageAnchorHandler : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private GameObject annotationPrefab;

    private Dictionary<string, ARTrackedImage> trackedImages = new Dictionary<string, ARTrackedImage>();
    private Dictionary<string, List<GameObject>> spawnedAnnotations = new Dictionary<string, List<GameObject>>();
    private string savePath => Path.Combine(Application.persistentDataPath, "annotations.json");

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnImageChanged;
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnImageChanged;
    }

    private void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                TryPlaceAnnotation(Input.GetTouch(0).position);
            }
        }
    }

    private void OnImageChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            trackedImages[trackedImage.name] = trackedImage;
            LoadAnnotations(trackedImage);
        }

        foreach (var trackedImage in args.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking &&
                !trackedImages.ContainsKey(trackedImage.name))
            {
                trackedImages[trackedImage.name] = trackedImage;
                LoadAnnotations(trackedImage);
            }
        }
    }

    private void TryPlaceAnnotation(Vector2 touchPos)
    {
        foreach (var pair in trackedImages)
        {
            var trackedImage = pair.Value;

            if (trackedImage.trackingState != TrackingState.Tracking)
                continue;

            Pose pose = new Pose(trackedImage.transform.position, trackedImage.transform.rotation);
            Vector3 hitPos = trackedImage.transform.position + trackedImage.transform.forward * 0.1f; // basic placement in front

            // Convert world to local position relative to image
            Vector3 localPos = trackedImage.transform.InverseTransformPoint(hitPos);
            Quaternion localRot = Quaternion.Inverse(trackedImage.transform.rotation) * Quaternion.identity;

            GameObject annotation = Instantiate(annotationPrefab, hitPos, trackedImage.transform.rotation);
            annotation.transform.SetParent(trackedImage.transform);

            if (!spawnedAnnotations.ContainsKey(pair.Key))
                spawnedAnnotations[pair.Key] = new List<GameObject>();

            spawnedAnnotations[pair.Key].Add(annotation);

            SaveAnnotations();
            break; // place only once
        }
    }

    private void SaveAnnotations()
    {
        List<AnnotationSaveData> allData = new List<AnnotationSaveData>();

        foreach (var pair in spawnedAnnotations)
        {
            var imageName = pair.Key;
            var objs = pair.Value;

            var data = new AnnotationSaveData { imageName = imageName };

            foreach (var obj in objs)
            {
                data.annotations.Add(new AnnotationDatas
                {
                    localPosition = obj.transform.localPosition,
                    localRotation = obj.transform.localRotation
                });
            }

            allData.Add(data);
        }

        string json = JsonUtility.ToJson(new Wrapper { data = allData }, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Annotations saved to: " + savePath);
    }

    private void LoadAnnotations(ARTrackedImage trackedImage)
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        var wrapper = JsonUtility.FromJson<Wrapper>(json);

        foreach (var data in wrapper.data)
        {
            if (data.imageName != trackedImage.name)
                continue;

            if (!spawnedAnnotations.ContainsKey(data.imageName))
                spawnedAnnotations[data.imageName] = new List<GameObject>();

            foreach (var ann in data.annotations)
            {
                Vector3 worldPos = trackedImage.transform.TransformPoint(ann.localPosition);
                Quaternion worldRot = trackedImage.transform.rotation * ann.localRotation;

                GameObject annotation = Instantiate(annotationPrefab, worldPos, worldRot);
                annotation.transform.SetParent(trackedImage.transform);
                spawnedAnnotations[data.imageName].Add(annotation);
            }
        }
    }

    [System.Serializable]
    public class Wrapper
    {
        public List<AnnotationSaveData> data;
    }
}
