using RimWorld;
using Verse;

namespace Abhuman40k;

public class ThoughtWorker_Gregarious : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (p.genes == null)
        {
            return false;
        }
            
        var gene = p.genes.GetGene(Abhuman40kDefOf.BEWH_RatlingGregarious);
        if (gene == null)
        {
            return false;
        }

        if (p.Map?.mapPawns == null)
        {
            return false;
        }
        
        var colonyPawns = p.Map.mapPawns.ColonistCount;

        return colonyPawns switch
        {
            < 2 => ThoughtState.ActiveAtStage(0),
            < 3 => ThoughtState.ActiveAtStage(1),
            < 4 => ThoughtState.ActiveAtStage(2),
            < 6 => ThoughtState.ActiveAtStage(3),
            > 8 => ThoughtState.ActiveAtStage(4),
            _ => ThoughtState.ActiveAtStage(0)
        };
    }
}