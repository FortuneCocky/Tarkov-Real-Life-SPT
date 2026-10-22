using EFT;
using IRLWeaponHandling.Configuration;
using IRLWeaponHandling.Core;
using UnityEngine;

namespace IRLWeaponHandling.Systems;

/// <summary>
/// Head-mounted camera. Samples the character's head bone and applies its
/// short-term motion (steps, lean, body sway) to the camera so the view moves
/// with the body. A low-pass baseline removes slow drift. Vertical movement and
/// yaw are removed so aim direction and up/down motion are untouched. Fades out completely while aiming down sights or leaning.
/// </summary>
internal static class HeadAttachSystem
{
    private const float MaxPositionOffset = 0.12f;
    private const float MaxRotationDegrees = 6f;
    private const float BaselineSpeed = 3f;
    private const float FadeSpeed = 10f;

    private static float _blend = 1f;

    private static Vector3 _basePos;
    private static Quaternion _baseRot = Quaternion.identity;
    private static bool _initialized;

    // Offset applied last frame, so it is never stacked on top of itself if the
    // game doesn't rewrite the camera position every frame.
    private static Vector3 _lastAppliedDelta;
    private static Vector3 _lastResultPos;
    private static Vector3 _shotShake;
    private static Vector3 _shotShakeVelocity;
    private static Vector3 _posKick;
    private static Vector3 _posKickVelocity;
    private static Vector3 _stabilizedHeadEuler;
    private static Vector3 _directionLag;
    private static Vector3 _weaponCoupling;
    private static Vector3 _injuryDrift;
    private static float _injuryTime;
    private static int _lastShakeFrame = -1;
    private static int _lastPosKickFrame = -1;

    public static void AddShotImpulse(float yaw, float pitch)
    {
        if (HandlingConfig.HeadAttachEnabled?.Value != true ||
            HandlingConfig.HeadShotShake == null || HandlingConfig.HeadShotShake.Value <= 0f)
            return;

        float amount = HandlingConfig.HeadShotShake.Value;
        Vector3 impulse = new(-pitch * 0.9f, yaw * 0.55f, -yaw * 0.35f);
        _shotShake += impulse * amount;
        _shotShake = Vector3.ClampMagnitude(_shotShake, 4f * amount);
        _shotShakeVelocity += impulse * (HandlingConfig.HeadShotShakeRecovery.Value * 0.7f * amount);
        _shotShakeVelocity = Vector3.ClampMagnitude(_shotShakeVelocity, 30f * amount);

        // Muzzle jolt: a small translational shove straight back — the head
        // gets pushed rearward, not just rotated. Camera-local -Z is backward.
        if (HandlingConfig.MuzzleJoltEnabled?.Value == true)
        {
            float jolt = HandlingConfig.MuzzleJoltAmount?.Value ?? 1f;
            float kick = 0.0035f * jolt * amount;
            _posKickVelocity += new Vector3(-pitch * 0.0004f * jolt, 0f, -kick * 9f);
            _posKickVelocity = Vector3.ClampMagnitude(_posKickVelocity, 0.09f);
        }
    }

    /// <summary>
    /// Direct euler impulse into the shot-shake spring — used by hit reactions
    /// where the offset direction is already computed.
    /// </summary>
    public static void AddHitImpulse(Vector3 euler)
    {
        if (HandlingConfig.HeadAttachEnabled?.Value != true)
            return;

        _shotShake += euler;
        _shotShake = Vector3.ClampMagnitude(_shotShake, 6f);
        _shotShakeVelocity += euler * 7f;
        _shotShakeVelocity = Vector3.ClampMagnitude(_shotShakeVelocity, 35f);
    }

    public static Vector3 ApplyShotShake(Vector3 headRotation)
    {
        if (HandlingConfig.HeadAttachEnabled?.Value != true)
            return headRotation;

        if (_lastShakeFrame != Time.frameCount)
        {
            _lastShakeFrame = Time.frameCount;
            float dt = Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);
            float recovery = HandlingConfig.HeadShotShakeRecovery.Value;
            float shakeAmount = _shotShake.magnitude;
            float frequency = Mathf.Lerp(recovery * 0.38f, recovery, Mathf.Clamp01(shakeAmount / 1.5f));
            float k = frequency * frequency;
            float d = 2f * frequency;
            _shotShakeVelocity += (-_shotShake * k - _shotShakeVelocity * d) * dt;
            _shotShake += _shotShakeVelocity * dt;
            if (_shotShake.sqrMagnitude < 0.000001f && _shotShakeVelocity.sqrMagnitude < 0.000001f)
            {
                _shotShake = Vector3.zero;
                _shotShakeVelocity = Vector3.zero;
            }

            float yawNormalized = Mathf.Clamp(MotionTracker.YawRate / 360f, -1f, 1f);
            Vector3 directionTarget = new(0f, -yawNormalized * 1.5f, yawNormalized * 0.8f);
            directionTarget *= HandlingConfig.HeadDirectionLag.Value;
            _directionLag = Vector3.Lerp(_directionLag, directionTarget, 1f - Mathf.Exp(-6f * dt));

            float couplingScale = HandlingState.IsAiming ? 0.08f : 0.02f;
            Vector2 weaponOffset = ProceduralHipSystem.ControlOffset;
            Vector3 couplingTarget = new(weaponOffset.y, weaponOffset.x, 0f);
            couplingTarget *= HandlingConfig.HeadWeaponCoupling.Value * couplingScale;
            _weaponCoupling = Vector3.Lerp(_weaponCoupling, couplingTarget, 1f - Mathf.Exp(-9f * dt));

            float injury = InjuryHandlingSystem.HeadPenalty + InjuryHandlingSystem.TorsoPenalty * 0.35f;
            _injuryTime += dt * Mathf.Lerp(0.6f, 1.4f, Mathf.Clamp01(injury));
            Vector3 injuryTarget = new(
                (Mathf.PerlinNoise(_injuryTime, 41.3f) - 0.5f) * injury * 0.9f,
                (Mathf.PerlinNoise(73.1f, _injuryTime) - 0.5f) * injury * 0.7f,
                (Mathf.PerlinNoise(_injuryTime * 0.7f, 19.9f) - 0.5f) * injury * 1.1f);
            _injuryDrift = Vector3.Lerp(_injuryDrift, injuryTarget, 1f - Mathf.Exp(-3f * dt));
        }

