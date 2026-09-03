using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Abhuman40k;

public class StorytellerComp_NavigatorDowned : StorytellerComp
{
    private StorytellerCompProperties_NavigatorDowned Props => (StorytellerCompProperties_NavigatorDowned)props;

    public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
    {
        var component = GameComponent_NavigatorQuest.Instance;
        if (component == null || component.NavigatorSecured)
        {
            yield break;
        }

        if (GameComponent_NavigatorQuest.QuestActive)
        {
            yield break;
        }

        var ticksGame = Find.TickManager.TicksGame;
        if (ticksGame < Props.fireAfterDaysPassed * GenDate.TicksPerDay)
        {
            yield break;
        }

        if (component.NextEarliestFireTick > 0 && ticksGame < component.NextEarliestFireTick)
        {
            yield break;
        }

        if (Props.skipIfOnExtremeBiome)
        {
            var homeMap = Find.AnyPlayerHomeMap;
            if (homeMap == null || homeMap.Biome.isExtremeBiome)
            {
                yield break;
            }
        }

        var incident = Props.incident;
        if (incident == null || !incident.TargetAllowed(target))
        {
            yield break;
        }

        component.Notify_QuestOffered();
        yield return new FiringIncident(incident, this, GenerateParms(incident.category, target));
    }
}
