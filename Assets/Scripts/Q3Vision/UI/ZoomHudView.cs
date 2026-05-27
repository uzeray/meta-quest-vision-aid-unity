using UnityEngine;

namespace Q3Vision.UI
{
    public sealed class ZoomHudView
    {
        private readonly Camera mainCamera;
        private readonly Vector3 localPosition;
        private readonly float visibleSeconds;

        private GameObject root;
        private TextMesh zoomText;
        private float hideAtTime;
        private bool isCreated;

        private const float TextScale = 0.010f;
        private const int FontSize = 56;

        public ZoomHudView(Camera mainCamera, Vector3 localPosition, float visibleSeconds)
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

            root = new GameObject("Q3 Vision Zoom HUD");
            root.transform.SetParent(mainCamera.transform, false);

            root.transform.localPosition = localPosition;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            GameObject textObject = new GameObject("Zoom HUD Text");
            textObject.transform.SetParent(root.transform, false);
            textObject.transform.localPosition = Vector3.zero;
            textObject.transform.localRotation = Quaternion.identity;
            textObject.transform.localScale = new Vector3(TextScale, TextScale, TextScale);

            zoomText = textObject.AddComponent<TextMesh>();
            zoomText.text = "Zoom 1.0 x";
            zoomText.anchor = TextAnchor.MiddleCenter;
            zoomText.alignment = TextAlignment.Center;
            zoomText.fontSize = FontSize;
            zoomText.fontStyle = FontStyle.Bold;
            zoomText.color = Color.white;

            MeshRenderer renderer = textObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 5000;
            }

            isCreated = true;
            Hide();
        }

        public void Show(float zoomFactor)
        {
            if (!isCreated)
            {
                Create();
            }

            if (root == null || zoomText == null)
            {
                return;
            }

            zoomText.text = $"Zoom {zoomFactor:0.0} x";
            root.SetActive(true);
            hideAtTime = Time.time + visibleSeconds;
        }

        public void Tick()
        {
            if (root == null || !root.activeSelf)
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
            if (root == null)
            {
                return;
            }

            root.SetActive(false);
        }

        public void Destroy()
        {
            if (root != null)
            {
                Object.Destroy(root);
            }

            root = null;
            zoomText = null;
            isCreated = false;
        }
    }
}