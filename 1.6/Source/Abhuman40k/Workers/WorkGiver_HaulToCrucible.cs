using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Abhuman40k;

public class WorkGiver_HaulToCrucible : WorkGiver_Scanner
{
	private const float NutritionBuffer = 2.5f;

	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(Abhuman40kDefOf.BEWH_KinCrucible);

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
		if (t is not Building_Crucible buildingCrucible)
		{
			return false;
		}
		if (buildingCrucible.NutritionNeeded > NutritionBuffer)
		{
			if (FindNutrition(pawn, buildingCrucible).Thing == null)
			{
				JobFailReason.Is("NoFood".Translate());
				return false;
			}
			return true;
		}
		return false;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (t is not Building_Crucible buildingCrucible)
		{
			return null;
		}
		if (buildingCrucible.NutritionNeeded > 0f)
		{
			var thingCount = FindNutrition(pawn, buildingCrucible);
			if (thingCount.Thing != null)
			{
				var job = HaulAIUtility.HaulToContainerJob(pawn, thingCount.Thing, t);
				job.count = Mathf.Min(job.count, thingCount.Count);
				return job;
			}
		}
		return null;
	}

	private ThingCount FindNutrition(Pawn pawn, Building_Crucible vat)
	{
		var thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, Validator);
		if (thing == null)
		{
			return default;
		}
		var b = Mathf.CeilToInt(vat.NutritionNeeded / thing.GetStatValue(StatDefOf.Nutrition));
		return new ThingCount(thing, Mathf.Min(thing.stackCount, b));
		bool Validator(Thing x)
		{
			if (x.IsForbidden(pawn) || !pawn.CanReserve(x))
			{
				return false;
			}
			if (!vat.CanAcceptNutrition(x))
			{
				return false;
			}
			if (x.def.GetStatValueAbstract(StatDefOf.Nutrition) > vat.NutritionNeeded)
			{
				return false;
			}
			return true;
		}
	}
}