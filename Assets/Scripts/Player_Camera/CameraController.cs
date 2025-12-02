using UnityEngine;
using UnityEngine.InputSystem;

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
    }

    public void StartDragging() => originMousePosition = GetMousePosition;

    public void DragCamera()
    {
        difference = GetMousePosition - cameraTransform.position;
        cameraTransform.position = originMousePosition - difference;
    }

    private Vector3 GetMousePosition => mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

    public void ZoomCamera(Vector2 pInput)
    {
        zoomTarget -= pInput.y * zoomSpeed;
        zoomTarget = Mathf.Clamp(zoomTarget, minZoom, maxZoom);
        mainCamera.orthographicSize = zoomTarget;
    }
}
