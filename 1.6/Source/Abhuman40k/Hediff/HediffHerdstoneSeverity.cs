using Verse;
using Core40k;
#pragma warning disable CS9266 // The '{0}' accessor of property '{1}' should use 'field' because the other accessor is using it.

namespace Abhuman40k;

public class HediffHerdstoneSeverity : HediffWithComps
{
    public override float Severity
    {
        get
        {
            if (pawn?.Map == null)
            {
                return 0;
            }

            var herdstoneCount = pawn.Map.listerBuildings.CountBuildingColonistOfDef(Abhuman40kDefOf.BEWH_HerdstonePlayer);
            var herdstoneConduitCount = pawn.Map.listerBuildings.CountBuildingColonistOfDef(Abhuman40kDefOf.BEWH_HerdstoneConduitPlayer);

            return SeverityCurve.Evaluate(herdstoneCount + herdstoneConduitCount);
        } 
        set;
    }

    private static readonly SimpleCurve SeverityCurve =
    [
        new CurvePoint(0f, 0.1f),
        new CurvePoint(1f, 1f),
        new CurvePoint(10f, 10f),
    ];
}