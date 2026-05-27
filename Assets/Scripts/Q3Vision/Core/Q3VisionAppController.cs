using UnityEngine;
using Q3Vision.Brightness;
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
        [SerializeField] private float maxZoom = 8.0f;
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

        [Header("Brightness Settings")]
        [SerializeField] private bool enableBrightnessControl = true;
        [SerializeField] private float brightnessInitialLevel = 50.0f;
        [SerializeField] private float brightnessMinLevel = 20.0f;
        [SerializeField] private float brightnessDefaultLevel = 50.0f;
        [SerializeField] private float brightnessMaxLevel = 100.0f;
        [SerializeField] private float brightnessChangeSpeed = 45.0f;
        [SerializeField] private float brightnessJoystickDeadZone = 0.35f;

        [Tooltip("Native passthrough brightness minimum. 0 = original image, negative = darker.")]
        [SerializeField] private float minNativePassthroughBrightness = -0.75f;

        [Tooltip("Native passthrough brightness maximum. 0 = original image, positive = brighter.")]
        [SerializeField] private float maxNativePassthroughBrightness = 0.75f;

        [SerializeField] private float passthroughContrast = 0.0f;
        [SerializeField] private float passthroughSaturation = 0.0f;

        [Header("Brightness HUD Settings")]
        [SerializeField] private bool showBrightnessHud = true;
        [SerializeField] private float brightnessHudVisibleSeconds = 2.5f;

        [Tooltip("X: sağ/sol, Y: yukarı/aşağı, Z: gözden uzaklık")]
        [SerializeField] private Vector3 brightnessHudLocalPosition = new Vector3(0.0f, -0.48f, 1.20f);

        private Camera mainCamera;
        private OVRPassthroughLayer passthroughLayer;

        private RemoteInputReader remoteInputReader;
        private ZoomController zoomController;
        private BrightnessController brightnessController;

        private ZoomHudView zoomHudView;
        private BrightnessHudView brightnessHudView;
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

            passthroughLayer = FindPassthroughLayer();

            remoteInputReader = new RemoteInputReader();

            zoomController = new ZoomController(
                initialZoom: initialZoom,
                minZoom: minZoom,
                maxZoom: maxZoom,
                zoomSpeed: zoomSpeed,
                joystickDeadZone: joystickDeadZone
            );

            brightnessController = new BrightnessController(
                initialLevel: brightnessInitialLevel,
                minLevel: brightnessMinLevel,
                defaultLevel: brightnessDefaultLevel,
                maxLevel: brightnessMaxLevel,
                changeSpeed: brightnessChangeSpeed,
                joystickDeadZone: brightnessJoystickDeadZone,
                minNativeBrightness: minNativePassthroughBrightness,
                maxNativeBrightness: maxNativePassthroughBrightness
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

            if (showBrightnessHud)
            {
                brightnessHudView = new BrightnessHudView(
                    mainCamera: mainCamera,
                    localPosition: brightnessHudLocalPosition,
                    visibleSeconds: brightnessHudVisibleSeconds
                );

                brightnessHudView.Create();
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

            ApplyBrightness();

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
            HandleBrightnessInput(inputFrame);

            zoomHudView?.Tick();
            brightnessHudView?.Tick();

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

        private void HandleBrightnessInput(RemoteInputFrame inputFrame)
        {
            if (!enableBrightnessControl || brightnessController == null || !inputFrame.HasGamepad)
            {
                return;
            }

            bool brightnessChanged = brightnessController.TryApplyJoystickBrightness(
                joystickX: inputFrame.BrightnessAxisX,
                deltaTime: Time.deltaTime
            );

            if (!brightnessChanged)
            {
                return;
            }

            ApplyBrightness();

            brightnessHudView?.Show(
                normalizedLevel: brightnessController.GetNormalizedLevel(),
                displayPercent: brightnessController.GetDisplayPercent()
            );
        }

        private void ResetZoom()
        {
            zoomController.Reset();

            currentMode = VisionMode.NativePassthrough;
            cameraMagnifierView?.Hide();
            zoomHudView?.Show(zoomController.ZoomFactor);

            Debug.Log("Q3VisionAppController: Zoom reset.");
        }

        private void ApplyBrightness()
        {
            if (brightnessController == null)
            {
                return;
            }

            float nativeBrightness = brightnessController.GetNativePassthroughBrightness();

            if (passthroughLayer != null)
            {
                passthroughLayer.SetBrightnessContrastSaturation(
                    nativeBrightness,
                    passthroughContrast,
                    passthroughSaturation
                );
            }

            cameraMagnifierView?.SetBrightnessMultiplier(
                brightnessController.GetMagnifierBrightnessMultiplier()
            );
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

        private OVRPassthroughLayer FindPassthroughLayer()
        {
            OVRPassthroughLayer layer = Object.FindFirstObjectByType<OVRPassthroughLayer>();

            if (layer != null)
            {
                return layer;
            }

            OVRPassthroughLayer[] layers = Object.FindObjectsByType<OVRPassthroughLayer>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            return layers.Length > 0 ? layers[0] : null;
        }

        private void OnDestroy()
        {
            zoomHudView?.Destroy();
            brightnessHudView?.Destroy();
            cameraMagnifierView?.Destroy();
        }
    }
}