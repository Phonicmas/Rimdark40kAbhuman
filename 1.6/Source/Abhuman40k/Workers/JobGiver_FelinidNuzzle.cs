using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Abhuman40k;

public class JobGiver_FelinidNuzzle : ThinkNode_JobGiver
{
    private const float MaxNuzzleDistance = 40f;

    protected override Job TryGiveJob(Pawn pawn)
    {
        var gene = pawn.genes?.GetFirstGeneOfType<Gene_CatlikeMindset>();
        
        if (gene is not { CanNuzzle: true })
        {
            return null;
        }

        if (pawn.story.traits.HasTrait(TraitDefOf.Psychopath))
        {
            return null;
        }

        if (!(from p in pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction)
                where !p.NonHumanlikeOrWildMan() && !p.IsSubhuman && p != pawn &&
                      p.Position.InHorDistOf(pawn.Position, MaxNuzzleDistance) && pawn.GetRoom() == p.GetRoom() &&
                      !p.Position.IsForbidden(pawn) && p.CanCasuallyInteractNow()
                select p).TryRandomElement(out var result))
        {
            return null;
        }

        var job = JobMaker.MakeJob(Abhuman40kDefOf.BEWH_FelinidNuzzle, result);
        job.locomotionUrgency = LocomotionUrgency.Walk;
        job.expiryInterval = 3000;
        return job;
    }
}