using EFT;
using EFT.Animations;
using TrueView.Configuration;
using UnityEngine;

namespace TrueView.Core;

internal static class HandlingState
{
	public static Player Player { get; private set; }

	public static Player.FirearmController Firearm { get; private set; }

	public static ProceduralWeaponAnimation Animation { get; private set; }

	public static WeaponStats Stats { get; private set; }

	public static float HandsStamina01 { get; private set; } = 1f;


	public static float Stamina01 { get; private set; } = 1f;


	public static float Oxygen01 { get; private set; } = 1f;


	public static float MovementSpeed01 { get; private set; }

	public static bool IsSprinting { get; private set; }

	public static bool IsHoldingBreath { get; private set; }

	/// <summary>Recoil Control skill, 0 = level 0, 1 = elite (51).</summary>
	public static float RecoilSkill01 { get; private set; }

	/// <summary>Aim Drills skill, 0 = level 0, 1 = elite (51).</summary>
	public static float AimSkill01 { get; private set; }

	public static bool HasWeapon
	{
		get
		{
			if (Firearm != null)
			{
				return Animation != null;
			}
			return false;
		}
	}

	public static Vector3 HeadRotation { get; set; }

	public static bool IsAiming
	{
		get
		{
			if (Firearm != null)
			{
				return Firearm.IsAiming;
			}
			return false;
		}
	}

	public static bool Enabled
	{
		get
		{
			if (HandlingConfig.Enabled != null)
			{
				return HandlingConfig.Enabled.Value;
			}
			return false;
		}
	}

	public static void Update(Player player)
	{
		Player = player;
		Animation = player.ProceduralWeaponAnimation;
		Firearm = player.HandsController as Player.FirearmController;
		if (Firearm == null || Firearm.Weapon == null || !player.HasFirearmInHands())
		{
			Firearm = null;
			Stats = default(WeaponStats);
		}
		else if (!Stats.Matches(Firearm))
		{
			RefreshStats();
		}
		PhysicalBase physical = player.Physical;
		if (physical != null)
		{
			HandsStamina01 = physical.HandsStamina?.NormalValue ?? 1f;
			Stamina01 = physical.Stamina?.NormalValue ?? 1f;
			Oxygen01 = physical.Oxygen?.NormalValue ?? 1f;
		}
		try
		{
			SkillManager skills = player.Skills;
			RecoilSkill01 = skills?.RecoilControl != null ? Mathf.Clamp01(skills.RecoilControl.Level / 51f) : 0f;
			AimSkill01 = skills?.AimDrills != null ? Mathf.Clamp01(skills.AimDrills.Level / 51f) : 0f;
		}
		catch
		{
			RecoilSkill01 = 0f;
			AimSkill01 = 0f;
		}
		MovementContext movementContext = player.MovementContext;
		if (movementContext != null)
		{
			float num = Mathf.Max(movementContext.MaxSpeed, 0.1f);
			MovementSpeed01 = Mathf.Clamp01(movementContext.CharacterMovementSpeed / num);
			IsSprinting = movementContext.IsSprintEnabled;
			IsHoldingBreath = movementContext.IsHoldingBreath();
		}
	}

	public static void RefreshStats()
	{
		if (Firearm != null && Firearm.Weapon != null)
		{
			Stats = WeaponStats.Read(Firearm);
		}
	}

	public static void Clear()
	{
		Player = null;
		Firearm = null;
		Animation = null;
		Stats = default(WeaponStats);
		HandsStamina01 = 1f;
		Stamina01 = 1f;
		Oxygen01 = 1f;
		MovementSpeed01 = 0f;
		IsSprinting = false;
		IsHoldingBreath = false;
		RecoilSkill01 = 0f;
		AimSkill01 = 0f;
		HeadRotation = Vector3.zero;
	}
}
