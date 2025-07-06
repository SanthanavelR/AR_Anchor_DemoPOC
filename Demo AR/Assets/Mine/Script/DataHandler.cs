using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static SimpleObjectSpawner;

public class DataHandler : MonoBehaviour
{
    
    public GameObject trackedImageObject;
    public GameObject cubePrefab;
    string filePath => Path.Combine(Application.persistentDataPath, "annotation.json");
    [SerializeField] private TMP_Text consoleText;

    public void SaveAnnotation(AnnotationData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
        Debug.Log("Saved to: " + filePath);
        StartCoroutine(ShowMessage($"Saved to: " + filePath));

    }

    public void LoadAndPlaceCube()
    {
        if (!File.Exists(filePath)) return;
        if(trackedImageObject== null)
        {
            trackedImageObject = FindObjectOfType<ARTrackedImage>().gameObject;
        }
        string json = File.ReadAllText(filePath);
        AnnotationDatas data = JsonUtility.FromJson<AnnotationDatas>(json);

        Vector3 worldPos = data.localPosition;
        Quaternion worldRot = data.localRotation;
        StartCoroutine(ShowMessage($"Saved to: " + worldPos + " rot :" + worldRot));

        GameObject spawned = Instantiate(cubePrefab);
        spawned.transform.SetParent(trackedImageObject.transform);
        spawned.transform.localPosition = worldPos;
        spawned.transform.localRotation = worldRot;
        
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
