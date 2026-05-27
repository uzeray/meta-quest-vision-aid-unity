using Meta.XR;
using UnityEngine;
using UnityEngine.UI;

namespace Q3Vision.UI
{
    public sealed class CameraMagnifierView
    {
        private readonly Camera mainCamera;
        private readonly Vector3 localPosition;
        private readonly Vector2 panelSize;
        private readonly float borderVisibleThreshold;

        private PassthroughCameraAccess cameraAccess;

        private GameObject rootObject;
        private RawImage rawImage;
        private Image outerBorderImage;
        private Image innerBorderImage;

        private RectTransform rawImageRect;
        private RectTransform innerBorderRect;

        private bool isCreated;
        private bool requestedVisible;

        private const float CanvasWorldScale = 0.0015f;
        private const float PanelPixelWidth = 900f;
        private const float PanelPixelHeight = 650f;

        private const float OuterBorderPx = 18f;
        private const float InnerBorderPx = 8f;

        private static readonly Color OuterBorderColor = Color.black;
        private static readonly Color InnerBorderColor = new Color(0.0f, 0.95f, 1.0f, 1.0f);

        public CameraMagnifierView(
            Camera mainCamera,
            Vector3 localPosition,
            Vector2 panelSize,
            float borderVisibleThreshold = 1.20f)
        {
            this.mainCamera = mainCamera;
            this.localPosition = localPosition;
            this.panelSize = panelSize;
            this.borderVisibleThreshold = Mathf.Max(1.0f, borderVisibleThreshold);
        }

        public void Create()
        {
            if (isCreated)
            {
                return;
            }

            if (mainCamera == null)
            {
                return;
            }

            cameraAccess = Object.FindFirstObjectByType<PassthroughCameraAccess>();

            if (cameraAccess == null)
            {
                return;
            }

            rootObject = new GameObject("Q3 Vision Camera Magnifier View");
            rootObject.transform.SetParent(mainCamera.transform, false);
            rootObject.transform.localPosition = localPosition;
            rootObject.transform.localRotation = Quaternion.identity;
            rootObject.transform.localScale = Vector3.one;

            Canvas canvas = rootObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = mainCamera;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 3000;

            CanvasScaler canvasScaler = rootObject.AddComponent<CanvasScaler>();
            canvasScaler.dynamicPixelsPerUnit = 30f;

            GraphicRaycaster raycaster = rootObject.AddComponent<GraphicRaycaster>();
            raycaster.enabled = false;

            RectTransform rootRect = rootObject.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(PanelPixelWidth, PanelPixelHeight);
            rootRect.localScale = new Vector3(
                CanvasWorldScale * panelSize.x,
                CanvasWorldScale * panelSize.y,
                CanvasWorldScale
            );

            outerBorderImage = CreatePanelImage(
                objectName: "Magnifier Outer Border",
                parent: rootRect,
                color: OuterBorderColor
            );

            innerBorderImage = CreatePanelImage(
                objectName: "Magnifier Inner Border",
                parent: rootRect,
                color: InnerBorderColor
            );

            innerBorderRect = innerBorderImage.GetComponent<RectTransform>();

            GameObject rawImageObject = new GameObject("Magnifier Camera RawImage");
            rawImageObject.transform.SetParent(rootObject.transform, false);

            rawImageRect = rawImageObject.AddComponent<RectTransform>();
            rawImageRect.anchorMin = Vector2.zero;
            rawImageRect.anchorMax = Vector2.one;
            rawImageRect.pivot = new Vector2(0.5f, 0.5f);
            rawImageRect.offsetMin = Vector2.zero;
            rawImageRect.offsetMax = Vector2.zero;
            rawImageRect.localScale = Vector3.one;

            rawImage = rawImageObject.AddComponent<RawImage>();
            rawImage.color = Color.white;
            rawImage.raycastTarget = false;
            rawImage.uvRect = new Rect(0f, 0f, 1f, 1f);

            SetBorderVisible(false);

            Hide();

            isCreated = true;
        }

        public void Show(float zoomFactor)
        {
            requestedVisible = true;

            if (!isCreated)
            {
                Create();
            }

            UpdateView(zoomFactor);
        }

        public void Hide()
        {
            requestedVisible = false;

            if (rootObject != null)
            {
                rootObject.SetActive(false);
            }
        }

