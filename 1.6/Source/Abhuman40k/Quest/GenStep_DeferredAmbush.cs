using Verse;

namespace Abhuman40k;

public class GenStep_DeferredAmbush : GenStep
{
    public FloatRange defaultPointsRange = new(180f, 340f);

    public override int SeedPart => 618394511;

    public override void Generate(Map map, GenStepParams parms)
    {
        var points = parms.sitePart != null
            ? parms.sitePart.parms.threatPoints
            : defaultPointsRange.RandomInRange;

        map.GetComponent<MapComponent_NavigatorRescue>()?.SetAmbushPoints(points);
    }
}
