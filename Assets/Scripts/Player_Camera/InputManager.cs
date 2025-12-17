using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class InputManager : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private CameraController cameraController;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private TileBrush tileBrush;
    [SerializeField] private EnvironementBrush environmentBrush;

    [Header("Settings")] private Vector2 moveInput;
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
        if (context.started) cameraController.StartDragging();
        isDragging = context.performed || context.started;
    }

    public void OnScroll(InputAction.CallbackContext context)
    {
        if (context.performed) cameraController.ZoomCamera(context.ReadValue<Vector2>());
    }

    public void OnAPressed(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        Vector2 point = new Vector2(worldPos.x, worldPos.y);

        Collider2D hit = Physics2D.OverlapPoint(point);

        if (hit != null)
        {
            SearchTree nation = hit.GetComponent<SearchTree>();

            if (nation != null)
            {
                NationEvents.OnNationSelected?.Invoke(nation);
            }
        }
    }

    public void OnPPressed(InputAction.CallbackContext context)
    {
        uiManager.PauseButton();
        uiManager.ChangePauseMode();
    }

    public void OnEscapePressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (tileBrush.GetTile() != null)
            {
                tileBrush.Reset();
            }
            else if (environmentBrush.GetTile() != null)
            {
                environmentBrush.Reset();
            }
            else
            {
                uiManager.PauseButton();
                uiManager.ChangePauseMode();
            }
        }
    }
}