using RimWorld.Planet;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using System.Linq;

namespace Abhuman40k;

public class Ability_WarpEyeWarpTravel : VEF.Abilities.Ability
{
    private readonly IntRange travelDurationRange = new IntRange(300, 600000);
    
    public override void DoAction()
    {
        var pawn = PawnsToTeleport().FirstOrDefault(p => p.IsQuestLodger());
        if (pawn != null)
        {
            Dialog_MessageBox.CreateConfirmation("FarskipConfirmTeleportingLodger".Translate(pawn.Named("PAWN")), base.DoAction);
        }
        else
        {
            base.DoAction();
        }
    }

    private IEnumerable<Pawn> PawnsToTeleport()
    {
        var caravan = CasterPawn.GetCaravan();
        if (caravan != null)
        {
            foreach (var caravanPawn in caravan.pawns)
            {
                yield return caravanPawn;
            }
            yield break;
        }
        var homeMap = CasterPawn.Map.IsPlayerHome;
        foreach (var thing in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, GetRadiusForPawn(), useCenter: true))
        {
            if (thing is Pawn mapPawn && !mapPawn.Dead && (mapPawn.IsColonist || mapPawn.IsPrisonerOfColony || (!homeMap && mapPawn.RaceProps.Animal && (mapPawn.Faction?.IsPlayer ?? false))))
            {
                yield return mapPawn;
            }
        }
    }
        
    public override bool CanHitTargetTile(GlobalTargetInfo target)
    {
        var range = Find.WorldGrid.TraversalDistanceBetween((CasterPawn.GetCaravan() != null) ? CasterPawn.GetCaravan().Tile : Caster.Map.Tile, target.Tile);
        return !(range > GetRangeForPawn());
    }

    public override void Cast(params GlobalTargetInfo[] targets)
    {
        base.Cast(targets);
        var caravan = pawn.GetCaravan();
        var travelingPawns = PawnsToTeleport().ToList();

        ThingOwner<Pawn> otherThingOwner = null;
        if (caravan != null)
        {
            otherThingOwner = caravan.pawns;
        }
        
        var arrivalTicks = Find.TickManager.TicksGame + travelDurationRange.RandomInRange;
        Abhuman40kUtils.MakeWarpTravelObject(travelingPawns, targets[0].Tile, arrivalTicks, false, otherThingOwner);
        foreach (var travelingPawn in travelingPawns)
        {
            if (!travelingPawn.IsWorldPawn())
            {
                travelingPawn.ExitMap(false, Rot4.Invalid);
            }
        }
        
        if (caravan != null)
        {
            caravan.RemoveAllPawns();
            caravan.Destroy();
        }
    }

    public override void GizmoUpdateOnMouseover()
    {
        if (WorldRendererUtility.WorldSelected)
        {
            return;
        }
        GenDraw.DrawRadiusRing(pawn.Position, GetRadiusForPawn(), Color.blue);
        var homeMap = CasterPawn.Map.IsPlayerHome;
        foreach (var thing in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, GetRadiusForPawn(), useCenter: true))
        {
            if (thing is Pawn mapPawn && !mapPawn.Dead && (mapPawn.IsColonist || mapPawn.IsPrisonerOfColony || (!homeMap && mapPawn.RaceProps.Animal && (mapPawn.Faction?.IsPlayer ?? false))))
            {
                GenDraw.DrawRadiusRing(mapPawn.Position, 0.9f, Color.green);
            }
        }
    }
}