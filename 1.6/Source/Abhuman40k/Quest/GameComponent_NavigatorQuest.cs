using System.Linq;
using RimWorld;
using Verse;

namespace Abhuman40k;

public class GameComponent_NavigatorQuest : GameComponent
{
    private const int CheckIntervalTicks = 2500;
    private const int DefaultRetryDelayDays = 30;

    private bool navigatorSecured;
    private int nextEarliestFireTick = -1;
    private int trackedQuestId = -1;

    public GameComponent_NavigatorQuest(Game game)
    {
    }

    public static GameComponent_NavigatorQuest Instance => Current.Game?.GetComponent<GameComponent_NavigatorQuest>();

    public bool NavigatorSecured => navigatorSecured;

    public int NextEarliestFireTick => nextEarliestFireTick;

    public void Notify_NavigatorSecured()
    {
        navigatorSecured = true;
        trackedQuestId = -1;
    }

    public void Notify_QuestOffered()
    {
        nextEarliestFireTick = Find.TickManager.TicksGame + RetryDelayTicks;
    }

    public static bool QuestActive
    {
        get
        {
            var quests = Find.QuestManager?.QuestsListForReading;
            if (quests == null)
            {
                return false;
            }

            return quests.Any(quest => quest.root == Abhuman40kDefOf.BEWH_NavigatorDowned
                                       && quest.State is QuestState.NotYetAccepted or QuestState.Ongoing);
        }
    }

    public override void GameComponentTick()
    {
        base.GameComponentTick();

        if (navigatorSecured || Find.TickManager.TicksGame % CheckIntervalTicks != 0)
        {
            return;
        }

        var quest = Find.QuestManager?.QuestsListForReading
            .FirstOrDefault(q => q.root == Abhuman40kDefOf.BEWH_NavigatorDowned
                                 && q.State is QuestState.NotYetAccepted or QuestState.Ongoing);

        if (quest != null)
        {
            trackedQuestId = quest.id;
            return;
        }

        if (trackedQuestId < 0)
        {
            return;
        }

        trackedQuestId = -1;
        nextEarliestFireTick = Find.TickManager.TicksGame + RetryDelayTicks;
    }

    private static int RetryDelayTicks
    {
        get
        {
            var comps = Find.Storyteller?.storytellerComps;
            if (comps == null)
            {
                return DefaultRetryDelayDays * GenDate.TicksPerDay;
            }

            foreach (var comp in comps)
            {
                if (comp.props is StorytellerCompProperties_NavigatorDowned navigatorProps)
                {
                    return navigatorProps.retryDelayDays * GenDate.TicksPerDay;
                }
            }

            return DefaultRetryDelayDays * GenDate.TicksPerDay;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref navigatorSecured, "navigatorSecured");
        Scribe_Values.Look(ref nextEarliestFireTick, "nextEarliestFireTick", -1);
        Scribe_Values.Look(ref trackedQuestId, "trackedQuestId", -1);
    }
}
