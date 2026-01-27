using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Abhuman40k;

public class JobDriver_FelinidNuzzle : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOnNotCasualInterruptible(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
        Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).socialMode = RandomSocialMode.Off;
        yield return Toils_General.Do(delegate
        {
            var recipient = (Pawn)pawn.CurJob.targetA.Thing;
            pawn.interactions.TryInteractWith(recipient, Abhuman40kDefOf.BEWH_Nuzzle);
            pawn.genes.GetFirstGeneOfType<Gene_CatlikeMindset>().Nuzzled();
        });
    }
}