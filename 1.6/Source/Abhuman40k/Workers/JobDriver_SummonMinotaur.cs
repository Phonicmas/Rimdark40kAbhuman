using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Abhuman40k;

public class JobDriver_SummonMinotaur : JobDriver
{
    private Building_Herdstone Herdstone => job.GetTarget(TargetIndex.A).Thing as Building_Herdstone;
    
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
	    var herdstone = Herdstone;
	    return herdstone != null && pawn.Reserve(herdstone, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOn(() => Herdstone == null);

		var ritual = ToilMaker.MakeToil("MakeNewToils");
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
		.FailOnDestroyedNullOrForbidden(TargetIndex.A);

		ritual.defaultCompleteMode = ToilCompleteMode.Delay;
		ritual.defaultDuration = Herdstone != null ? (int)Herdstone.TimeLeft + 50 : 50;

		ritual.AddPreTickAction(delegate
		{
			Herdstone?.WorkTick();
		});
		ritual.AddEndCondition(delegate
		{
			var herdstone = Herdstone;
			if (herdstone == null)
			{
				return JobCondition.Incompletable;
			}

			return herdstone.TimeLeft < 0 || !herdstone.canBeWorked ? JobCondition.Succeeded : JobCondition.Ongoing;
		});
		yield return ritual;
	}
}