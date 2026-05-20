using Verse;

namespace Abhuman40k;

public class PlaceWorker_OnlyAvailableWithHerdstoneOnMap : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
    {
        if (map.listerBuildings.ColonistsHaveBuilding(Abhuman40kDefOf.BEWH_HerdstonePlayer))
        {
            return true;
        }

        return "BEWH.Abhuman.PlacementLimit.RequiresHerdstone".Translate(((ThingDef)checkingDef).label.CapitalizeFirst());
    }
}