using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private CameraController cameraController;
    private Vector2 moveInput;
    private bool isDragging = false;

    private void Start()
    {
        if (cameraController == null)
        {
            Debug.LogError("CameraController is null");
        }
    }

    private void Update()
    {
        cameraController.MoveCamera(moveInput);

        if (isDragging)
        {
            cameraController.DragCamera();
        }
    }

    public void OnCameraMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    public void OnRightClick(InputAction.CallbackContext context)
    {
        if(context.started) cameraController.StartDragging();
        isDragging = context.performed || context.started;
    }
}
