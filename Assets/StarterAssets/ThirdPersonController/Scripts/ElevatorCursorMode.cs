using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class ElevatorCursorMode : MonoBehaviour
{
    public StarterAssetsInputs starterInput;

    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            starterInput.cursorLocked = false;
            starterInput.cursorInputForLook = false;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            starterInput.cursorLocked = true;
            starterInput.cursorInputForLook = true;
        }
    }
}