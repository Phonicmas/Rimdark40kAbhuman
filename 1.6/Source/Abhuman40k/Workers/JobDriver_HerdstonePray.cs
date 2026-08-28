using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Abhuman40k;

/// <summary>
/// Stand at the assigned spot facing the raid herdstone and chant, throwing off a dark haze
/// and the occasional spark. Re-issued on a loop by <see cref="JobGiver_HerdstonePray"/>.
/// </summary>
public class JobDriver_HerdstonePray : JobDriver
{
	private const int PrayDurationTicks = 600;

	private Building_HerdstoneEnemy Herdstone => job.GetTarget(TargetIndex.A).Thing as Building_HerdstoneEnemy;

	[Unsaved(false)]
	private Effecter chantEffecter;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		// Raiders don't share a reservation manager with the colony and every shaman gets his
		// own cell from the herdstone, so there is nothing to reserve.
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOn(() => Herdstone == null);

		yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);

		var pray = ToilMaker.MakeToil("HerdstonePray");
		pray.defaultCompleteMode = ToilCompleteMode.Delay;
		pray.defaultDuration = PrayDurationTicks;
		pray.handlingFacing = true;
		pray.socialMode = RandomSocialMode.Off;
		pray.tickAction = delegate
		{
			var herdstone = Herdstone;
			if (herdstone is { Spawned: true })
			{
				pawn.rotationTracker.FaceTarget(herdstone);
			}

			chantEffecter ??= Abhuman40kDefOf.BEWH_HerdstoneChant.Spawn();
			chantEffecter.EffectTick(pawn, pawn);
		};
		pray.AddFinishAction(CleanupEffecter);
		yield return pray;
	}

	private void CleanupEffecter()
	{
		chantEffecter?.Cleanup();
		chantEffecter = null;
	}
}
