using EFT;
using EFT.HealthSystem;
using TrueView.Configuration;
using UnityEngine;

namespace TrueView.Systems;

internal static class InjuryHandlingSystem
{
    private static float _armPenalty;
    private static float _legPenalty;
    private static float _torsoPenalty;
    private static float _headPenalty;
    private static float _armBias;
    private static float _legBias;

    public static float ArmPenalty => _armPenalty;
    public static float LegPenalty => _legPenalty;
    public static float TorsoPenalty => _torsoPenalty;
    public static float HeadPenalty => _headPenalty;
    public static float ArmBias => _armBias;
    public static float LegBias => _legBias;
    public static float WeaponFollowMultiplier => Mathf.Clamp(1f - _armPenalty * 0.38f - _torsoPenalty * 0.12f, 0.35f, 1f);
    public static float RecoilMultiplier => 1f + _armPenalty * 0.75f + _torsoPenalty * 0.2f;
    public static float SwayMultiplier => 1f + _armPenalty * 0.8f + _torsoPenalty * 0.35f + _headPenalty * 0.45f;
    public static float GaitInstability => 1f + _legPenalty * 0.75f + _torsoPenalty * 0.15f;
    public static float MomentumMultiplier => 1f + _legPenalty * 0.5f + _torsoPenalty * 0.2f;

    public static void Tick(Player player, float deltaTime)
    {
        if (HandlingConfig.InjuryHandlingEnabled?.Value != true || player?.HealthController is not ActiveHealthController health)
        {
            Reset();
            return;
        }

        float leftArm = 1f - HealthFraction(health, EBodyPart.LeftArm);
        float rightArm = 1f - HealthFraction(health, EBodyPart.RightArm);
        float leftLeg = 1f - HealthFraction(health, EBodyPart.LeftLeg);
        float rightLeg = 1f - HealthFraction(health, EBodyPart.RightLeg);
        float chest = 1f - HealthFraction(health, EBodyPart.Chest);
        float stomach = 1f - HealthFraction(health, EBodyPart.Stomach);
        float head = 1f - HealthFraction(health, EBodyPart.Head);

        float arms = Mathf.Max(leftArm, rightArm) * 0.65f + (leftArm + rightArm) * 0.175f;
        float legs = Mathf.Max(leftLeg, rightLeg) * 0.65f + (leftLeg + rightLeg) * 0.175f;
        float torso = chest * 0.65f + stomach * 0.35f;
        float masking = 1f;
        if (HandlingConfig.InjuryPainkillerMasking.Value)
        {
            try
            {
                if (player.MovementContext.PhysicalConditionIs(EPhysicalCondition.OnPainkillers))
                    masking = 0.7f;
            }
            catch { }
        }

        float strength = HandlingConfig.InjuryHandlingStrength.Value;
        float t = 1f - Mathf.Exp(-5f * Mathf.Min(deltaTime, 1f / 30f));
        _armPenalty = Mathf.Lerp(_armPenalty, arms * strength * masking, t);
        _legPenalty = Mathf.Lerp(_legPenalty, legs * strength * Mathf.Lerp(1f, masking, 0.6f), t);
        _torsoPenalty = Mathf.Lerp(_torsoPenalty, torso * strength * masking, t);
        _headPenalty = Mathf.Lerp(_headPenalty, head * strength * masking, t);
        _armBias = Mathf.Lerp(_armBias, (leftArm - rightArm) * strength * masking, t);
        _legBias = Mathf.Lerp(_legBias, (leftLeg - rightLeg) * strength * masking, t);
    }

    public static void Reset()
    {
        _armPenalty = 0f;
        _legPenalty = 0f;
        _torsoPenalty = 0f;
        _headPenalty = 0f;
        _armBias = 0f;
        _legBias = 0f;
    }

    private static float HealthFraction(ActiveHealthController health, EBodyPart part)
    {
        try
        {
            var value = health.GetBodyPartHealth(part);
            return value.Maximum > 0f ? Mathf.Clamp01(value.Current / value.Maximum) : 1f;
        }
        catch
        {
            return 1f;
        }
    }
}
