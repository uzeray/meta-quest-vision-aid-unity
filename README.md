# Q3 Vision Aid — Mixed-Reality Low-Vision Assistant (Unity, Meta Quest)

**A Meta Quest passthrough app that gives low-vision users adjustable brightness/contrast and a controller-driven zoom magnifier over their real surroundings.**

## Purpose

Built for people with **low vision or reduced contrast sensitivity** who need the real world made brighter, higher-contrast, or magnified to see it clearly — using the Quest headset's own color passthrough camera as the "video see-through" source, rather than a phone or external camera. Where [android-dual-camera-vision-pipeline-cpp](https://github.com/uzeray/android-dual-camera-vision-pipeline-cpp) targets restricted field of view and [DonosCam](https://github.com/uzeray/donoscam) targets a phone-based headset, this project targets **standalone Meta Quest hardware** (Quest 2 / Pro / 3 / 3S) and its native passthrough pipeline.

**What it runs on:** Meta Quest devices, using Meta's `OVRPassthroughLayer` (native color passthrough) and `PassthroughCameraAccess` (raw passthrough camera texture) APIs — configured with hand-tracking, the passthrough camera permission (`HEADSET_CAMERA`), and passthrough declared as an optional hardware feature so the app degrades gracefully on devices without it.

**How it's controlled:** the Quest touch controller's left thumbstick — vertical axis adjusts zoom, horizontal axis adjusts brightness — with a face/select/start button to reset zoom to 1×. On-screen HUDs briefly appear whenever a value changes, then fade out, so the wearer gets feedback without a persistent overlay cluttering their view.

## How it works (logic and modes)

The app runs in one of two modes (`VisionMode`):

1. **Native Passthrough** — the default. The Quest's own passthrough layer is shown directly, with brightness/contrast/saturation adjusted in real time via `OVRPassthroughLayer.SetBrightnessContrastSaturation(...)`. Brightness maps a 0–100 user-facing level onto a native brightness range of **-0.75 (darker) to +0.75 (brighter)** around a neutral default of 50 — so users get a wide, comfortable adjustment range without ever fully clipping the image.
2. **Camera Magnifier** — activated automatically once the user zooms in past 1×. A floating, screen-space panel anchored in front of the camera renders a **live, cropped-and-zoomed view of the passthrough camera feed** (via `PassthroughCameraAccess.GetTexture()`), with aspect-correct UV cropping so the zoomed image isn't stretched, and a two-tone border that appears once zoom passes a threshold to make the magnifier panel's edges clearly visible against the real world behind it.

Both modes share the same brightness setting — `BrightnessController` computes a small brightness multiplier for the magnifier panel too, so the magnified view and the native passthrough stay visually consistent.

### Component breakdown

| Component | Responsibility |
|---|---|
| `Q3VisionAppController` | Top-level orchestrator: owns the current `VisionMode`, wires controller input to zoom/brightness, drives the HUDs and magnifier panel |
| `ZoomController` | Pure logic: clamps/accumulates zoom (1×–8× by default) from joystick input, exposes a reset |
| `BrightnessController` | Pure logic: maps a 0–100 level to native passthrough brightness (-0.75…+0.75) and a magnifier brightness multiplier |
| `CameraMagnifierView` | Builds and updates the floating magnifier panel: world-space canvas, bordered `RawImage`, aspect-correct zoom/crop math |
| `ZoomHudView` / `BrightnessHudView` | Temporary on-screen indicators that appear on change and auto-hide after a few seconds |
| `RemoteInputReader` / `RemoteInputFrame` | Reads the Quest controller (via Unity's Input System `Gamepad` abstraction) into a simple input snapshot |

## Status

The passthrough brightness/contrast pipeline, joystick-driven zoom, camera magnifier panel, and both HUDs are implemented and wired together in `Q3VisionAppController`. Project-level XR/Oculus configuration (Quest 2/Pro/3/3S support, hand-tracking, passthrough feature declaration) is in place. This is an active prototype rather than a store-ready release.

## Stack

Unity · Meta XR / Oculus SDK (`OVRPassthroughLayer`, `PassthroughCameraAccess`) · Unity Input System · C#

## Author

Ünal Zeray
