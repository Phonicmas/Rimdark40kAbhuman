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
	    return pawn.Reserve(Herdstone, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
	{
		var ritual = ToilMaker.MakeToil("MakeNewToils");
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch)
		.FailOnDestroyedNullOrForbidden(TargetIndex.A);
		
		yield return Toils_General.Do(delegate
		{
			Herdstone.workingPawn = pawn;
		});

		ritual.defaultCompleteMode = ToilCompleteMode.Delay;
		ritual.defaultDuration = (int)Herdstone.TimeLeft+50;

		ritual.AddPreTickAction(delegate
		{
			Herdstone.WorkTick();
		});
		ritual.AddEndCondition(() => Herdstone.TimeLeft < 0 || !Herdstone.canBeWorked ? JobCondition.Succeeded : JobCondition.Ongoing);
		ritual.AddFinishAction(delegate
		{
			Herdstone.workingPawn = null;
		});
		yield return ritual;
	}
}