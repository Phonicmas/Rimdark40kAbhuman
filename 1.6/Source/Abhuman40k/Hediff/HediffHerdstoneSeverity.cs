using Verse;
using Core40k;

namespace Abhuman40k;

public class HediffHerdstoneSeverity : HediffWithComps
{
    private const int RecacheIntervalTicks = 120;

    private float lastKnownSeverity = 0.1f;

    [Unsaved(false)]
    private int lastRecacheTick = -1;

    public override float Severity
    {
        get
        {
            // Off the map (caravan, transport pod) there is nothing to count. Returning 0 here
            // made Hediff.ShouldRemove true and silently deleted the hediff, so hold the last
            // value instead.
            var map = pawn?.Map;
            if (map == null)
            {
                return lastKnownSeverity;
            }

            // Counting buildings walks the whole colonist building list, and this getter is on a
            // very hot path, so it only recounts a few times a second.
            var ticksGame = Find.TickManager?.TicksGame ?? 0;
            if (lastRecacheTick >= 0 && ticksGame - lastRecacheTick < RecacheIntervalTicks)
            {
                return lastKnownSeverity;
            }

            lastRecacheTick = ticksGame;

            var herdstoneCount = map.listerBuildings.CountBuildingColonistOfDef(Abhuman40kDefOf.BEWH_HerdstonePlayer);
            var herdstoneConduitCount = map.listerBuildings.CountBuildingColonistOfDef(Abhuman40kDefOf.BEWH_HerdstoneConduitPlayer);

            lastKnownSeverity = SeverityCurve.Evaluate(herdstoneCount + herdstoneConduitCount);
            return lastKnownSeverity;
        }
        set => base.Severity = value;
    }

    private static readonly SimpleCurve SeverityCurve =
    [
        new CurvePoint(0f, 0.1f),
        new CurvePoint(1f, 1f),
        new CurvePoint(10f, 10f),
    ];

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref lastKnownSeverity, "lastKnownSeverity", 0.1f);
    }
}
