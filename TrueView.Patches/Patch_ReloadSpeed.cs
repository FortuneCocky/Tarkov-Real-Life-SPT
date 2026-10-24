using System.Reflection;
using System.Runtime.CompilerServices;
using EFT;
using EFT.HealthSystem;
using HarmonyLib;
using TrueView.Configuration;
using SPT.Reflection.Patching;
using UnityEngine;

namespace TrueView.Patches;

/// <summary>
/// Arm-injury reload scaling. Reload speed is untouched at full arm health,
/// starts slowing below 85% arm HP, and bottoms out at 30% speed when an arm
/// is blacked. Applied to both normal and quick reloads.
/// </summary>
internal static class ReloadArmScale
{
    private static FieldInfo _buffInfoField;
    private static FieldInfo _reloadSpeedField;
    private static FieldInfo _playerField;
    private static bool _resolved;

    // Cache each WeaponBuffsInfo's untouched ReloadSpeed so scaling never
    // compounds across reloads.
    private class Box { public float Value; }
    private static readonly ConditionalWeakTable<object, Box> _baseSpeeds = new();

    private static bool TryResolve()
    {
        if (_resolved) return _reloadSpeedField != null;
        _resolved = true;
        _buffInfoField = AccessTools.Field(typeof(Player.FirearmController), "_buffInfo");
        _playerField = AccessTools.Field(typeof(Player.FirearmController), "_player");
        if (_buffInfoField != null)
            _reloadSpeedField = AccessTools.Field(_buffInfoField.FieldType, "ReloadSpeed");
        return _reloadSpeedField != null;
    }

    public static void Apply(Player.FirearmController fc)
    {
        if (!HandlingConfig.Enabled.Value || !TryResolve())
            return;

        object buffInfo = _buffInfoField.GetValue(fc);
        Player player = _playerField?.GetValue(fc) as Player;
        if (buffInfo == null || player == null || !player.IsYourPlayer)
            return;

        float baseSpeed;
        if (!_baseSpeeds.TryGetValue(buffInfo, out Box box))
        {
            box = new Box { Value = (float)_reloadSpeedField.GetValue(buffInfo) };
            _baseSpeeds.Add(buffInfo, box);
        }
        baseSpeed = box.Value;

        float armHealth = ArmHealth01(player);
        // Above 85% arm HP: full speed. Below: linear to 30% at 0 (blacked).
        float scale = armHealth >= 0.85f ? 1f
            : Mathf.Lerp(0.3f, 1f, armHealth / 0.85f);

        _reloadSpeedField.SetValue(buffInfo, baseSpeed * scale);
    }

    private static float ArmHealth01(Player player)
    {
        try
        {
            if (player.HealthController is ActiveHealthController health)
            {
                float left = HealthFraction(health, EBodyPart.LeftArm);
                float right = HealthFraction(health, EBodyPart.RightArm);
                // The worse arm dictates manipulation speed.
                return Mathf.Min(left, right);
            }
        }
        catch { }
        return 1f;
    }

    private static float HealthFraction(ActiveHealthController health, EBodyPart part)
    {
        var value = health.GetBodyPartHealth(part);
        return value.Maximum > 0f ? Mathf.Clamp01(value.Current / value.Maximum) : 1f;
    }
}

internal class Patch_ReloadSpeed : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        => typeof(Player.FirearmController).GetMethod("ReloadMag", BindingFlags.Instance | BindingFlags.Public);

    [PatchPrefix]
    private static void Prefix(Player.FirearmController __instance) => ReloadArmScale.Apply(__instance);
}

internal class Patch_QuickReloadSpeed : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        => typeof(Player.FirearmController).GetMethod("QuickReloadMag", BindingFlags.Instance | BindingFlags.Public);

    [PatchPrefix]
    private static void Prefix(Player.FirearmController __instance) => ReloadArmScale.Apply(__instance);
}
