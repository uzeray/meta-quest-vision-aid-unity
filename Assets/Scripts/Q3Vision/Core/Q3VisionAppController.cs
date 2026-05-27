using UnityEngine;
using Q3Vision.Input;
using Q3Vision.UI;
using Q3Vision.Zoom;

namespace Q3Vision.Core
{
    public class Q3VisionAppController : MonoBehaviour
    {
        [Header("Current State")]
        [SerializeField] private VisionMode currentMode = VisionMode.NativePassthrough;

        [Header("Zoom Settings")]
        [SerializeField] private float initialZoom = 1.0f;
        [SerializeField] private float minZoom = 1.0f;
        [SerializeField] private float maxZoom = 6.0f;
        [SerializeField] private float zoomSpeed = 1.5f;
        [SerializeField] private float joystickDeadZone = 0.25f;

        [Header("HUD Settings")]
        [SerializeField] private bool showZoomHud = true;
        [SerializeField] private float hudVisibleSeconds = 3.0f;

        [Tooltip("X: sağ/sol, Y: yukarı/aşağı, Z: gözden uzaklık")]
        [SerializeField] private Vector3 hudLocalPosition = new Vector3(0.5f, -0.5f, 1.25f);

        [Header("Camera Magnifier View")]
        [SerializeField] private bool enableCameraMagnifierView = true;

        [Tooltip("X: sağ/sol, Y: yukarı/aşağı, Z: gözden uzaklık")]
        [SerializeField] private Vector3 magnifierLocalPosition = new Vector3(0.0f, 0.0f, 1.35f);

        [Tooltip("X: genişlik, Y: yükseklik")]
        [SerializeField] private Vector2 magnifierPanelSize = new Vector2(1.20f, 0.90f);

        [SerializeField] private float magnifierVisibleThreshold = 1.05f;
        [SerializeField] private float magnifierBorderVisibleThreshold = 1.20f;

        private Camera mainCamera;
        private RemoteInputReader remoteInputReader;
        private ZoomController zoomController;
        private ZoomHudView zoomHudView;
        private CameraMagnifierView cameraMagnifierView;

        public float ZoomFactor => zoomController?.ZoomFactor ?? initialZoom;
        public VisionMode CurrentMode => currentMode;

        private void Start()
        {
            mainCamera = FindMainCamera();

            if (mainCamera == null)
            {
                Debug.LogWarning("Q3VisionAppController: Main Camera not found.");
                return;
            }

            remoteInputReader = new RemoteInputReader();

            zoomController = new ZoomController(
                initialZoom: initialZoom,
                minZoom: minZoom,
                maxZoom: maxZoom,
                zoomSpeed: zoomSpeed,
                joystickDeadZone: joystickDeadZone
            );

            if (showZoomHud)
            {
                zoomHudView = new ZoomHudView(
                    mainCamera: mainCamera,
                    localPosition: hudLocalPosition,
                    visibleSeconds: hudVisibleSeconds
                );

                zoomHudView.Create();
            }

            if (enableCameraMagnifierView)
            {
                cameraMagnifierView = new CameraMagnifierView(
                    mainCamera: mainCamera,
                    localPosition: magnifierLocalPosition,
                    panelSize: magnifierPanelSize,
                    borderVisibleThreshold: magnifierBorderVisibleThreshold
                );

                cameraMagnifierView.Create();
            }

            Debug.Log("Q3VisionAppController: Started.");
        }

        private void Update()
        {
            if (remoteInputReader == null || zoomController == null)
            {
                return;
            }

            RemoteInputFrame inputFrame = remoteInputReader.Read();

            HandleZoomInput(inputFrame);

            zoomHudView?.Tick();

            if (currentMode == VisionMode.CameraMagnifier)
            {
                cameraMagnifierView?.Tick(zoomController.ZoomFactor);
            }
        }

        private void HandleZoomInput(RemoteInputFrame inputFrame)
        {
            if (!inputFrame.HasGamepad)
            {
                return;
            }

            bool zoomChanged = zoomController.TryApplyJoystickZoom(
                joystickY: inputFrame.ZoomAxisY,
                deltaTime: Time.deltaTime
            );

            if (zoomChanged)
            {
                currentMode = VisionMode.CameraMagnifier;

                float currentZoom = zoomController.ZoomFactor;

                zoomHudView?.Show(currentZoom);

                if (currentZoom >= magnifierVisibleThreshold)
                {
                    cameraMagnifierView?.Show(currentZoom);
                }
                else
                {
                    cameraMagnifierView?.Hide();
                    currentMode = VisionMode.NativePassthrough;
                }
            }

            if (inputFrame.ResetZoomPressed)
            {
                ResetZoom();
            }
        }

        private void ResetZoom()
        {
            zoomController.Reset();

            currentMode = VisionMode.NativePassthrough;
            cameraMagnifierView?.Hide();
            zoomHudView?.Show(zoomController.ZoomFactor);

            Debug.Log("Q3VisionAppController: Zoom reset.");
        }

        private Camera FindMainCamera()
        {
            if (Camera.main != null)
            {
                return Camera.main;
            }

            Camera[] cameras = Object.FindObjectsByType<Camera>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

            return cameras.Length > 0 ? cameras[0] : null;
        }

        private void OnDestroy()
        {
            zoomHudView?.Destroy();
            cameraMagnifierView?.Destroy();
        }
    }
}