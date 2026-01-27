using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Abhuman40k;

public class WorkGiver_HaulCorpseToAncestorCore : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(Abhuman40kDefOf.BEWH_AncestorCore);

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

		if (t is not Building_AncestorCore ancestorCore)
		{
			return false;
		}

		return ancestorCore.TargetCorpse != null;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (t is not Building_AncestorCore ancestorCore)
		{
			return null;
		}
		
		var job = JobMaker.MakeJob(Abhuman40kDefOf.BEWH_CarryKinToAncestorCore, ancestorCore, ancestorCore.TargetCorpse);
		return job;
	}
}