using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Abhuman40k;

[StaticConstructorOnStartup]
public class WarpTravelWorldObject : WorldObject, IThingHolder
{
    public ThingOwner<Pawn> pawns;
    public int arrivalTick;
    
    private readonly IntRange randomDurationAdd = new IntRange(0, 60000);
    private readonly IntRange randomDurationTake = new IntRange(-60000, 0);

    private int randomDurationAdded;
    private int randomDurationTaken;
    
    public Pawn Navigator
    {
        get
        {
            return pawns?.InnerListForReading.FirstOrFallback(p => p.genes.HasActiveGene(Abhuman40kDefOf.BEWH_NavigtorWarpNavigation));
        }
    }
    
    public WarpTravelWorldObject()
    {
        pawns = new ThingOwner<Pawn>(this, oneStackOnly: false, LookMode.Reference);
        randomDurationAdded = randomDurationAdd.RandomInRange;
        randomDurationTaken = randomDurationTake.RandomInRange;
    }
    
    public override bool SelectableNow => false;
    public override bool NeverMultiSelect => true;

    public override void Draw() {}
    
    public override IEnumerable<Gizmo> GetGizmos()
    {
        yield return new Command_Action
        {
            defaultLabel = "DEV: Conclude travel",
            action = delegate
            {
                arrivalTick = Find.TickManager.TicksGame;
            }
        };
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        if (stringBuilder.Length != 0)
        {
            stringBuilder.AppendLine();
        }
        var currentTick = Find.TickManager.TicksGame;
        var lowerEstimate = arrivalTick - randomDurationTaken;
        if (lowerEstimate < currentTick)
        {
            lowerEstimate = currentTick;
        }
        var higherEstimate = arrivalTick + randomDurationAdded;
        
        stringBuilder.Append("BEWH.Abhuman.Navigator.TravelTimeRemaining".Translate(TicksToHour(lowerEstimate), TicksToHour(higherEstimate)));
        return stringBuilder.ToString();
    }
    
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref pawns, "pawns");
        Scribe_Values.Look(ref arrivalTick, "arrivalTick");
    }
    
    protected override void TickInterval(int delta)
    {
        base.TickInterval(delta);
        
        if (arrivalTick > Current.Game.tickManager.TicksGame)
        {
            return;
        }

        ExitMessage();
        ArriveAtPlace();
        Destroy();
    }

    private void ArriveAtPlace()
    {
        foreach (var pawnFaction in pawns.InnerListForReading.Where(pawnFaction => !pawnFaction.IsPrisoner && pawnFaction.Faction != Faction.OfPlayer))
        {
            pawnFaction.SetFaction(Faction.OfPlayer);
        }
        var target = Tile;
        var targetMap = Find.WorldObjects.MapParentAt(target)?.Map;
        var targetCaravan = Find.WorldObjects.PlayerControlledCaravanAt(target);
        var targetCell = IntVec3.Invalid;
        if (targetMap != null)
        {
            var alliedPawnOnMap = AlliedPawnOnMap(targetMap, pawns.InnerListForReading);
            if (alliedPawnOnMap != null)
            {
                var position = alliedPawnOnMap.Position;
                if (true)
                {
                    targetCell = position;
                }
            }
        }
        //Another allied pawn is on map which is being teleported too
        if (targetCell.IsValid)
        {
            for (var i = pawns.InnerListForReading.Count-1; i >= 0; i--)
            {
                var pawnToSpawn = pawns.InnerListForReading[i];
                CellFinder.TryFindRandomSpawnCellForPawnNear(targetCell, targetMap, out var result, 4, cell => cell != targetCell && cell.GetRoom(targetMap) == targetCell.GetRoom(targetMap));
                GenSpawn.Spawn(pawnToSpawn, result, targetMap);
                if (pawnToSpawn.drafter != null && pawnToSpawn.IsColonistPlayerControlled)
                {
                    pawnToSpawn.drafter.Drafted = true;
                }
                if (pawnToSpawn.IsPrisoner)
                {
                    pawnToSpawn.guest.WaitInsteadOfEscapingForDefaultTicks();
                }
                if ((pawnToSpawn.IsColonist || pawnToSpawn.RaceProps.packAnimal) && pawnToSpawn.Map.IsPlayerHome)
                {
                    pawnToSpawn.inventory.UnloadEverything = true;
                }
            }
        }
        //Teleport to friendly caravan on world map
        else if (targetCaravan != null)
        {
            targetCaravan.pawns.TryAddRangeOrTransfer(pawns);
        }
        //Teleport to unoccupied world map tile
        else
        {
            var newCaravan = CaravanMaker.MakeCaravan(new List<Pawn>(), Faction.OfPlayer, Tile, false);
            newCaravan.pawns.TryAddRangeOrTransfer(pawns);
        }
    }
    
    private void ExitMessage()
    {
        var travelers = pawns.InnerListForReading;
        var teleportedPawns = "";
        for (var i = 0; i < travelers.Count; i++)
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

        var timeSpent = TicksToHour(arrivalTick);

        var letter = new StandardLetter
        {
            lookTargets = Navigator,
            def = Abhuman40kDefOf.BEWH_WarpTravel,
            Text = "BEWH.Abhuman.Navigator.WarpTravelMessage".Translate(Navigator.Named("PAWN"), timeSpent, teleportedPawns),
            Label = "BEWH.Abhuman.Navigator.WarpTravelLetter".Translate()
        };

        Find.LetterStack.ReceiveLetter(letter);
    }

    private string TicksToHour(int tick)
    {
        string timeSpent;
        if (tick / 2500 == 1)
        {
            timeSpent = "1 hour";
        }
        else
        {
            timeSpent = (tick / 2500) + "hours";
        }

        return timeSpent;
    }
    
    public void AddPawn(Pawn p, bool addCarriedPawnToWorldPawnsIfAny)
    {
        if (p == null)
        {
            Log.Warning("Tried to add a null pawn to " + this);
            return;
        }
        if (p.Dead)
        {
            Log.Warning("Tried to add " + p + " to " + this + ", but this pawn is dead.");
            return;
        }
        var pawn = p.carryTracker.CarriedThing as Pawn;
        if (pawn != null)
        {
            p.carryTracker.innerContainer.Remove(pawn);
        }
        p.DeSpawnOrDeselect();
        if (pawns.TryAddOrTransfer(p))
        {
            if (pawn != null)
            {
                AddPawn(pawn, addCarriedPawnToWorldPawnsIfAny);
                if (addCarriedPawnToWorldPawnsIfAny)
                {
                    Find.WorldPawns.PassToWorld(pawn);
                }
            }
        }
        else
        {
            Log.Error("Couldn't add pawn " + p + " to caravan.");
        }
    }

    public bool ContainsPawn(Pawn p)
    {
        return pawns.Contains(p);
    }
    
    private Pawn AlliedPawnOnMap(Map targetMap, List<Pawn> pawnsToTeleport)
    {
        return targetMap.mapPawns.AllPawnsSpawned.FirstOrDefault(p => !p.NonHumanlikeOrWildMan() && p.IsColonist && p.HomeFaction == Faction.OfPlayer && !pawnsToTeleport.Contains(p));
    }

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public ThingOwner GetDirectlyHeldThings()
    {
        return pawns;
    }
}