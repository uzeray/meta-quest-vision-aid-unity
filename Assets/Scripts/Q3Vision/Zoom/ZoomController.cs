using UnityEngine;

namespace Q3Vision.Zoom
{
    public sealed class ZoomController
    {
        private readonly float minZoom;
        private readonly float maxZoom;
        private readonly float zoomSpeed;
        private readonly float joystickDeadZone;

        public float ZoomFactor { get; private set; }

        public ZoomController(
            float initialZoom,
            float minZoom,
            float maxZoom,
            float zoomSpeed,
            float joystickDeadZone)
        {
            this.minZoom = minZoom;
            this.maxZoom = maxZoom;
            this.zoomSpeed = zoomSpeed;
            this.joystickDeadZone = joystickDeadZone;

            ZoomFactor = Mathf.Clamp(initialZoom, minZoom, maxZoom);
        }

        public bool TryApplyJoystickZoom(float joystickY, float deltaTime)
        {
            if (Mathf.Abs(joystickY) <= joystickDeadZone)
            {
                return false;
            }

            float oldZoom = ZoomFactor;
            float zoomDelta = joystickY * zoomSpeed * deltaTime;

            ZoomFactor = Mathf.Clamp(ZoomFactor + zoomDelta, minZoom, maxZoom);

            return !Mathf.Approximately(oldZoom, ZoomFactor);
        }

        public bool Reset()
        {
            if (Mathf.Approximately(ZoomFactor, 1.0f))
            {
                return false;
            }

            ZoomFactor = 1.0f;
            return true;
        }
    }
}