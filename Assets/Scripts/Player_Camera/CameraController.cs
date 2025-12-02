using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    [Header("CameraSettings")]
    private Camera mainCamera;
    private Transform cameraTransform;
    
    [Header("CameraMovement")]
    [SerializeField] private float moveSpeed = 5f;
    private Vector3 originMousePosition;
    private Vector3 difference;

    [Header("CameraZoom")]
    [SerializeField] private float startZoom = 6f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 10f;
    private float zoomTarget;
    
    [Header("CameraLimits")]
    [SerializeField] private Tilemap limitTilemap;

    
    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found");
        }
        else
        {
            cameraTransform = mainCamera.transform;
            zoomTarget = mainCamera.orthographicSize = startZoom;
        }
    }

    public void MoveCamera(Vector2 pInput)
    {
        Vector3 moveDirection = new Vector2(pInput.x, pInput.y).normalized;
        cameraTransform.position += moveDirection * (moveSpeed * Time.deltaTime);
        ClampCameraPosition(cameraTransform.position);
    }

    public void StartDragging() => originMousePosition = GetMousePosition;

    public void DragCamera()
    {
        difference = GetMousePosition - cameraTransform.position;
        cameraTransform.position = originMousePosition - difference;
        ClampCameraPosition(cameraTransform.position);
    }

    private Vector3 GetMousePosition => mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

    public void ZoomCamera(Vector2 pInput)
    {
        zoomTarget -= pInput.y * zoomSpeed;
        zoomTarget = Mathf.Clamp(zoomTarget, minZoom, maxZoom);
        mainCamera.orthographicSize = zoomTarget;
    }
    
    private void ClampCameraPosition(Vector3 pCameraPosition)
    {
        if (limitTilemap == null)
        {
            Debug.LogError("LimitTilemap not set");
            return;
        }

        Vector3 cameraMin = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 cameraMax = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        Bounds tilemapBounds = limitTilemap.localBounds;

        float clampedX = Mathf.Clamp(cameraTransform.position.x, tilemapBounds.min.x + (cameraMax.x - cameraMin.x) / 2, tilemapBounds.max.x - (cameraMax.x - cameraMin.x) / 2);
        float clampedY = Mathf.Clamp(cameraTransform.position.y, tilemapBounds.min.y + (cameraMax.y - cameraMin.y) / 2, tilemapBounds.max.y - (cameraMax.y - cameraMin.y) / 2);

        cameraTransform.position = new Vector3(clampedX, clampedY, cameraTransform.position.z);
    }
}
