using System.Linq;
using Verse;

namespace Abhuman40k;

public class PlaceWorker_OnlyAvailableWithBeastmanInColony : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        if (map.mapPawns.FreeColonistsSpawned.Where(pawn => pawn.genes != null).Any(pawn => pawn.genes.HasActiveGene(Abhuman40kDefOf.BEWH_BeastmanHerdstoneAffinity)))
        {
            return true;
        }

        return "BEWH.Abhuman.PlacementLimit.RequiresBeastman".Translate(((ThingDef)checkingDef).label.CapitalizeFirst());
    }
}