using RimWorld;
using Verse;
using Verse.AI;

namespace Abhuman40k;

public class WorkGiver_HerdstoneRitual : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(Abhuman40kDefOf.BEWH_HerdstonePlayer);

	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
		{
			return false;
		}
		if (t.IsBurning())
		{
			return false;
		}
		if (t is not Building_Herdstone buildingHerdstone)
		{
			return false;
		}
		if (pawn.genes == null || !pawn.genes.HasActiveGene(Abhuman40kDefOf.BEWH_BeastmanHerdstoneAffinity))
		{
			JobFailReason.Is("BEWH.Abhuman.Beastman.HerdstoneNoAffinityGene".Translate());
		}
		if (!buildingHerdstone.canBeWorked)
		{
			JobFailReason.Is("BEWH.Abhuman.Beastman.HerdstoneNotStarted".Translate());
		}
		
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return null;
		}
		if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
		{
			return null;
		}
		if (t.IsBurning())
		{
			return null;
		}
		if (t is not Building_Herdstone buildingHerdstone)
		{
			return null;
		}
		if (pawn.genes == null || !pawn.genes.HasActiveGene(Abhuman40kDefOf.BEWH_BeastmanHerdstoneAffinity))
		{
			return null;
		}
		if (!buildingHerdstone.canBeWorked)
		{
			return null;
		}
		
		return JobMaker.MakeJob(Abhuman40kDefOf.BEWH_DoMinotaurRitual, buildingHerdstone);;
	}
}