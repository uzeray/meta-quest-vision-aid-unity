using UnityEngine;
using UnityEngine.UI;

namespace Q3Vision.UI
{
    public sealed class BrightnessHudView
    {
        private readonly Camera mainCamera;
        private readonly Vector3 localPosition;
        private readonly float visibleSeconds;

        private GameObject rootObject;
        private RectTransform fillRect;
        private Text labelText;

        private float hideAtTime;
        private bool isCreated;

        private const float CanvasWorldScale = 0.0015f;
        private const float RootWidth = 760f;
        private const float RootHeight = 110f;
        private const float BarWidth = 620f;
        private const float BarHeight = 22f;

        private static readonly Color PanelColor = new Color(0f, 0f, 0f, 0.68f);
        private static readonly Color BarBackgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.95f);
        private static readonly Color BarFillColor = new Color(0.0f, 0.95f, 1.0f, 1.0f);
        private static readonly Color TextColor = Color.white;

        public BrightnessHudView(Camera mainCamera, Vector3 localPosition, float visibleSeconds)
        {
            this.mainCamera = mainCamera;
            this.localPosition = localPosition;
            this.visibleSeconds = Mathf.Max(0.1f, visibleSeconds);
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

            rootObject = new GameObject("Q3 Vision Brightness HUD");
            rootObject.transform.SetParent(mainCamera.transform, false);
            rootObject.transform.localPosition = localPosition;
            rootObject.transform.localRotation = Quaternion.identity;
            rootObject.transform.localScale = Vector3.one;

            Canvas canvas = rootObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = mainCamera;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 7000;

            CanvasScaler canvasScaler = rootObject.AddComponent<CanvasScaler>();
            canvasScaler.dynamicPixelsPerUnit = 30f;

            GraphicRaycaster raycaster = rootObject.AddComponent<GraphicRaycaster>();
            raycaster.enabled = false;

            RectTransform rootRect = rootObject.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(RootWidth, RootHeight);
            rootRect.localScale = Vector3.one * CanvasWorldScale;

            Image panelImage = rootObject.AddComponent<Image>();
            panelImage.color = PanelColor;
            panelImage.raycastTarget = false;

            labelText = CreateText(
                parent: rootRect,
                objectName: "Brightness Label",
                anchoredPosition: new Vector2(0f, 26f),
                size: new Vector2(700f, 48f),
                fontSize: 34
            );

            Image barBackground = CreateImage(
                parent: rootRect,
                objectName: "Brightness Bar Background",
                anchoredPosition: new Vector2(0f, -24f),
                size: new Vector2(BarWidth, BarHeight),
                color: BarBackgroundColor
            );

            GameObject fillObject = new GameObject("Brightness Bar Fill");
            fillObject.transform.SetParent(barBackground.transform, false);

            fillRect = fillObject.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0.5f);
            fillRect.anchorMax = new Vector2(0f, 0.5f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.anchoredPosition = new Vector2(-BarWidth * 0.5f, 0f);
            fillRect.sizeDelta = new Vector2(BarWidth * 0.5f, BarHeight);
            fillRect.localScale = Vector3.one;

            Image fillImage = fillObject.AddComponent<Image>();
            fillImage.color = BarFillColor;
            fillImage.raycastTarget = false;

            isCreated = true;
            Hide();
        }

        public void Show(float normalizedLevel, int displayPercent)
        {
            if (!isCreated)
            {
                Create();
            }

            if (rootObject == null || fillRect == null || labelText == null)
            {
                return;
            }

            float safeNormalizedLevel = Mathf.Clamp01(normalizedLevel);
            float fillWidth = Mathf.Lerp(0f, BarWidth, safeNormalizedLevel);

            fillRect.sizeDelta = new Vector2(fillWidth, BarHeight);
            labelText.text = $"Brightness {displayPercent}%";

            rootObject.SetActive(true);
            hideAtTime = Time.time + visibleSeconds;
        }

        public void Tick()
        {
            if (rootObject == null || !rootObject.activeSelf)
            {
                return;
            }

            if (Time.time >= hideAtTime)
            {
                Hide();
            }
        }

        public void Hide()
        {
            if (rootObject == null)
            {
                return;
            }

            rootObject.SetActive(false);
        }

        public void Destroy()
        {
            if (rootObject != null)
            {
                Object.Destroy(rootObject);
            }

            rootObject = null;
            fillRect = null;
            labelText = null;
            isCreated = false;
        }

        private static Image CreateImage(
            RectTransform parent,
            string objectName,
            Vector2 anchoredPosition,
            Vector2 size,
            Color color)
        {
            GameObject imageObject = new GameObject(objectName);
            imageObject.transform.SetParent(parent, false);

            RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;
            rectTransform.localScale = Vector3.one;

            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            return image;
        }

        private static Text CreateText(
            RectTransform parent,
            string objectName,
            Vector2 anchoredPosition,
            Vector2 size,
            int fontSize)
        {
            GameObject textObject = new GameObject(objectName);
            textObject.transform.SetParent(parent, false);

            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;
            rectTransform.localScale = Vector3.one;

            Text text = textObject.AddComponent<Text>();
            text.text = "Brightness 50%";
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = TextColor;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return text;
        }
    }
}