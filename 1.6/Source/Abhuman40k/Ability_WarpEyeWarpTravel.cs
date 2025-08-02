using RimWorld.Planet;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using System.Linq;

namespace Abhuman40k;

public class Ability_WarpEyeWarpTravel : VEF.Abilities.Ability
{
    private IntRange travelDurationRange = new IntRange(60, 60000);

    private int travelDuration = -2;

    private int travelledTime = -1;

    private GlobalTargetInfo[] targets;

    private List<Pawn> pawnsToSpawn;

    private static WarpTravelGameComponent warpTravelGameComponent = Current.Game.GetComponent<WarpTravelGameComponent>();

    public override void DoAction()
    {
        Pawn pawn = PawnsToTeleport().FirstOrDefault((Pawn p) => p.IsQuestLodger());
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
        Caravan caravan = pawn.GetCaravan();
        if (caravan != null)
        {
            foreach (Pawn pawn3 in caravan.pawns)
            {
                yield return pawn3;
            }
            yield break;
        }
        bool homeMap = pawn.Map.IsPlayerHome;
        foreach (Thing thing in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, GetRadiusForPawn(), useCenter: true))
        {
            if (thing is Pawn pawn2 && !pawn2.Dead && (pawn2.IsColonist || pawn2.IsPrisonerOfColony || (!homeMap && pawn2.RaceProps.Animal && (pawn2.Faction?.IsPlayer ?? false))))
            {
                yield return pawn2;
            }
        }
    }
        
    public override bool CanHitTargetTile(GlobalTargetInfo target)
    {
        int range = Find.WorldGrid.TraversalDistanceBetween((CasterPawn.GetCaravan() != null) ? CasterPawn.GetCaravan().Tile : Caster.Map.Tile, target.Tile);
        if (range > GetRangeForPawn())
        {
            return false;
        }
        return true;
    }

    public override void Cast(params GlobalTargetInfo[] targets)
    {
        this.targets = targets;

        Caravan caravan = pawn.GetCaravan();
        List<Pawn> list = PawnsToTeleport().ToList();

        //Teleport from map to somewhere
        if (caravan == null)
        {
            foreach (Pawn item in list)
            {
                if (item.Spawned)
                {
                    item.teleporting = true;
                    item.ExitMap(allowedToJoinOrCreateCaravan: false, Rot4.Invalid);
                }
            }
        }
        else //Teleport from caravan to somewhere
        {
            caravan.Destroy();
        }

        base.Cast(targets);
        warpTravelGameComponent.AddToTravels(CasterPawn, list, targets);
    }

    public override void GizmoUpdateOnMouseover()
    {
        GenDraw.DrawRadiusRing(pawn.Position, GetRadiusForPawn(), Color.blue);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref travelDuration, "travelDuration", -2);
        Scribe_Values.Look(ref travelledTime, "travelledTime", -1);
        Scribe_Collections.Look(ref pawnsToSpawn, "pawnsToSpawn", saveDestroyedThings: true, LookMode.Reference);
        //Scribe_Deep.Look(ref targets, "targets", LookMode.GlobalTargetInfo);
    }
}