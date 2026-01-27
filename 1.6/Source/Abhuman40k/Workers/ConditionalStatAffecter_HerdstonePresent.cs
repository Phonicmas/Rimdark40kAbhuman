using RimWorld;
using Verse;

namespace Abhuman40k;

public class ConditionalStatAffecter_HerdstonePresent : ConditionalStatAffecter
{
    public override string Label => "BEWH.Abhuman.Beastman.HerdstonePresent".Translate();

    public override bool Applies(StatRequest req)
    {
        if (req is not { HasThing: true, Thing: Pawn pawn })
        {
            return false;
        }
        
        if (pawn.Map?.listerBuildings == null)
        {
            return false;
        }
            
        return pawn.Map.listerBuildings.ColonistsHaveBuilding(Abhuman40kDefOf.BEWH_HerdstonePlayer);

    }
}