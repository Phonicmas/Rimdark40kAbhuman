using HarmonyLib;
using Verse;
using Verse.AI;

namespace Abhuman40k;

[HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState")]
public class ToddlerLogicMentalStateHandlerPatch
{
    public static void Prefix(Pawn ___pawn, ref MentalStateDef stateDef)
    {
        if (___pawn.genes == null || !___pawn.genes.HasActiveGene(Abhuman40kDefOf.BEWH_OgrynToddlerLogic))
        {
            return;
        }

        stateDef = Abhuman40kSecondDefOf.Tantrum;
    }
}