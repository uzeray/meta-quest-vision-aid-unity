namespace Q3Vision.Input
{
    public readonly struct RemoteInputFrame
    {
        public readonly bool HasGamepad;
        public readonly float ZoomAxisY;
        public readonly float BrightnessAxisX;
        public readonly bool ResetZoomPressed;

        public RemoteInputFrame(
            bool hasGamepad,
            float zoomAxisY,
            float brightnessAxisX,
            bool resetZoomPressed)
        {
            HasGamepad = hasGamepad;
            ZoomAxisY = zoomAxisY;
            BrightnessAxisX = brightnessAxisX;
            ResetZoomPressed = resetZoomPressed;
        }

        public static RemoteInputFrame Empty => new RemoteInputFrame(
            hasGamepad: false,
            zoomAxisY: 0f,
            brightnessAxisX: 0f,
            resetZoomPressed: false
        );
    }
}