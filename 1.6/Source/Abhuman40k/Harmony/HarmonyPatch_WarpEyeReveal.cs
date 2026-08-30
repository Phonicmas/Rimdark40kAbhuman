using System.Reflection;
using HarmonyLib;
using Verse;

namespace Abhuman40k;

//ForcedVisible is the single gate every consumer of invisibility reads through: PsychologicallyVisible,
//InvisibilityUtility.IsPsychologicallyInvisible, IsHiddenFromPlayer, GetAlpha, and the fade/effecter/notify
//pipeline in HediffComp_Invisibility.CompPostTick. It is private, so it has to be resolved by reflection.
[HarmonyPatch]
public class WarpEyeRevealPatch
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(HediffComp_Invisibility), "ForcedVisible");
    }

    public static void Postfix(ref bool __result, HediffComp_Invisibility __instance)
    {
        if (__result)
        {
            return;
        }

        __result = WarpEyeRevealUtility.IsRevealed(__instance.Pawn);
    }
}
