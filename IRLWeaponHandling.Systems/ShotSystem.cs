using EFT.Animations;
using IRLWeaponHandling.Configuration;
using IRLWeaponHandling.Core;
using IRLWeaponHandling.Utilities;
using UnityEngine;

namespace IRLWeaponHandling.Systems;

internal static class ShotSystem
{
	// How long after a shot the deadzone systems disengage. Vanilla recoil
	// rotates player.HeadRotation; if the deadzones kept holding the camera
	// fixed, the weapon tracker would read the kick as divergence and the
	// gun would visibly fly off the hands. During this window the camera
	// tracks the raw rotation so recoil looks and feels vanilla.
	private const float ShotRecoveryWindow = 0.45f;

	private static string _baseGainWeaponId;

	private static float _baseGainPerShot;

	private static float _lastShotTime = -10f;

	public static bool InShotRecovery => Time.unscaledTime - _lastShotTime < ShotRecoveryWindow;

	public static void OnShot()
	{
		_lastShotTime = Time.unscaledTime;
		if (!HandlingState.HasWeapon)
			return;

		WeaponStats stats = HandlingState.Stats;
		float fatigue = 1f - HandlingState.HandsStamina01;

		// Shooter skill: Recoil Control is the main driver, Aim Drills adds a
		// little extra steadiness once shouldered. A level-0 PMC still fights
		// the gun; an elite one tames it but never removes recoil entirely.
		float skill = HandlingState.RecoilSkill01;
		float aimSkill = HandlingState.AimSkill01;
		bool aiming = HandlingState.IsAiming;
		float skillScale = Mathf.Lerp(1f, 0.7f, skill);
		// Shouldering the weapon braces it: ADS kicks much less than hip,
		// and trained shooters gain more from bracing than untrained ones.
		float adsScale = aiming ? Mathf.Lerp(0.5f, 0.3f, skill * 0.75f + aimSkill * 0.25f) : 1f;
		// Breath-hold while shouldered: braced, steady shot — hardcoded 70%
		// recoil cut on both the camera kick and the weapon misalignment.
		// Skilled shooters already brace hard via adsScale/skillScale; this
		// keeps enough kick that high levels aren't a complete laser.
		float breathHoldScale = (aiming && HandlingState.IsHoldingBreath) ? 0.3f : 1f;

		float headKick = breathHoldScale * Mathf.Lerp(0.35f, 0.8f, stats.WeightProgress) *
			Mathf.Lerp(0.85f, 1.25f, fatigue) * (aiming ? 0.7f : 1f) * skillScale *
			(1f + InjuryHandlingSystem.HeadPenalty * 0.35f + InjuryHandlingSystem.TorsoPenalty * 0.2f);
		HeadAttachSystem.AddShotImpulse(HandlingMath.SignedRandom(headKick * 0.35f), headKick);

		if (HandlingConfig.ShotMisalignmentEnabled.Value)
		{
			float num = breathHoldScale * HandlingConfig.ShotMisalignmentStrength.Value * (1f + stats.WeightProgress) * (1.5f - 0.5f * stats.Ergonomics01) * adsScale * skillScale * (1f + fatigue * HandlingConfig.StaminaShotInfluence.Value) * InjuryHandlingSystem.RecoilMultiplier;
			float value = HandlingConfig.ShotMisalignmentMax.Value * (aiming ? 0.6f : 1f);
			// Skilled shooters also keep the muzzle straighter side-to-side.
			float yaw = Mathf.Clamp(HandlingMath.SignedRandom(num * 0.6f * Mathf.Lerp(1f, 0.6f, skill)), 0f - value, value);
			// Negative X is muzzle-up in Unity's euler convention.
			float pitch = -Mathf.Clamp(num * Random.Range(0.4f, 1f), 0f, value);
			ProceduralHipSystem.AddImpulse(yaw, pitch);
		}
	}

	public static void ApplyWeaponVariables(ProceduralWeaponAnimation animation)
	{
		BreathEffector breath = animation.Breath;
		if (breath == null)
		{
			return;
		}
		string weaponId = HandlingState.Stats.WeaponId;
		if (weaponId != null)
		{
			if (_baseGainWeaponId != weaponId)
			{
				_baseGainWeaponId = weaponId;
				_baseGainPerShot = breath.AmplitudeGainPerShot;
			}
			breath.AmplitudeGainPerShot = (HandlingConfig.ShotMisalignmentEnabled.Value ? (_baseGainPerShot * (1f + HandlingConfig.ShotBreathPenalty.Value) * (1f + (1f - HandlingState.HandsStamina01) * HandlingConfig.StaminaShotInfluence.Value)) : _baseGainPerShot);
		}
	}

	public static void Reset()
	{
		_baseGainWeaponId = null;
		_baseGainPerShot = 0f;
		_lastShotTime = -10f;
	}
}
