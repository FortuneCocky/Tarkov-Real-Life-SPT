using EFT;
using IRLWeaponHandling.Configuration;
using IRLWeaponHandling.Core;
using UnityEngine;

namespace IRLWeaponHandling.Systems;

internal static class MomentumSystem
{
	private const float MaxDeltaTime = 1f / 15f;

	private const float ReferenceTurnRate = 180f;

	private const float MaxHeadPitch = 4f;

	private const float MaxHeadYaw = 2.5f;

	private const float MaxHeadRoll = 7f;

	private static Vector3 _lastLocalVelocity;

	private static Vector3 _smoothedLocalVelocity;

	private static Vector3 _smoothedLocalAcceleration;

	private static float _lastYawRate;

	private static float _smoothedYawRate;

	private static float _smoothedYawAcceleration;

	private static bool _hasSample;

	private static Vector3 _headRotation;

	private static Vector3 _headVelocity;

	private static float _aimBlend;

	public static Vector3 Modify(Vector3 headRotation)
	{
		if (_headRotation == Vector3.zero)
		{
			return headRotation;
		}
		headRotation.x += _headRotation.x;
		headRotation.y += _headRotation.y;
		headRotation.z += _headRotation.z;
		return headRotation;
	}

	public static float WeaponRoll()
	{
		return _headRotation.z;
	}

	public static void Tick(Player player, float deltaTime)
	{
		float rateDt = Mathf.Max(deltaTime, 1f / 300f);
		float simDt = Mathf.Clamp(deltaTime, 0f, 1f / 30f);
		if (simDt <= 0f || !HandlingConfig.MomentumEnabled.Value || player == null || !player.IsYourPlayer)
		{
			Reset();
			return;
		}

		_aimBlend = Mathf.Lerp(_aimBlend, HandlingState.IsAiming ? 1f : 0f, 1f - Mathf.Exp(-5f * simDt));
		float hipMul = 1f - _aimBlend * 0.72f;
		float amount = Mathf.Clamp01(HandlingConfig.MomentumAmount.Value);
		float weightInfluence = Mathf.Clamp01(HandlingConfig.MomentumWeight.Value);
		PhysicalBase physical = player.Physical;
		float overweight = physical == null ? 0f : Mathf.Clamp01(physical.Overweight);
		float inertia = physical == null ? 0f : Mathf.Clamp01(physical.Inertia);
		float bodyWeight = Mathf.Clamp01(overweight * 0.65f + inertia * 0.55f);
		float weightScale = 1f + bodyWeight * weightInfluence * 1.35f;

		float yaw = player.Rotation.x * Mathf.Deg2Rad;
		float sin = Mathf.Sin(yaw);
		float cos = Mathf.Cos(yaw);
		Vector3 worldVelocity = player.Velocity;
		Vector3 localVelocity = new(
			worldVelocity.x * cos - worldVelocity.z * sin,
			worldVelocity.y,
			worldVelocity.x * sin + worldVelocity.z * cos);

		if (!_hasSample)
		{
			_lastLocalVelocity = localVelocity;
			_smoothedLocalVelocity = localVelocity;
			_lastYawRate = MotionTracker.YawRate;
			_hasSample = true;
		}

		Vector3 rawAcceleration = Vector3.ClampMagnitude((localVelocity - _lastLocalVelocity) / rateDt, 35f);
		_lastLocalVelocity = localVelocity;
		float velocityResponse = Mathf.Lerp(12f, 6f, bodyWeight);
		float accelerationResponse = Mathf.Lerp(10f, 5f, bodyWeight);
		_smoothedLocalVelocity = Vector3.Lerp(_smoothedLocalVelocity, localVelocity, 1f - Mathf.Exp(-velocityResponse * simDt));
		_smoothedLocalAcceleration = Vector3.Lerp(_smoothedLocalAcceleration, rawAcceleration, 1f - Mathf.Exp(-accelerationResponse * simDt));

		float rawYawRate = Mathf.Clamp(MotionTracker.YawRate, -720f, 720f);
		float rawYawAcceleration = Mathf.Clamp((rawYawRate - _lastYawRate) / rateDt, -1800f, 1800f);
		_lastYawRate = rawYawRate;
		_smoothedYawRate = Mathf.Lerp(_smoothedYawRate, rawYawRate, 1f - Mathf.Exp(-Mathf.Lerp(11f, 5f, bodyWeight) * simDt));
		_smoothedYawAcceleration = Mathf.Lerp(_smoothedYawAcceleration, rawYawAcceleration, 1f - Mathf.Exp(-Mathf.Lerp(9f, 4f, bodyWeight) * simDt));

		float forwardSpeed = _smoothedLocalVelocity.z;
		float sideSpeed = _smoothedLocalVelocity.x;
		float forwardAccel = _smoothedLocalAcceleration.z;
		float sideAccel = _smoothedLocalAcceleration.x;
		float speed01 = Mathf.Clamp01(new Vector2(sideSpeed, forwardSpeed).magnitude / 5f);
		float turn01 = Mathf.Clamp(_smoothedYawRate / ReferenceTurnRate, -2f, 2f);
		float turnAccel01 = Mathf.Clamp(_smoothedYawAcceleration / 900f, -1f, 1f);
		float turnStrength = HandlingConfig.MomentumTurn.Value;

		float pitch = -forwardAccel * 0.11f + forwardSpeed * HandlingConfig.MomentumWalkForwardLean.Value * 0.32f;
		float yawLean = sideAccel * 0.055f + sideSpeed * 0.08f;
		float strafeRoll = sideAccel * 0.16f + sideSpeed * HandlingConfig.MomentumWalkSideLean.Value * 0.7f;
		float turnRoll = -(turn01 * (2.2f + speed01 * 2.3f) + turnAccel01 * 1.4f) * turnStrength;
		Vector3 target = new(pitch, yawLean, strafeRoll + turnRoll);
		target *= amount * weightScale * hipMul * LocomotionGaitSystem.MomentumMultiplier * InjuryHandlingSystem.MomentumMultiplier;
		target.x = Mathf.Clamp(target.x, -MaxHeadPitch, MaxHeadPitch);
		target.y = Mathf.Clamp(target.y, -MaxHeadYaw, MaxHeadYaw);
		target.z = Mathf.Clamp(target.z, -MaxHeadRoll, MaxHeadRoll);

		float stopWeight = Mathf.Clamp01(HandlingConfig.MomentumStopDelay.Value / 2f);
		float frequency = Mathf.Lerp(5.5f, 2.2f, Mathf.Clamp01(bodyWeight * 0.7f + stopWeight * 0.3f));
		Spring(ref _headRotation, ref _headVelocity, target, frequency, simDt);
	}

	public static void Reset()
	{
		_lastLocalVelocity = Vector3.zero;
		_smoothedLocalVelocity = Vector3.zero;
		_smoothedLocalAcceleration = Vector3.zero;
		_lastYawRate = 0f;
		_smoothedYawRate = 0f;
		_smoothedYawAcceleration = 0f;
		_hasSample = false;
		_headRotation = Vector3.zero;
		_headVelocity = Vector3.zero;
		_aimBlend = 0f;
	}

	private static void Spring(ref Vector3 current, ref Vector3 velocity, Vector3 target, float frequency, float deltaTime)
	{
		float num = frequency * frequency;
		float num2 = 1.4f * frequency;
		velocity += ((target - current) * num - velocity * num2) * deltaTime;
		current += velocity * deltaTime;
	}
}