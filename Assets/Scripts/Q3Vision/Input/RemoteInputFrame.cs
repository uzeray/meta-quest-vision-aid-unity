namespace Q3Vision.Input
{
    public readonly struct RemoteInputFrame
    {
        public readonly bool HasGamepad;
        public readonly float ZoomAxisY;
        public readonly bool ResetZoomPressed;

        public RemoteInputFrame(bool hasGamepad, float zoomAxisY, bool resetZoomPressed)
        {
            HasGamepad = hasGamepad;
            ZoomAxisY = zoomAxisY;
            ResetZoomPressed = resetZoomPressed;
        }

        public static RemoteInputFrame Empty => new RemoteInputFrame(false, 0f, false);
    }
}