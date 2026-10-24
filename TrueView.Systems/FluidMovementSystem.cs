using EFT;
using TrueView.Configuration;
using TrueView.Core;
using UnityEngine;

namespace TrueView.Systems;

/// <summary>
/// Fluid player movement system. Adds natural, organic motion on top of
/// the existing momentum/lean/camera-lag systems:
///
///  - Landing impact: camera dips down when the player lands from a fall,
///    with intensity proportional to fall speed/duration.
///  - Crouch dip: a small camera lower when entering crouch, smoothed.
///
/// All effects are additive on the head rotation (pitch/yaw/roll) and
/// fully configurable. They are reduced while aiming for stability.
/// </summary>
internal static class FluidMovementSystem
{
    private const float MaxDeltaTime = 1f / 15f;

    // Landing impact state (spring)
    private static float _landingDip;
    private static float _landingDipVel;

    // Crouch smoothing
    private static float _crouchBlend;
    private static float _crouchBlendVel;

    // Previous grounded state for landing detection
    private static bool _wasGrounded;
    private static float _prevFreefallTime;

    // Aim blend — reduces all effects while aiming
    private static float _aimBlend;

    /// <summary>
    /// Apply the fluid movement offsets to the head rotation.
    /// Called from Patch_SetHeadRotation after all other systems.
    /// </summary>
    public static Vector3 Modify(Vector3 headRotation)
    {
        if (!HandlingConfig.FluidMovementEnabled.Value)
            return headRotation;

        // Pitch: landing dip (downward) + crouch dip
        headRotation.x += _landingDip + _crouchBlend;

        return headRotation;
    }

    public static void Tick(Player player, float deltaTime)
    {
        deltaTime = Mathf.Clamp(deltaTime, 0f, MaxDeltaTime);
        if (deltaTime <= 0f || player == null || !player.IsYourPlayer)
        {
            Reset();
            return;
        }

        if (!HandlingConfig.FluidMovementEnabled.Value)
        {
            Reset();
            return;
        }

        // Aim blend — smoothly reduce effects while aiming
        float aimTarget = HandlingState.IsAiming ? 1f : 0f;
        _aimBlend = Mathf.Lerp(_aimBlend, aimTarget, 1f - Mathf.Exp(-deltaTime * 6f));
        float aimReduce = 1f - _aimBlend * Mathf.Clamp01(HandlingConfig.FluidAimReduction.Value);

        MovementContext mc = player.MovementContext;
        if (mc == null)
        {
            Reset();
            return;
        }

        // --- Landing impact ---
        bool isGrounded = mc.IsGrounded;
        float freefallTime = mc.FreefallTime;

        if (isGrounded && !_wasGrounded && _prevFreefallTime > 0.15f)
        {
            // Just landed — compute impact strength from freefall duration
            float impactStrength = Mathf.Clamp01(_prevFreefallTime / 1.5f);
            float dipAmount = impactStrength * Mathf.Clamp01(HandlingConfig.FluidLandingDip.Value) * aimReduce;
            // Apply impulse to the landing dip spring
            _landingDipVel -= dipAmount * 8f;
        }

        _wasGrounded = isGrounded;
        _prevFreefallTime = isGrounded ? 0f : freefallTime;

        // Spring the landing dip back to zero
        float landingFreq = Mathf.Max(2f, HandlingConfig.FluidLandingRecovery.Value);
        Spring(ref _landingDip, ref _landingDipVel, 0f, landingFreq, deltaTime);

        // --- Crouch dip ---
        // PoseLevel: 1 = standing, ~0.5 = crouch, 0 = prone.
        // We detect crouch by checking if pose level is below 0.75.
        float poseLevel = 1f;
        try
        {
            poseLevel = mc.PoseLevel;
        }
        catch { }

        // Crouch factor: 0 when standing (poseLevel >= 0.75), 1 when fully crouched (poseLevel <= 0.4)
        float crouchFactor = Mathf.InverseLerp(0.75f, 0.4f, poseLevel);
        float crouchTarget = crouchFactor * Mathf.Clamp01(HandlingConfig.FluidCrouchDip.Value) * aimReduce;
        float crouchFreq = Mathf.Max(1f, HandlingConfig.FluidCrouchSpeed.Value);
        Spring(ref _crouchBlend, ref _crouchBlendVel, crouchTarget, crouchFreq, deltaTime);
    }

    public static void Reset()
    {
        _landingDip = 0f;
        _landingDipVel = 0f;
        _crouchBlend = 0f;
        _crouchBlendVel = 0f;
        _wasGrounded = true;
        _prevFreefallTime = 0f;
        _aimBlend = 0f;
    }

    /// <summary>
    /// Critically-damped spring for smooth motion.
    /// </summary>
    private static void Spring(ref float current, ref float velocity, float target, float frequency, float deltaTime)
    {
        float k = frequency * frequency;
        float d = 1.4f * frequency;
        velocity += ((target - current) * k - velocity * d) * deltaTime;
        current += velocity * deltaTime;
    }
}
