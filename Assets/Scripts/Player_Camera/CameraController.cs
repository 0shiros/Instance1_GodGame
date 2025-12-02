using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private Camera mainCamera;
    private Transform cameraTransform;
    [SerializeField] private float moveSpeed = 5f;
    public Vector3 originMousePosition;
    private Vector3 difference;
    
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
}
