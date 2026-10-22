using EFT;
using EFT.CameraControl;
using IRLWeaponHandling.Configuration;
using IRLWeaponHandling.Core;
using UnityEngine;

namespace IRLWeaponHandling.Systems;

/// <summary>
/// Dynamic field of view. Applies a continuous multiplier to the FPS
/// camera's FOV every frame: slightly narrower while aiming for focus,
/// plus an adrenaline tunnel pinch.
///
/// EFT only writes camera FOV during transitions (CameraManager.SetFov's
/// coroutine), so a one-shot scale on the requested value is imperceptible.
/// Instead we track the camera's FOV directly: if it changed since our last
/// write, the game produced a new base; otherwise our own write is undone
/// before re-applying the factor. The result stays multiplied until the
/// factor drifts back to 1.
/// </summary>
internal static class DynamicFovSystem
{
    private static float _factor = 1f;
    private static float _baseFov;
    private static float _appliedFov;
    private static bool _hasCamState;
    private static bool _aimHeld;
    private static float _aimHoldTime;

    // Mounted/bipod micro-movement flickers IsAiming each frame; without a
    // dwell the ADS factor retriggers on every direction change and the
    // camera visibly re-zooms.
    private const float AimDwellSeconds = 0.18f;

    public static float Factor => _factor;

    public static void Tick(Player player, float deltaTime)
    {
        if (player == null || HandlingConfig.DynamicFovEnabled?.Value != true ||
            HandlingConfig.Enabled?.Value != true)
        {
            Reset();
            return;
        }

        float dt = Mathf.Min(deltaTime, 1f / 30f);

        // Mounted/bipod: IsAiming flickers on every direction change and the
        // dwell isn't enough — skip the ADS zoom entirely while mounted.
        bool mounted = false;
        try { mounted = player.ProceduralWeaponAnimation != null && player.ProceduralWeaponAnimation.IsMountedState; } catch { }
        bool aiming = HandlingState.IsAiming && !mounted;

        // Hysteresis: aim must be sustained (or released) for the dwell
        // window before the FOV target switches.
        if (aiming != _aimHeld)
        {
            _aimHoldTime += dt;
            if (_aimHoldTime >= AimDwellSeconds)
            {
                _aimHeld = aiming;
                _aimHoldTime = 0f;
            }
        }
        else
        {
            _aimHoldTime = 0f;
        }

        float target = 1f;
        if (_aimHeld)
            target = 1f + HandlingConfig.DynamicFovAds.Value * 0.01f;

        target *= AdrenalineSystem.FovFactor;
        _factor = Mathf.Lerp(_factor, target, 1f - Mathf.Exp(-5f * dt));
        if (Mathf.Abs(_factor - 1f) < 0.0005f && target == 1f)
            _factor = 1f;

        Apply();
    }

    private static void Apply()
    {
        Camera cam = CameraManager.Instance?.Camera;
        if (cam == null)
        {
            _hasCamState = false;
            return;
        }

        // If the camera FOV is unchanged since our last write, the value in
        // it IS our applied result — restore the base. If it differs, the
        // game wrote a fresh value (transition, settings change) — adopt it.
        _baseFov = (_hasCamState && Mathf.Approximately(cam.fieldOfView, _appliedFov))
            ? _baseFov
            : cam.fieldOfView;

        float outFov = _baseFov * _factor;
        if (!Mathf.Approximately(cam.fieldOfView, outFov))
            cam.fieldOfView = outFov;
        _appliedFov = cam.fieldOfView;
        _hasCamState = true;
    }

    public static void Reset()
    {
        if (_hasCamState)
        {
            Camera cam = CameraManager.Instance?.Camera;
            if (cam != null)
                cam.fieldOfView = _baseFov;
            _hasCamState = false;
        }
        _factor = 1f;
        _aimHeld = false;
        _aimHoldTime = 0f;
    }
}
