using System.Linq;
using LudeonTK;
using Verse;

namespace Abhuman40k;

public static class DebugActions
{
    [DebugAction("RimDark", "Log current warp travels", false, false, true, false, false,0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
    private static void WarpTravelsInfo()
    {
        var warpTravelObjects = Find.WorldObjects.AllWorldObjects.Where(o => o is WarpTravelWorldObject).Cast<WarpTravelWorldObject>().ToList();
        Log.Message("Current warp travels: " + warpTravelObjects.Count);
        var currentTime = Current.Game.tickManager.TicksGame;
        foreach (var warpTravel in warpTravelObjects)
        {
            Log.Message("Navigator: " + warpTravel.Navigator);
            Log.Message("Traveler Amount: " + warpTravel.GetDirectlyHeldThings().Count);
            Log.Message("Time left: " + (warpTravel.arrivalTick - currentTime));
            Log.Message("Target: " + warpTravel.Tile);
        }
    }
}