        return headRotation + _shotShake + _directionLag + _weaponCoupling + _injuryDrift;
    }

    public static void Apply(Player player, Transform camera)
    {
        if (!HandlingConfig.HeadAttachEnabled.Value || player == null || camera == null || camera.parent == null)
        {
            _initialized = false;
            return;
        }

        Transform head = player.PlayerBones?.Head?.Original;
        Transform root = player.Transform?.Original;
        if (head == null || root == null)
        {
            _initialized = false;
            return;
        }

        float dt = Mathf.Min(Time.deltaTime, 1f / 30f);
        Vector3 localPos = root.InverseTransformPoint(head.position);
        Quaternion localRot = Quaternion.Inverse(root.rotation) * head.rotation;

        if (!_initialized)
        {
            _basePos = localPos;
            _baseRot = localRot;
            _lastAppliedDelta = Vector3.zero;
            _lastResultPos = camera.localPosition;
            _initialized = true;
            return;
        }

        float k = 1f - Mathf.Exp(-BaselineSpeed * dt);
        _basePos = Vector3.Lerp(_basePos, localPos, k);
        _baseRot = Quaternion.Slerp(_baseRot, localRot, k);

        bool suppressed = HandlingState.IsAiming || Mathf.Abs(MotionTracker.Lean) > 0.05f;
        _blend = Mathf.MoveTowards(_blend, suppressed ? 0f : 1f, FadeSpeed * dt);

        float posScale = HandlingConfig.HeadAttachPosition.Value * _blend;
        float rotScale = HandlingConfig.HeadAttachRotation.Value * _blend;
        Vector3 basePosition = camera.localPosition;
        if ((basePosition - _lastResultPos).sqrMagnitude < 1e-10f)
            basePosition -= _lastAppliedDelta;

        Vector3 positionDelta = Vector3.zero;
        if (posScale > 0.001f)
        {
            Vector3 delta = localPos - _basePos;
            delta.y = 0f;
            Vector3 worldDelta = Vector3.ClampMagnitude(root.TransformVector(delta) * posScale, MaxPositionOffset * posScale);
            positionDelta = camera.parent.InverseTransformVector(worldDelta);
        }

        // Muzzle jolt spring — once per frame, camera-local space.
        if (_lastPosKickFrame != Time.frameCount)
        {
            _lastPosKickFrame = Time.frameCount;
            float kdt = Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);
            _posKickVelocity += (-_posKick * 400f - _posKickVelocity * 26f) * kdt;
            _posKick += _posKickVelocity * kdt;
            if (_posKick.sqrMagnitude < 1e-10f && _posKickVelocity.sqrMagnitude < 1e-10f)
            {
                _posKick = Vector3.zero;
                _posKickVelocity = Vector3.zero;
            }
        }
        positionDelta += _posKick;

        _lastAppliedDelta = positionDelta;
        camera.localPosition = basePosition + positionDelta;
        _lastResultPos = camera.localPosition;

        if (rotScale > 0.001f)
        {
            Vector3 euler = (localRot * Quaternion.Inverse(_baseRot)).eulerAngles;
            euler.x = Mathf.DeltaAngle(0f, euler.x);
            euler.y = 0f;
            euler.z = Mathf.DeltaAngle(0f, euler.z);
            _stabilizedHeadEuler = Vector3.Lerp(_stabilizedHeadEuler, euler, 1f - Mathf.Exp(-6f * dt));
            float stabilization = HandlingConfig.HeadEyeStabilization.Value * Mathf.Clamp01(1f - InjuryHandlingSystem.HeadPenalty * 0.7f);
            euler = Vector3.Lerp(euler, _stabilizedHeadEuler, stabilization);
            float max = MaxRotationDegrees * rotScale;
            euler.x = Mathf.Clamp(euler.x * rotScale, -max, max);
            euler.z = Mathf.Clamp(euler.z * rotScale, -max, max);

            Quaternion worldRot = root.rotation * Quaternion.Euler(euler) * Quaternion.Inverse(root.rotation);
            Quaternion localDelta = Quaternion.Inverse(camera.parent.rotation) * worldRot * camera.parent.rotation;
            camera.localRotation = localDelta * camera.localRotation;
        }
    }

    public static void Reset()
    {
        _initialized = false;
        _basePos = Vector3.zero;
        _baseRot = Quaternion.identity;
        _lastAppliedDelta = Vector3.zero;
        _lastResultPos = Vector3.zero;
        _shotShake = Vector3.zero;
        _shotShakeVelocity = Vector3.zero;
        _posKick = Vector3.zero;
        _posKickVelocity = Vector3.zero;
        _lastPosKickFrame = -1;
        _stabilizedHeadEuler = Vector3.zero;
        _directionLag = Vector3.zero;
        _weaponCoupling = Vector3.zero;
        _injuryDrift = Vector3.zero;
        _injuryTime = 0f;
        _lastShakeFrame = -1;
        _blend = 1f;
    }
}
