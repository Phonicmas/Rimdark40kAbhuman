using RimWorld;
using Verse;
using Verse.AI;

namespace Abhuman40k;

/// <summary>
/// Sends a beastman shaman to his own assigned spot around the raid herdstone to chant.
/// Returns null while there is no stone yet (so the duty falls through to building it) and
/// while the pawn is in real need of food or sleep.
/// </summary>
public class JobGiver_HerdstonePray : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		var duty = pawn.mindState.duty;
		if (duty == null)
		{
			return null;
		}

		if (pawn.needs?.food is { CurCategory: >= HungerCategory.UrgentlyHungry })
		{
			return null;
		}

		if (pawn.needs?.rest is { CurCategory: >= RestCategory.VeryTired })
		{
			return null;
		}

		var herdstone = FindHerdstone(pawn, duty);
		if (herdstone == null)
		{
			return null;
		}

		var cell = herdstone.GetPrayerCellFor(pawn);
		if (!cell.IsValid)
		{
			return null;
		}

		var job = JobMaker.MakeJob(Abhuman40kDefOf.BEWH_HerdstonePray, herdstone, cell);
		job.locomotionUrgency = LocomotionUrgency.Walk;

		// Chanting is a long job, so re-run the think tree once a second while anything hostile
		// is close. That lets the JobGiver_AIDefendPoint node above this one grab the shaman the
		// moment something walks into bow range, instead of him praying through the fight.
		job.expiryInterval = 60;
		job.checkOverrideOnExpire = true;
		job.expireRequiresEnemiesNearby = true;
		return job;
	}

	/// <summary>The nearest friendly raid herdstone to the duty's focus.</summary>
	private static Building_HerdstoneEnemy FindHerdstone(Pawn pawn, PawnDuty duty)
	{
		var map = pawn.Map;
		if (map == null)
		{
			return null;
		}

		var focus = duty.focus.IsValid ? duty.focus.Cell : pawn.Position;
		var maxDist = duty.radius > 0f ? duty.radius * 2f : 40f;
		var maxDistSquared = maxDist * maxDist;

		Building_HerdstoneEnemy best = null;
		var bestDistSquared = float.MaxValue;

		var candidates = map.listerThings.ThingsOfDef(Abhuman40kDefOf.BEWH_HerdstoneRaid);
		foreach (var thing in candidates)
		{
			if (thing is not Building_HerdstoneEnemy herdstone || herdstone.Faction != pawn.Faction)
			{
				continue;
			}

			float distSquared = (herdstone.Position - focus).LengthHorizontalSquared;
			if (distSquared > maxDistSquared || distSquared >= bestDistSquared)
			{
				continue;
			}

			best = herdstone;
			bestDistSquared = distSquared;
		}

		return best;
	}
}
