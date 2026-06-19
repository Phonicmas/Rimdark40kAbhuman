using System.Collections.Generic;
using System.Linq;
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
    
    public override List<Pawn> SpawnThreats(IncidentParms parms)
    {
        
        var groupKind = parms.pawnGroupKind ?? PawnGroupKindDefOf.Combat;
        var defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(groupKind, parms);
        var pawns = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();
        for (var i = 0; i < 4; i++)
        {
            var request = new PawnGenerationRequest(Abhuman40kDefOf.BEWH_BeastmanFactionBeastmanShaman, parms.faction, PawnGenerationContext.NonPlayer, parms.target.Tile, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: true, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, biocodeWeaponChance: parms.biocodeWeaponsChance, biocodeApparelChance: parms.biocodeApparelChance, allowFood: def.pawnsCanBringFood, forcedXenotype: Abhuman40kDefOf.BEWH_Beastman)
            {
                BiocodeApparelChance = 1f
            };
            var pawn = PawnGenerator.GeneratePawn(request);
            if (pawn != null)
            {
                pawns.Add(pawn);
                var warMask = (Apparel)ThingMaker.MakeThing(ThingDefOf.Apparel_Parka, Abhuman40kDefOf.DevilstrandCloth);
                pawn.apparel.Wear(warMask);
            }
        }

        if (!pawns.Any())
        {
            return null;
        }
        parms.raidArrivalMode.Worker.Arrive(pawns, parms);
        var baseList = base.SpawnThreats(parms);
        if (baseList != null)
        {
            pawns.AddRange(baseList);
        }
        
        return pawns;
    }

}