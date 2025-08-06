using HarmonyLib;
using Verse;
using Verse.AI;

namespace Abhuman40k;

[HarmonyPatch(typeof(MentalBreakWorker), "TryStart")]
public class ToddlerLogicMentalBreakWorkerPatch
{
    public static void Prefix(MentalBreakWorker __instance, Pawn pawn)
    {
        if (pawn.genes == null || !pawn.genes.HasActiveGene(Abhuman40kDefOf.BEWH_OgrynToddlerLogic))
        {
            return;
        }

        __instance.def = Abhuman40kDefOf.Tantrum;
    }
}