using EFT;
using IRLWeaponHandling.Configuration;
using IRLWeaponHandling.Core;
using UnityEngine;

namespace IRLWeaponHandling.Systems;

internal enum ELocomotionGait
{
    Idle,
    SlowWalk,
    Walk,
    Jog,
    Sprint,
    ExhaustedSprint,
    Crouch,
    Prone
}

internal static class LocomotionGaitSystem
{
    private static ELocomotionGait _currentGait;
    private static float _cadence;
    private static float _cameraAmplitude;
    private static float _weaponFollow = 1f;
    private static float _momentum = 1f;
    private static float _phase;
    private static Vector3 _cameraOffset;

    public static ELocomotionGait CurrentGait => _currentGait;
    public static float WeaponFollowMultiplier => _weaponFollow;
    public static float MomentumMultiplier => _momentum;
    /// <summary>Continuously advancing stride phase — every π crossing is a footfall.</summary>
    public static float StepPhase => _phase;

    public static void Tick(Player player, float deltaTime)
    {
        if (player?.MovementContext?.CurrentState == null || HandlingConfig.GaitEnabled?.Value != true)
        {
            Reset();
            return;
        }

        float dt = Mathf.Min(deltaTime, 1f / 30f);
        ELocomotionGait targetGait = ResolveGait(player);
        Profile(targetGait, out float targetCadence, out float targetCamera, out float targetWeaponFollow, out float targetMomentum);

        float transition = HandlingConfig.GaitTransitionSpeed.Value;
        float t = 1f - Mathf.Exp(-transition * dt);
        _cadence = Mathf.Lerp(_cadence, targetCadence, t);
        _cameraAmplitude = Mathf.Lerp(_cameraAmplitude, targetCamera, t);
        _weaponFollow = Mathf.Lerp(_weaponFollow, targetWeaponFollow, t);
        _momentum = Mathf.Lerp(_momentum, targetMomentum, t);
        _currentGait = targetGait;

        float speed = new Vector2(player.Velocity.x, player.Velocity.z).magnitude;
        float movementBlend = Mathf.Clamp01(speed / 0.8f);
        _phase += dt * _cadence * Mathf.PI * 2f * Mathf.Lerp(0.6f, 1.25f, Mathf.Clamp01(speed / 5f));

        float intensity = HandlingConfig.GaitIntensity.Value;
        float fatigue = 1f - Mathf.Min(HandlingState.Stamina01, HandlingState.Oxygen01);
        float irregularity = targetGait == ELocomotionGait.ExhaustedSprint ? 1f + fatigue * 0.35f : 1f;
        float amplitude = _cameraAmplitude * intensity * movementBlend * irregularity * InjuryHandlingSystem.GaitInstability;
        float adsScale = HandlingState.IsAiming ? 0.28f : 1f;
        Vector3 targetOffset = new(
            Mathf.Sin(_phase * 2f) * amplitude * 0.35f,
            0f,
            Mathf.Sin(_phase) * amplitude * 0.55f + InjuryHandlingSystem.LegBias * 1.2f * movementBlend);
        _cameraOffset = Vector3.Lerp(_cameraOffset, targetOffset * adsScale, 1f - Mathf.Exp(-10f * dt));
    }

    public static Vector3 ModifyCamera(Vector3 rotation)
    {
        return rotation + _cameraOffset;
    }

    public static void Reset()
    {
        _currentGait = ELocomotionGait.Idle;
        _cadence = 0f;
        _cameraAmplitude = 0f;
        _weaponFollow = 1f;
        _momentum = 1f;
        _phase = 0f;
        _cameraOffset = Vector3.zero;
    }

    private static ELocomotionGait ResolveGait(Player player)
    {
        MovementContext movement = player.MovementContext;
        EPlayerState state = movement.CurrentState.Name;
        if (state == EPlayerState.ProneIdle || state == EPlayerState.ProneMove || state == EPlayerState.Transit2Prone)
            return ELocomotionGait.Prone;
        if (!MotionTracker.IsMoving)
            return ELocomotionGait.Idle;
        if (movement.IsSprintEnabled || state == EPlayerState.Sprint)
        {
            if (HandlingState.Stamina01 < 0.28f || HandlingState.Oxygen01 < 0.32f)
                return ELocomotionGait.ExhaustedSprint;
            return ELocomotionGait.Sprint;
        }
        if (movement.PoseLevel < 0.65f)
            return ELocomotionGait.Crouch;

        float normalizedSpeed = Mathf.Clamp01(movement.CharacterMovementSpeed / Mathf.Max(movement.MaxSpeed, 0.1f));
        if (normalizedSpeed < 0.28f)
            return ELocomotionGait.SlowWalk;
        if (normalizedSpeed < 0.62f)
            return ELocomotionGait.Walk;
        return ELocomotionGait.Jog;
    }

    private static void Profile(ELocomotionGait gait, out float cadence, out float camera, out float weaponFollow, out float momentum)
    {
        switch (gait)
        {
            case ELocomotionGait.SlowWalk:
                cadence = 0.9f; camera = 0.12f; weaponFollow = 0.95f; momentum = 0.8f; break;
            case ELocomotionGait.Walk:
                cadence = 1.55f; camera = 0.26f; weaponFollow = 0.88f; momentum = 1f; break;
            case ELocomotionGait.Jog:
                cadence = 2.15f; camera = 0.48f; weaponFollow = 0.76f; momentum = 1.18f; break;
            case ELocomotionGait.Sprint:
                cadence = 2.75f; camera = 0.8f; weaponFollow = 0.62f; momentum = 1.35f; break;
            case ELocomotionGait.ExhaustedSprint:
                cadence = 2.45f; camera = 1.05f; weaponFollow = 0.5f; momentum = 1.55f; break;
            case ELocomotionGait.Crouch:
                cadence = 1.1f; camera = 0.18f; weaponFollow = 0.82f; momentum = 0.9f; break;
            case ELocomotionGait.Prone:
                cadence = 0.65f; camera = 0.1f; weaponFollow = 0.72f; momentum = 0.7f; break;
            default:
                cadence = 0f; camera = 0f; weaponFollow = 1f; momentum = 0.65f; break;
        }
    }
}
