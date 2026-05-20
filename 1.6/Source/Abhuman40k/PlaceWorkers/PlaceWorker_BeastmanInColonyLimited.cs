using System;
using System.Linq;
using Core40k;
using Verse;

namespace Abhuman40k;

public class PlaceWorker_BeastmanInColonyLimited : PlaceWorker
{
    private static readonly SimpleCurve AllowedConduitAmountCurve =
    [
        new CurvePoint(0f, 0f),
        new CurvePoint(2f, 1f),
        new CurvePoint(4f, 2f),
        new CurvePoint(5f, 3f),
        new CurvePoint(10f, 6f),
        new CurvePoint(20f, 9f),
    ];
    
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        var conduitCount = map.listerBuildings.CountBuildingColonistOfDef(Abhuman40kDefOf.BEWH_HerdstoneConduitPlayer);
        var herdstoneAffinityPawnsCount = map.mapPawns.FreeColonistsSpawned.Where(pawn => pawn.genes != null).Count(pawn => pawn.genes.HasActiveGene(Abhuman40kDefOf.BEWH_BeastmanHerdstoneAffinity));

        var allowedConduitsCount = Math.Floor(AllowedConduitAmountCurve.Evaluate(herdstoneAffinityPawnsCount));

        if (conduitCount < allowedConduitsCount)
        {
            return true;
        }

        return "BEWH.Abhuman.PlacementLimit.HerdstoneConduitLimit".Translate(allowedConduitsCount, conduitCount);
    }
}