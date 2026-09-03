using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Abhuman40k;

public static class Abhuman40kUtils
{
    private static readonly List<Pawn> tmpPawns = new List<Pawn>();
    
    /// <summary>
    /// <paramref name="durationTicks"/> is how long the passage lasts, not when it ends. Passing an
    /// absolute tick here is the mistake that used to make late-game translations finish instantly.
    /// </summary>
    public static WarpTravelWorldObject MakeWarpTravelObject(IEnumerable<Pawn> pawns, PlanetTile startingTile, int durationTicks, bool addToWorldPawnsIfNotAlready)
    {
        if (!startingTile.Valid && addToWorldPawnsIfNotAlready)
        {
            Log.Warning("Tried to create a WarpTravelWorldObject but chose not to spawn a WarpTravelWorldObject but pass pawns to world. This can cause bugs because pawns can be discarded.");
        }
        tmpPawns.Clear();
        tmpPawns.AddRange(pawns);
        var warpTravelObject = (WarpTravelWorldObject)WorldObjectMaker.MakeWorldObject(Abhuman40kDefOf.BEWH_NavigatorWarpTravel);
        if (startingTile.Valid)
        {
            warpTravelObject.Tile = startingTile;
        }
        warpTravelObject.SetFaction(Faction.OfPlayer);
        warpTravelObject.travelDurationTicks = durationTicks;
        warpTravelObject.arrivalTick = Find.TickManager.TicksGame + durationTicks;
        if (startingTile.Valid)
        {
            Find.WorldObjects.Add(warpTravelObject);
        }
        foreach (var pawn in tmpPawns)
        {
            if (pawn.Dead)
            {
                Log.Warning("Tried to form a warp travel with a dead pawn " + pawn);
                continue;
            }
            if (!warpTravelObject.ContainsPawn(pawn))
            {
                warpTravelObject.AddPawn(pawn, addToWorldPawnsIfNotAlready);
                
            }
            if (addToWorldPawnsIfNotAlready && !pawn.IsWorldPawn())
            {
                Find.WorldPawns.PassToWorld(pawn);
            }
        }

        tmpPawns.Clear();
        return warpTravelObject;
    }
}