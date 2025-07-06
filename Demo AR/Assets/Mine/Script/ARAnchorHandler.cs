using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using System.IO;
using System.Collections;

public class ARAnchorHandler : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float dragSpeed = 0.001f;

    private Camera mainCamera;
    private bool isDragging = false;
    private Vector2 lastInputPosition;

    [System.Serializable]
    public class AnchorData
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
    }

    public void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
#if UNITY_EDITOR
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            isDragging = true;
            lastInputPosition = touch.position;
        }
        else if (touch.phase == TouchPhase.Moved && isDragging)
        {
            Vector2 delta = touch.position - lastInputPosition;
            lastInputPosition = touch.position;
            MoveObject(delta);
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            isDragging = false;
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastInputPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastInputPosition;
            lastInputPosition = Input.mousePosition;
            MoveObject(delta);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    void MoveObject(Vector2 screenDelta)
    {
        Vector3 worldDelta = new Vector3(screenDelta.x, screenDelta.y, 0) * dragSpeed;
        transform.position += mainCamera.transform.TransformDirection(worldDelta);
    }


}
