using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace Abhuman40k;

public class SitePartWorker_DownedNavigator : SitePartWorker_DownedRefugee
{
    public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
    {
        base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
        var pawnKind = Abhuman40kDefOf.BEWH_NavigatorRescue;
        var pawn = DownedRefugeeQuestUtility.GenerateRefugee(part.site.Tile, pawnKind, 0);
        part.things = new ThingOwner<Pawn>(part, oneStackOnly: true);
        part.things.TryAdd(pawn);
        pawn.relations?.everSeenByPlayer = false;
        var mostImportantColonyRelative = PawnRelationUtility.GetMostImportantColonyRelative(pawn);
        if (mostImportantColonyRelative != null)
        {
            var mostImportantRelation = mostImportantColonyRelative.GetMostImportantRelation(pawn);
            TaggedString text = "";
            if (mostImportantRelation != null && mostImportantRelation.opinionOffset > 0)
            {
                pawn.relations.relativeInvolvedInRescueQuest = mostImportantColonyRelative;
                text = "\n\n" + "RelatedPawnInvolvedInQuest".Translate(mostImportantColonyRelative.LabelShort, mostImportantRelation.GetGenderSpecificLabel(pawn), mostImportantColonyRelative.Named("RELATIVE"), pawn.Named("PAWN")).AdjustedFor(pawn);
            }
            else
            {
                PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, pawn);
            }
            outExtraDescriptionRules.Add(new Rule_String("pawnInvolvedInQuestInfo", text));
        }
        slate.Set("navigator", pawn);
    }

}