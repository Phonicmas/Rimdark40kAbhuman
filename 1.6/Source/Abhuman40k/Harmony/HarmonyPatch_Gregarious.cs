using HarmonyLib;
using RimWorld;
using Verse;

namespace Abhuman40k;

[HarmonyPatch(typeof(Thought_Memory), "MoodOffset")]
public class GregariousPatch
{
    public static void Postfix(ref float __result, ref Thought_MemorySocial __instance)
    {
        if (__instance.pawn.genes == null)
        {
            return;
        }
        if (__instance.pawn.genes.HasActiveGene(Abhuman40kDefOf.BEWH_RatlingGregarious))
        {
            if (__instance.pawn.genes.GetGene(Abhuman40kDefOf.BEWH_RatlingGregarious).def.GetModExtension<DefModExtension_Gregarious>().thoughtEffectsDoubled.Contains(__instance.def))
            {
                __result *= 2;
            }
        }
    }
}