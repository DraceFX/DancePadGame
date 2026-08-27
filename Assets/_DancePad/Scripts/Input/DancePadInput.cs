using UnityEngine;
using UnityEngine.InputSystem;

public class DancePadInput : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference up;
    [SerializeField] private InputActionReference down;
    [SerializeField] private InputActionReference left;
    [SerializeField] private InputActionReference right;

    [SerializeField] private InputActionReference upLeft;
    [SerializeField] private InputActionReference upRight;
    [SerializeField] private InputActionReference downLeft;
    [SerializeField] private InputActionReference downRight;

    private void OnEnable()
    {
        Enable(up);
        Enable(down);
        Enable(left);
        Enable(right);

        Enable(upLeft);
        Enable(upRight);
        Enable(downLeft);
        Enable(downRight);

        if (up != null) up.action.performed += OnUp;
        if (down != null) down.action.performed += OnDown;
        if (left != null) left.action.performed += OnLeft;
        if (right != null) right.action.performed += OnRight;

        if (upLeft != null) upLeft.action.performed += OnUpLeft;
        if (upRight != null) upRight.action.performed += OnUpRight;
        if (downLeft != null) downLeft.action.performed += OnDownLeft;
        if (downRight != null) downRight.action.performed += OnDownRight;
    }

    private void OnDisable()
    {
        if (up != null) up.action.performed -= OnUp;
        if (down != null) down.action.performed -= OnDown;
        if (left != null) left.action.performed -= OnLeft;
        if (right != null) right.action.performed -= OnRight;

        if (upLeft != null) upLeft.action.performed -= OnUpLeft;
        if (upRight != null) upRight.action.performed -= OnUpRight;
        if (downLeft != null) downLeft.action.performed -= OnDownLeft;
        if (downRight != null) downRight.action.performed -= OnDownRight;

        Disable(up);
        Disable(down);
        Disable(left);
        Disable(right);

        Disable(upLeft);
        Disable(upRight);
        Disable(downLeft);
        Disable(downRight);
    }

    private void Enable(InputActionReference action)
    {
        action?.action.Enable();
    }

    private void Disable(InputActionReference action)
    {
        action?.action.Disable();
    }

    private void OnUp(InputAction.CallbackContext _) => Press(DancePadDirection.Up);
    private void OnDown(InputAction.CallbackContext _) => Press(DancePadDirection.Down);
    private void OnLeft(InputAction.CallbackContext _) => Press(DancePadDirection.Left);
    private void OnRight(InputAction.CallbackContext _) => Press(DancePadDirection.Right);

    private void OnUpLeft(InputAction.CallbackContext _) => Press(DancePadDirection.UpLeft);
    private void OnUpRight(InputAction.CallbackContext _) => Press(DancePadDirection.UpRight);
    private void OnDownLeft(InputAction.CallbackContext _) => Press(DancePadDirection.DownLeft);
    private void OnDownRight(InputAction.CallbackContext _) => Press(DancePadDirection.DownRight);

    private void Press(DancePadDirection direction)
    {
        // Debug.Log($"Pressed {direction}");
        GameEvents.RaiseDancePadPressed(direction);
    }
}
