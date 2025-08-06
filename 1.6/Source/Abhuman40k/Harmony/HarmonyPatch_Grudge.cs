using HarmonyLib;
using RimWorld;

namespace Abhuman40k;

[HarmonyPatch(typeof(Thought_MemorySocial), "ShouldDiscard", MethodType.Getter)]
public class GrudgePatch
{
    public static void Postfix(ref bool __result, ref Thought_MemorySocial __instance)
    {
        if (__result == false)
        {
            return;
        }
        if (__instance.opinionOffset >= 0)
        {
            return;
        }
        if (__instance.pawn.genes == null)
        {
            return;
        }
        if (__instance.pawn.genes.HasActiveGene(Abhuman40kDefOf.BEWH_KinGrudgy))
        {
            if (__instance.age <= (__instance.DurationTicks * 2))
            {
                __result = false;
            }
        }
    }
}