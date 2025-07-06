using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.IO;
using TMPro;
using System.Collections;

public class TrackedImageAnchorHandler : MonoBehaviour
{
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private TMP_Text consoleText;

    private GameObject spawnedObject;
    private Transform imageTransform;
    bool firstTime = false;
    private static string savePath => Path.Combine(Application.persistentDataPath, "anchor_pos.json");
    private bool anchorLoaded = false;
    [System.Serializable]
    public class AnchorData
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
    }

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var trackedImage in args.added)
        {
            TryHandleImage(trackedImage);
        }
        foreach (var trackedImage in args.updated)
        {
            TryHandleImage(trackedImage);
        }

    }
    void TryHandleImage(ARTrackedImage trackedImage)
    {
        if (anchorLoaded) return;

        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            imageTransform = trackedImage.transform;
            anchorLoaded = true;

            Debug.Log("Image detected and handled.");
            StartCoroutine(ShowMessage("Image tracked: " + trackedImage.referenceImage.name));

            TryLoadAnchorRelativeToImage();
        }
    }
    public void SaveAnchorRelativeToImage()
    {
        if (spawnedObject == null || imageTransform == null)
        {
            spawnedObject = Instantiate(objectPrefab, imageTransform.position, imageTransform.rotation);
            Debug.LogWarning("No object or image to anchor to.");
            StartCoroutine(ShowMessage("No object or image to save."));
            return;
        }

        // Save position/rotation relative to image
        AnchorData data = new AnchorData
        {
            localPosition = imageTransform.InverseTransformPoint(spawnedObject.transform.position),
            localRotation = Quaternion.Inverse(imageTransform.rotation) * spawnedObject.transform.rotation
        };

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, json);

        Debug.Log("Anchor saved relative to image.");
        StartCoroutine(ShowMessage("Anchor saved."));
    }

   public void TryLoadAnchorRelativeToImage()
    {
        if(spawnedObject == null)
        {
            if (!File.Exists(savePath))
            {
                Debug.Log("No saved anchor found.");
                StartCoroutine(ShowMessage("No saved anchor."));
                return;
            }

            string json = File.ReadAllText(savePath);
            AnchorData data = JsonUtility.FromJson<AnchorData>(json);

            if (data != null && imageTransform != null)
            {
                Vector3 worldPos = imageTransform.TransformPoint(data.localPosition);
                Quaternion worldRot = imageTransform.rotation * data.localRotation;

                spawnedObject = Instantiate(objectPrefab, worldPos, worldRot);
                Debug.Log("Loaded object relative to image.");
                StartCoroutine(ShowMessage("Anchor loaded."));
            }
            else
            {
                spawnedObject = Instantiate(objectPrefab, imageTransform.position, imageTransform.rotation);

            }
        }
        
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

    public void imageTraked()
    {
        Debug.Log("Image Tracked.");
    }
}
