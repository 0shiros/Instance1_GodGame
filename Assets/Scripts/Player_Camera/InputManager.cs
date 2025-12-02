using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private CameraController cameraController;
    private Vector2 moveInput;
    private bool isDragging = false;
    private bool canMove = true;

    private void Start()
    {
        if (cameraController == null)
        {
            Debug.LogError("CameraController is null");
        }
    }

    private void Update()
    {
        if (canMove) cameraController.MoveCamera(moveInput);

        if (isDragging && canMove) cameraController.DragCamera();
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

    public void OnScroll(InputAction.CallbackContext context)
    {
        if(context.performed) cameraController.ZoomCamera(context.ReadValue<Vector2>());
    }
    
    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if(context.started) Debug.Log("Left Clicked Pressed");
    }
    
    public void OnSpacebar(InputAction.CallbackContext context)
    {
        if(context.started) Debug.Log("Spacebar Pressed");
    }
}
