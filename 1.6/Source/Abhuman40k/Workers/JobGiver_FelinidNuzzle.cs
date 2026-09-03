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

        var map = pawn.Map;
        if (map == null)
        {
            return null;
        }

        var origin = pawn.Position;
        var room = pawn.GetRoom();

        if (!map.mapPawns.SpawnedPawnsInFaction(pawn.Faction)
                .Where(p => p != pawn
                            && p.Position.InHorDistOf(origin, MaxNuzzleDistance)
                            && !p.NonHumanlikeOrWildMan() && !p.IsSubhuman
                            && p.GetRoom() == room
                            && !p.Position.IsForbidden(pawn)
                            && p.CanCasuallyInteractNow())
                .TryRandomElement(out var result))
        {
            return null;
        }

        var job = JobMaker.MakeJob(Abhuman40kDefOf.BEWH_FelinidNuzzle, result);
        job.locomotionUrgency = LocomotionUrgency.Walk;
        job.expiryInterval = 3000;
        return job;
    }
}