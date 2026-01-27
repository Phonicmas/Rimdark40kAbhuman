using RimWorld;
using Verse;

namespace Abhuman40k;

public class ThoughtWorker_FelinidCutePresence : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
    {
        return other.genes != null && other.genes.HasActiveGene(Abhuman40kDefOf.BEWH_FelinidCatlikeMindset);
    }
}