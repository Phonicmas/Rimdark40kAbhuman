using System.Linq;
using RimWorld;
using Verse;

namespace Abhuman40k;

public class ThoughtWorker_HerdstoneRitual : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        var thing = p?.Map?.listerThings?.ThingsOfDef(Abhuman40kDefOf.BEWH_HerdstonePlayer)?.FirstOrDefault();
        
        if (thing is not Building_Herdstone)
        {
            return false;
        }

        if (p.genes == null)
        {
            return false;
        }

        if (p.genes.HasActiveGene(Abhuman40kDefOf.BEWH_BeastmanHerdstoneAffinity))
        {
            return ThoughtState.ActiveAtStage(1);
        }

        if (ModLister.AnomalyInstalled && p.story.traits.HasTrait(TraitDefOf.VoidFascination))
        {
            return false;
        }

        return ThoughtState.ActiveAtStage(0);
    }
}