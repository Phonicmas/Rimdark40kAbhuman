using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Abhuman40k;

public class RaidStrategyWorker_Beastman : RaidStrategyWorker
{
    protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
    {
        var siegeSpot = RCellFinder.FindSiegePositionFrom(parms.spawnCenter.IsValid ? parms.spawnCenter : pawns[0].PositionHeld, map);

        return new LordJob_Beastman(parms.faction, siegeSpot, parms.points);
    }
}