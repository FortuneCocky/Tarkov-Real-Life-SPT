using EFT.Animations;
using TrueView.Configuration;
using TrueView.Core;
using UnityEngine;

namespace TrueView.Systems;

internal static class SwaySystem
{
	private static float _appliedTremor = -1f;
	private static float _breathStabilize = 1f;

	public static void Apply()
	{
		if (HandlingConfig.SwayEnabled.Value && HandlingState.HasWeapon)
		{
			ProceduralWeaponAnimation animation = HandlingState.Animation;
			bool isAiming = HandlingState.IsAiming;
			// Holding breath while aiming stabilizes the arms almost fully.
			float stabTarget = isAiming && HandlingState.IsHoldingBreath ? 0.1f : 1f;
			_breathStabilize = Mathf.Lerp(_breathStabilize, stabTarget, 1f - Mathf.Exp(-6f * Time.deltaTime));
			float num = Fatigue();
			float weightProgress = HandlingState.Stats.WeightProgress;
			float num2 = (isAiming ? HandlingConfig.AimSwayMultiplier.Value : HandlingConfig.HipSwayMultiplier.Value);
			float num3 = num2 * num * _breathStabilize;
			if (animation.Breath != null)
			{
				animation.Breath.Intensity = animation.IntensityByPoseLevel * animation.IntensityByAiming * num3;
				animation.Breath.HipPenalty = HandlingConfig.HipSwayMultiplier.Value * (1f + 0.5f * weightProgress);
			}
			if (animation.MotionReact != null)
			{
				animation.MotionReact.Intensity = 0.45f * HandlingConfig.MotionInertiaMultiplier.Value * (1f + 0.35f * weightProgress) * _breathStabilize;
			}
			ApplyTremor(animation, num * _breathStabilize);
		}
	}

	private static void ApplyTremor(ProceduralWeaponAnimation animation, float fatigue)
	{
		if (animation.HandShakeEffector != null)
		{
			float num = 3f * HandlingConfig.TremorMultiplier.Value * fatigue + AdrenalineSystem.TremorAdd;
			if (!(Mathf.Abs(num - _appliedTremor) < 0.01f))
			{
				_appliedTremor = num;
				animation.HandShakeEffector.Setup(num, 1f, 1f);
			}
		}
	}

	public static float Fatigue()
	{
		float num = Mathf.Max(1f - HandlingState.HandsStamina01, 1f - HandlingState.Oxygen01);
		float num2 = 0.5f * HandlingState.Stats.WeightProgress * (1f - HandlingState.Stamina01);
		return (1f + HandlingConfig.StaminaSwayInfluence.Value * (num + num2)) * InjuryHandlingSystem.SwayMultiplier;
	}

	public static void Reset()
	{
		_appliedTremor = -1f;
	}
}
