using UnityEngine;
using UnityEngine.InputSystem;

namespace Q3Vision.Input
{
    public sealed class RemoteInputReader
    {
        public RemoteInputFrame Read()
        {
            Gamepad gamepad = Gamepad.current;

            if (gamepad == null && Gamepad.all.Count > 0)
            {
                gamepad = Gamepad.all[0];
            }

            if (gamepad == null)
            {
                return RemoteInputFrame.Empty;
            }

            Vector2 leftStick = gamepad.leftStick.ReadValue();

            bool resetPressed =
                gamepad.buttonEast.wasPressedThisFrame ||
                gamepad.selectButton.wasPressedThisFrame ||
                gamepad.startButton.wasPressedThisFrame;

            return new RemoteInputFrame(
                hasGamepad: true,
                zoomAxisY: leftStick.y,
                resetZoomPressed: resetPressed
            );
        }
    }
}