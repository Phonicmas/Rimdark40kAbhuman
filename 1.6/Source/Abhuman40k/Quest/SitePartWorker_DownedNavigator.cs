using System.Collections.Generic;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace Abhuman40k;

public class SitePartWorker_DownedNavigator : SitePartWorker_DownedRefugee
{
    public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
    {
        slate.Set("refugeeKind", Abhuman40kDefOf.BEWH_NavigatorRescue);

        base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);

        var pawn = slate.Get<Pawn>("refugee");
        if (pawn == null)
        {
            return;
        }

        if (pawn.relations != null)
        {
            pawn.relations.everSeenByPlayer = false;
        }

        slate.Set("navigator", pawn);
    }
}
