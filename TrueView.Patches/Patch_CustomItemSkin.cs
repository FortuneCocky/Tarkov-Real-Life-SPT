using System;
using System.Reflection;
using TrueView.Utilities;
using SPT.Reflection.Patching;

namespace TrueView.Patches;

/// <summary>
/// EFT's weapon-skin system can throw when a skinned renderer was destroyed
/// before a hands transition (e.g. re-equipping after eating). The exception
/// aborts FirearmController creation, leaving the player with no weapon and a
/// permanently broken MovementContext. Swallow it here so controller creation
/// continues; the weapon simply falls back to its default skin.
/// </summary>
internal class Patch_CustomItemSkinSetIsCustom : ModulePatch
{
    private static int _logged;

    protected override MethodBase GetTargetMethod()
    {
        return typeof(CustomItemSkin).GetMethod("SetIsCustom", new[] { typeof(bool) });
    }

    [PatchFinalizer]
    private static Exception Finalizer(Exception __exception)
    {
        return GuardSkinException(__exception, "SetIsCustom", ref _logged);
    }

    internal static Exception GuardSkinException(Exception exception, string source, ref int logged)
    {
        if (exception == null)
            return null;

        if (logged < 3)
        {
            logged++;
            Log.Warning($"[Guard] CustomItemSkin.{source} threw ({exception.GetType().Name}) — suppressed to keep hands controller alive: {exception.Message}");
        }

        return null;
    }
}

internal class Patch_CustomItemSkinProceed : ModulePatch
{
    private static int _logged;

    protected override MethodBase GetTargetMethod()
    {
        return typeof(CustomItemSkin).GetMethod("ProceedItemChanged", new[] { typeof(EFT.InventoryLogic.Item) });
    }

    [PatchFinalizer]
    private static Exception Finalizer(Exception __exception)
    {
        return Patch_CustomItemSkinSetIsCustom.GuardSkinException(__exception, "ProceedItemChanged", ref _logged);
    }
}
