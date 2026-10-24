using System.Reflection;
using EFT;
using EFT.Animations;
using HarmonyLib;
using TrueView.Core;
using SPT.Reflection.Patching;
using UnityEngine;

namespace TrueView.Patches;

internal class Patch_LerpCamera : ModulePatch
{
    private static readonly FieldInfo _headRotationVecField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_headRotationVec");

    protected override MethodBase GetTargetMethod()
    {
        return typeof(ProceduralWeaponAnimation).GetMethod("LerpCamera", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPostfix]
    private static void Postfix(ProceduralWeaponAnimation __instance)
    {
        Player player = HandlingManager.LocalPlayer();
        if (player == null || player.ProceduralWeaponAnimation != __instance)
        {
            return;
        }

        HandlingManager.Tick(player, Time.deltaTime);

        // Skip camera override during free look — let vanilla handle it
        if (player.MouseLookControl)
        {
            return;
        }

        // Firearms only — non-weapon hand states (eating, drinking, planting)
        // must keep their own camera animation.
        if (player.HandsController is not Player.FirearmController)
        {
            return;
        }

        ApplyCamera(__instance);
        ClampCameraPushback(__instance, player);
    }

    // Vanilla TurnAway pushes the camera backward to keep the sight line when
    // the barrel is blocked — far enough that you end up staring at the back
    // of your own helmet/arms. Clamp the camera to stay at least slightly in
    // front of the head's forward plane; the weapon still collapses normally.
    internal static void ClampCameraPushback(ProceduralWeaponAnimation pwa, Player player)
    {
        Transform cam = pwa.HandsContainer?.CameraTransform;
        Transform head = player?.PlayerBones?.Head?.Original;
        if (cam == null || head == null)
            return;

        // forward axis = view direction, closest thing to the face plane.
        Vector3 rel = cam.position - head.position;
        float fwd = Vector3.Dot(rel, cam.forward);
        const float minForward = 0.02f;
        if (fwd < minForward)
            cam.position += cam.forward * (minForward - fwd);
    }

    internal static void ApplyCamera(ProceduralWeaponAnimation __instance)
    {
        if (__instance.HandsContainer?.CameraTransform != null)
        {
            __instance.HandsContainer.CameraTransform.localRotation = Quaternion.Euler(HandlingState.HeadRotation);
            _headRotationVecField?.SetValue(__instance, HandlingState.HeadRotation);
        }
    }
}