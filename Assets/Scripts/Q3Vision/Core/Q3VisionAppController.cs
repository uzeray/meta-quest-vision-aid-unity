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
        [SerializeField] private Vector3 hudLocalPosition = new Vector3(0.10f, -0.06f, 1.25f);

        private Camera mainCamera;
        private RemoteInputReader remoteInputReader;
        private ZoomController zoomController;
        private ZoomHudView zoomHudView;

        public float ZoomFactor => zoomController?.ZoomFactor ?? initialZoom;
        public VisionMode CurrentMode => currentMode;

        private void Start()
        {
            mainCamera = FindMainCamera();

            if (mainCamera == null)
            {
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
                zoomHudView?.Show(zoomController.ZoomFactor);
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
            zoomHudView?.Show(zoomController.ZoomFactor);

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

            if (cameras.Length > 0)
            {
                return cameras[0];
            }

            return null;
        }

        private void OnDestroy()
        {
            zoomHudView?.Destroy();
        }
    }
}