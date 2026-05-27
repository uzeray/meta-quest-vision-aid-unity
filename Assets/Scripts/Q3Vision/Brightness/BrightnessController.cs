using UnityEngine;

namespace Q3Vision.Brightness
{
    public sealed class BrightnessController
    {
        private readonly float minLevel;
        private readonly float defaultLevel;
        private readonly float maxLevel;
        private readonly float changeSpeed;
        private readonly float joystickDeadZone;
        private readonly float minNativeBrightness;
        private readonly float maxNativeBrightness;

        public float Level { get; private set; }

        public BrightnessController(
            float initialLevel,
            float minLevel,
            float defaultLevel,
            float maxLevel,
            float changeSpeed,
            float joystickDeadZone,
            float minNativeBrightness,
            float maxNativeBrightness)
        {
            this.minLevel = Mathf.Clamp(minLevel, 0f, 100f);
            this.defaultLevel = Mathf.Clamp(defaultLevel, this.minLevel, 100f);
            this.maxLevel = Mathf.Clamp(maxLevel, this.defaultLevel, 100f);
            this.changeSpeed = Mathf.Max(1f, changeSpeed);
            this.joystickDeadZone = Mathf.Clamp01(joystickDeadZone);

            this.minNativeBrightness = Mathf.Clamp(minNativeBrightness, -0.75f, 0f);
            this.maxNativeBrightness = Mathf.Clamp(maxNativeBrightness, 0f, 0.75f);

            Level = Mathf.Clamp(initialLevel, this.minLevel, this.maxLevel);
        }

        public bool TryApplyJoystickBrightness(float joystickX, float deltaTime)
        {
            if (Mathf.Abs(joystickX) <= joystickDeadZone)
            {
                return false;
            }

            float oldLevel = Level;
            float delta = joystickX * changeSpeed * deltaTime;

            Level = Mathf.Clamp(Level + delta, minLevel, maxLevel);

            return Mathf.Abs(Level - oldLevel) >= 0.05f;
        }

        public float GetNormalizedLevel()
        {
            return Mathf.InverseLerp(minLevel, maxLevel, Level);
        }

        public int GetDisplayPercent()
        {
            return Mathf.RoundToInt(Level);
        }

        public float GetNativePassthroughBrightness()
        {
            if (Mathf.Approximately(Level, defaultLevel))
            {
                return 0f;
            }

            if (Level > defaultLevel)
            {
                float t = Mathf.InverseLerp(defaultLevel, maxLevel, Level);
                return Mathf.Lerp(0f, maxNativeBrightness, t);
            }

            float darkenT = Mathf.InverseLerp(defaultLevel, minLevel, Level);
            return Mathf.Lerp(0f, minNativeBrightness, darkenT);
        }

        public float GetMagnifierBrightnessMultiplier()
        {
            float nativeBrightness = GetNativePassthroughBrightness();

            if (nativeBrightness > 0f)
            {
                return Mathf.Lerp(1.0f, 1.15f, nativeBrightness / 0.75f);
            }

            if (nativeBrightness < 0f)
            {
                return Mathf.Lerp(1.0f, 0.85f, Mathf.Abs(nativeBrightness) / 0.75f);
            }

            return 1.0f;
        }
    }
}