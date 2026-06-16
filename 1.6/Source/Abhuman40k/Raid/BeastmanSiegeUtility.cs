using System.Collections.Generic;
using RimWorld;
using VEF;
using VEF.Planet;
using Verse;

namespace Abhuman40k;

public class BeastmanSiegeUtility
{
	private static SiegeParameterSetDef customParams;

	public static Blueprint_Build PlaceBlueprint(LordToilData_SiegeBeastman data, Map map, Faction placeFaction)
	{
		NonPublicFields.SiegeBlueprintPlacer_center() = data.siegeCenter;
		NonPublicFields.SiegeBlueprintPlacer_faction() = placeFaction;
		
		var beastmanHerdstoneDef = Abhuman40kDefOf.BEWH_HerdstoneRaid;
		var rot = Rot4.South;
		var artySpot = NonPublicMethods.SiegeBlueprintPlacer_FindArtySpot(beastmanHerdstoneDef, rot, map);
		return GenConstruct.PlaceBlueprintForBuild(beastmanHerdstoneDef, artySpot, map, rot, NonPublicFields.SiegeBlueprintPlacer_faction(), GenStuff.DefaultStuffFor(beastmanHerdstoneDef));
	}
}