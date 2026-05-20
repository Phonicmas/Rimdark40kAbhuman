using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Abhuman40k;

public class JobDriver_OpenContractWindow : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_General.Do(delegate
        {
            Find.WindowStack.Add(new Dialog_Contracts());
        });
    }
}