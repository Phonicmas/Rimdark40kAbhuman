using RimWorld;
using Verse;

namespace Abhuman40k;

public class ThoughtWorker_UnsettlingPresence : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
    {
        if (other.genes == null)
        {
            return false;
        }
        
        if (other.genes.HasActiveGene(Abhuman40kDefOf.BEWH_NavigtorHouseCastana))
        {
            return false;
        }

        return other.genes.HasActiveGene(Abhuman40kDefOf.BEWH_NavigtorUnsettlingPresence);
    }
}