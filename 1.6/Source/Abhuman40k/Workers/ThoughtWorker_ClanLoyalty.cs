using RimWorld;
using Verse;

namespace Abhuman40k;

public class ThoughtWorker_ClanLoyalty : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
    {
        if (pawn.genes == null || other.genes == null)
        {
            return false;
        }
            
        var gene = pawn.genes.GetGene(Abhuman40kDefOf.BEWH_KinClanLoyalty);
        var gene2 = other.genes.GetGene(Abhuman40kDefOf.BEWH_KinClanLoyalty);
        if (gene == null || gene2 == null)
        {
            return false;
        }
        
        var defMod = gene.def.GetModExtension<DefModExtension_ClanLoyalty>();

        foreach (var pawnRelation in pawn.GetRelations(other))
        {
            if (defMod.doubledByRelations.Contains(pawnRelation))
            {
                return ThoughtState.ActiveAtStage(1);
            }
        }
        
        return ThoughtState.ActiveAtStage(0);
    }
}