        public void Tick(float zoomFactor)
        {
            if (!requestedVisible)
            {
                return;
            }

            UpdateView(zoomFactor);
        }

        public void Destroy()
        {
            if (rootObject != null)
            {
                Object.Destroy(rootObject);
            }

            rootObject = null;
            rawImage = null;
            rawImageRect = null;
            outerBorderImage = null;
            innerBorderImage = null;
            innerBorderRect = null;
            cameraAccess = null;
            isCreated = false;
            requestedVisible = false;
        }

        private void UpdateView(float zoomFactor)
        {
            if (cameraAccess == null || rawImage == null || rootObject == null)
            {
                return;
            }

            Texture cameraTexture = cameraAccess.GetTexture();

            if (cameraTexture == null)
            {
                rootObject.SetActive(false);
                return;
            }

            rawImage.texture = cameraTexture;

            float safeZoom = Mathf.Max(1.0f, zoomFactor);
            bool borderVisible = safeZoom >= borderVisibleThreshold;

            SetBorderVisible(borderVisible);
            ApplyAspectCorrectCenteredZoomCrop(safeZoom, cameraTexture, borderVisible);

            if (!rootObject.activeSelf)
            {
                rootObject.SetActive(true);
            }
        }

        private void ApplyAspectCorrectCenteredZoomCrop(
            float zoomFactor,
            Texture texture,
            bool borderVisible)
        {
            float textureWidth = Mathf.Max(1f, texture.width);
            float textureHeight = Mathf.Max(1f, texture.height);
            float textureAspect = textureWidth / textureHeight;

            float visiblePanelWidth = PanelPixelWidth;
            float visiblePanelHeight = PanelPixelHeight;

            if (borderVisible)
            {
                float inset = OuterBorderPx + InnerBorderPx;
                visiblePanelWidth -= inset * 2f;
                visiblePanelHeight -= inset * 2f;
            }

            visiblePanelWidth = Mathf.Max(1f, visiblePanelWidth);
            visiblePanelHeight = Mathf.Max(1f, visiblePanelHeight);

            float panelAspect =
                (visiblePanelWidth * Mathf.Max(0.01f, panelSize.x)) /
                (visiblePanelHeight * Mathf.Max(0.01f, panelSize.y));

            float baseUvWidth;
            float baseUvHeight;

            if (panelAspect >= textureAspect)
            {
                baseUvWidth = 1.0f;
                baseUvHeight = textureAspect / panelAspect;
            }
            else
            {
                baseUvHeight = 1.0f;
                baseUvWidth = panelAspect / textureAspect;
            }

            float uvWidth = Mathf.Clamp01(baseUvWidth / zoomFactor);
            float uvHeight = Mathf.Clamp01(baseUvHeight / zoomFactor);

            float offsetX = (1.0f - uvWidth) * 0.5f;
            float offsetY = (1.0f - uvHeight) * 0.5f;

            rawImage.uvRect = new Rect(
                offsetX,
                offsetY,
                uvWidth,
                uvHeight
            );
        }

        private void SetBorderVisible(bool visible)
        {
            if (outerBorderImage != null)
            {
                outerBorderImage.enabled = visible;
            }

            if (innerBorderImage != null)
            {
                innerBorderImage.enabled = visible;
            }

            if (rawImageRect == null)
            {
                return;
            }

            if (visible)
            {
                float rawInset = OuterBorderPx + InnerBorderPx;

                if (innerBorderRect != null)
                {
                    innerBorderRect.offsetMin = new Vector2(OuterBorderPx, OuterBorderPx);
                    innerBorderRect.offsetMax = new Vector2(-OuterBorderPx, -OuterBorderPx);
                }

                rawImageRect.offsetMin = new Vector2(rawInset, rawInset);
                rawImageRect.offsetMax = new Vector2(-rawInset, -rawInset);
            }
            else
            {
                rawImageRect.offsetMin = Vector2.zero;
                rawImageRect.offsetMax = Vector2.zero;

                if (innerBorderRect != null)
                {
                    innerBorderRect.offsetMin = Vector2.zero;
                    innerBorderRect.offsetMax = Vector2.zero;
                }
            }
        }

        private static Image CreatePanelImage(
            string objectName,
            RectTransform parent,
            Color color)
        {
            GameObject imageObject = new GameObject(objectName);
            imageObject.transform.SetParent(parent, false);

            RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            return image;
        }
    }
}