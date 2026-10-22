using EFT;
using IRLWeaponHandling.Configuration;
using IRLWeaponHandling.Core;
using UnityEngine;

namespace IRLWeaponHandling.Systems;

/// <summary>
/// Turn-in-place footwork. While standing still, mouse yaw is absorbed by
/// the waist/shoulders first: a signed "torso twist" accumulates against the
/// feet. Small turns produce only subtle camera counter-roll and extra weapon
/// lag; once the twist exceeds the step threshold the feet reposition, which
/// fires a short weight-scaled step pulse that dips the camera and swings the
/// weapon. Heavy gear delays the step (more twist) and makes it heavier; ADS
/// steps sooner and displaces the weapon more.
/// </summary>
internal static class TurnInPlaceSystem
{
    private const float MaxTwist = 85f;
    private const float StepDuration = 0.45f;
    private const float StationarySpeed = 0.55f;

    private static float _twist;
    private static float _twistDisplay;
    private static float _stepPhase;
    private static float _stepDirection;
    private static float _stepStrength;
    private static Vector3 _cameraOffset;
    private static Vector3 _weaponEuler;
    private static Vector3 _weaponPos;
    private static float _weaponFollow = 1f;

    public static float WeaponFollowMultiplier => _weaponFollow;
    public static Quaternion WeaponRotationOffset => Quaternion.Euler(_weaponEuler);
    public static Vector3 WeaponPosOffset => _weaponPos;

    public static void Tick(Player player, float deltaTime)
    {
        if (deltaTime <= 0f || player?.MovementContext?.CurrentState == null ||
            HandlingConfig.TurnInPlaceEnabled?.Value != true)
        {
            Reset();
            return;
        }

        MovementContext mc = player.MovementContext;
        EPlayerState state = mc.CurrentState.Name;
        bool prone = state == EPlayerState.ProneIdle || state == EPlayerState.ProneMove || state == EPlayerState.Transit2Prone;
        float speed = new Vector2(player.Velocity.x, player.Velocity.z).magnitude;
        bool canTwist = !prone && !mc.IsInMountedState && speed < StationarySpeed &&
            !HandlingState.IsSprinting && !HandlingState.IsHoldingBreath;

        float dt = Mathf.Min(deltaTime, 1f / 30f);

        // Accumulate signed torso twist from real yaw input while stationary.
        // Twist unwinds toward zero as soon as the feet start moving again.
        if (canTwist)
        {
            _twist += MotionTracker.YawRate * deltaTime;
            _twist = Mathf.Clamp(_twist, -MaxTwist, MaxTwist);
        }
        else
        {
            _twist = Mathf.Lerp(_twist, 0f, 1f - Mathf.Exp(-6f * dt));
        }

        // Body mass: heavier kit plants the feet longer and steps harder.
        PhysicalBase physical = player.Physical;
        float bodyWeight = physical == null ? 0f :
            Mathf.Clamp01(Mathf.Clamp01(physical.Overweight) * 0.65f + Mathf.Clamp01(physical.Inertia) * 0.55f);
        bool crouched = mc.PoseLevel < 0.65f;

        float threshold = HandlingConfig.TurnInPlaceThreshold.Value;
        threshold *= Mathf.Lerp(1f, 1.45f, bodyWeight);
        if (HandlingState.IsAiming) threshold *= 0.8f;
        if (crouched) threshold *= 1.15f;

        if (Mathf.Abs(_twist) > threshold && _stepPhase <= 0f && canTwist)
        {
            _stepPhase = 1f;
            _stepDirection = Mathf.Sign(_twist);
            _stepStrength = Mathf.Lerp(0.75f, 1.35f, bodyWeight) *
                (HandlingState.IsAiming ? 1.25f : 1f) * (crouched ? 0.7f : 1f);
            _twist = 0f;
        }
        if (_stepPhase > 0f)
            _stepPhase = Mathf.Max(0f, _stepPhase - dt / StepDuration);

        // Step pulse: fast weight drop, slower recovery — sin envelope.
        float stepT = 1f - _stepPhase;
        float envelope = _stepPhase > 0f ? Mathf.Sin(stepT * Mathf.PI) : 0f;
        float step = envelope * _stepStrength * _stepDirection;
        float stepDown = envelope * _stepStrength;

        float intensity = HandlingConfig.TurnInPlaceIntensity.Value;
        _twistDisplay = Mathf.Lerp(_twistDisplay, _twist, 1f - Mathf.Exp(-8f * dt));

        // Camera: slight counter-roll while the torso stays twisted, plus a
        // dip and bank while the feet actually reposition.
        float adsCamera = HandlingState.IsAiming ? 0.55f : 1f;
        Vector3 camTarget = new Vector3(
            stepDown * 1.1f,
            step * 0.9f,
            (_twistDisplay * -0.02f + step * 1.3f) * adsCamera) * intensity;
        _cameraOffset = Vector3.Lerp(_cameraOffset, camTarget, 1f - Mathf.Exp(-10f * dt));

        // Weapon: lags extra while the torso is twisted, and swings with the
        // step — muzzle trails the pivot direction, grip dips with the body.
        // While ADS the dip components are suppressed — sight alignment must
        // not sink on every footwork step.
        float adsWeapon = HandlingState.IsAiming ? 1.4f : 1f;
        float dipMul = HandlingState.IsAiming ? 0f : 1f;
        Vector3 weaponTarget = new Vector3(
            stepDown * 1.4f * adsWeapon * dipMul,
            (_twistDisplay * -0.035f + step * 2.2f) * adsWeapon,
            (_twistDisplay * 0.025f + step * 0.8f)) * intensity;
        _weaponEuler = Vector3.Lerp(_weaponEuler, weaponTarget, 1f - Mathf.Exp(-10f * dt));
        _weaponPos = Vector3.Lerp(_weaponPos,
            new Vector3(0f, -stepDown * 0.012f * adsWeapon * dipMul, 0f) * intensity,
            1f - Mathf.Exp(-10f * dt));

        _weaponFollow = Mathf.Lerp(_weaponFollow, 1f - stepDown * 0.3f * intensity * dipMul, 1f - Mathf.Exp(-12f * dt));
    }

    public static Vector3 ModifyCamera(Vector3 rotation)
    {
        return rotation + _cameraOffset;
    }

    public static void Reset()
    {
        _twist = 0f;
        _twistDisplay = 0f;
        _stepPhase = 0f;
        _stepDirection = 0f;
        _stepStrength = 0f;
        _cameraOffset = Vector3.zero;
        _weaponEuler = Vector3.zero;
        _weaponPos = Vector3.zero;
        _weaponFollow = 1f;
    }
}
