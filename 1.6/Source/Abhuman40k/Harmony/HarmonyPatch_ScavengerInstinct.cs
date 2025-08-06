using HarmonyLib;
using RimWorld;
using Verse;

namespace Abhuman40k;

[HarmonyPatch(typeof(ThoughtWorker_DeadMansApparel), "ApparelCounts")]
public class ScavengerInstinctPatch
{
    public static void Postfix(ref bool __result, ref Thought_MemorySocial __instance)
    {
        if (!__result)
        {
            return;
        }
        if (__instance.pawn.genes == null)
        {
            return;
        }
        if (__instance.pawn.genes.HasActiveGene(Abhuman40kDefOf.BEWH_RatlingScavengerInstinct))
        {
            __result = false;
        }
    }
}