using UnityEngine;

public class TapToSpawnAndDrag : MonoBehaviour
{
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private float dragSpeed = 0.001f;

    private Camera mainCamera;
    private GameObject spawnedObject;
    private bool isDragging = false;
    private Vector2 lastInputPosition;

    void Start()
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

        if (spawnedObject == null && touch.phase == TouchPhase.Began)
        {
            SpawnObject();
        }
        else if (spawnedObject != null)
        {
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
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (spawnedObject == null)
            {
                SpawnObject();
            }
            else
            {
                isDragging = true;
                lastInputPosition = Input.mousePosition;
            }
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

    void SpawnObject()
    {
        Vector3 spawnPos = mainCamera.transform.position + mainCamera.transform.forward * 1f;
        spawnedObject = Instantiate(objectPrefab, spawnPos, Quaternion.identity);
    }

    void MoveObject(Vector2 screenDelta)
    {
        Vector3 worldDelta = new Vector3(screenDelta.x, screenDelta.y, 0) * dragSpeed;
        spawnedObject.transform.position += mainCamera.transform.TransformDirection(worldDelta);
    }
}
