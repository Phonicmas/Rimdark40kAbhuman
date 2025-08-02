using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Abhuman40k;

public class WarpTravelGameComponent : GameComponent
{

    public List<(Pawn, List<Pawn>, GlobalTargetInfo[], int)> warpTravelVars = new List<(Pawn, List<Pawn>, GlobalTargetInfo[], int)>();

    private readonly IntRange travelDurationRange = new IntRange(60, 6000);

    public WarpTravelGameComponent(Game game)
    {            
    }

    public override void GameComponentTick()
    {
        if (!warpTravelVars.NullOrEmpty())
        {
            for (int i = warpTravelVars.Count-1; i >= 0; i--)
            {
                (Pawn, List<Pawn>, GlobalTargetInfo[], int) travel = warpTravelVars.ElementAt(i);
                if (Current.Game.CurrentMap.IsHashIntervalTick(travel.Item4))
                {
                    ExitWarp(travel.Item1, travel.Item2, travel.Item3);
                    ExitMessage(travel.Item1, travel.Item2, travel.Item4);
                    warpTravelVars.RemoveAt(i);
                }
            }
        }            
    }

    public void AddToTravels(Pawn pawn, List<Pawn> travelers, GlobalTargetInfo[] targets)
    {
        (Pawn, List<Pawn>, GlobalTargetInfo[], int) travel = (pawn, travelers, targets, travelDurationRange.RandomInRange);
        warpTravelVars.Add(travel);
    }

    private void ExitWarp(Pawn pawn, List<Pawn> pawnsToSpawn, GlobalTargetInfo[] targets)
    {
        Caravan caravan = pawn.GetCaravan();
        Map targetMap = ((targets[0].WorldObject is MapParent mapParent) ? mapParent.Map : null);
        IntVec3 targetCell = IntVec3.Invalid;
        if (targetMap != null)
        {
            Pawn pawn2 = AlliedPawnOnMap(targetMap, pawnsToSpawn);
            if (pawn2 != null)
            {
                IntVec3 position = pawn2.Position;
                if (true)
                {
                    targetCell = position;
                }
            }
        }
        //Another allied pawn is on map which is being teleported too
        if (targetCell.IsValid)
        {
            foreach (Pawn item in pawnsToSpawn)
            {
                CellFinder.TryFindRandomSpawnCellForPawnNear(targetCell, targetMap, out var result, 4, (IntVec3 cell) => cell != targetCell && cell.GetRoom(targetMap) == targetCell.GetRoom(targetMap));
                GenSpawn.Spawn(item, result, targetMap);
                item.teleporting = false;
                if (item.drafter != null && item.IsColonistPlayerControlled)
                {
                    item.drafter.Drafted = true;
                }
                item.Notify_Teleported();
                if (item.IsPrisoner)
                {
                    item.guest.WaitInsteadOfEscapingForDefaultTicks();
                }
                if ((item.IsColonist || item.RaceProps.packAnimal) && item.Map.IsPlayerHome)
                {
                    item.inventory.UnloadEverything = true;
                }
            }

            if (Find.WorldSelector.IsSelected(caravan))
            {
                Find.WorldSelector.Deselect(caravan);
            }
            caravan?.Destroy();
        }
        //Teleport to friendly caravan on world map
        else if (targets[0].WorldObject is Caravan caravan2 && caravan2.Faction == pawn.Faction)
        {
            if (caravan != null)
            {
                caravan.pawns.TryTransferAllToContainer(caravan2.pawns);
                caravan2.Notify_Merged(new List<Caravan> { caravan });
                caravan.Destroy();
            }
            else
            {
                foreach (Pawn item2 in pawnsToSpawn)
                {
                    caravan2.AddPawn(item2, addCarriedPawnToWorldPawnsIfAny: true);
                }
            }
        }
        //Teleport from world map to world map
        else if (caravan != null)
        {
            caravan.Tile = targets[0].Tile;
            caravan.pather.StopDead();
        }
        //Teleport to unoccupied world map tile
        else
        {
            CaravanMaker.MakeCaravan(pawnsToSpawn, pawn.Faction, targets[0].Tile, addToWorldPawnsIfNotAlready: false);
        }
    }

    private Pawn AlliedPawnOnMap(Map targetMap, List<Pawn> pawnsToTeleport)
    {
        return targetMap.mapPawns.AllPawnsSpawned.FirstOrDefault((Pawn p) => !p.NonHumanlikeOrWildMan() && p.IsColonist && p.HomeFaction == Faction.OfPlayer && !pawnsToTeleport.Contains(p));
    }
    
    private void ExitMessage(Pawn caster, List<Pawn> travelers,  int travelDuration)
    {
        string teleportedPawns = "";
        for (int i = 0; i < travelers.Count; i++)
        {
            teleportedPawns += travelers[i].NameShortColored;
            if (i == travelers.Count - 2)
            {
                teleportedPawns += " and ";
            }
            else if (i != travelers.Count - 1)
            {
                teleportedPawns += ", ";
            }
        }

        string timeSpent = "";
        if (travelDuration / 2500 == 1)
        {
            timeSpent = "1 hour";
        }
        else
        {
            timeSpent = (travelDuration / 2500) + "hours";
        }

        var letter = new StandardLetter
        {
            lookTargets = caster,
            def = Abhuman40kDefOf.BEWH_WarpTravel,
            Text = "WarpTravelMessage".Translate(caster.Named("PAWN"), timeSpent, teleportedPawns),
            Label = "WarpTravelLetter".Translate()
        };

        Find.LetterStack.ReceiveLetter(letter);
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref warpTravelVars, "warpTravelVars", saveDestroyedThings: true, LookMode.Deep);
        base.ExposeData();
    }
}