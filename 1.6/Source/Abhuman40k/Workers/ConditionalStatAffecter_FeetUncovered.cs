using System.Linq;
using RimWorld;
using Verse;

namespace Abhuman40k;

public class ConditionalStatAffecter_FeetUncovered : ConditionalStatAffecter
{
    public override string Label => "BEWH.Abhuman.Conditional.StatsReport_FeetUncovered".Translate();

    public override bool Applies(StatRequest req)
    {
        if (req is { HasThing: true, Thing: Pawn pawn } && pawn.apparel != null)
        {
            return !pawn.apparel.WornApparel.All(apparel => apparel.def.apparel.CoversBodyPartGroup(Abhuman40kDefOf.Feet));
        }
        
        return false;
    }
}