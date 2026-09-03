using RimWorld;

namespace Abhuman40k;

public class StorytellerCompProperties_NavigatorDowned : StorytellerCompProperties
{
    public IncidentDef incident;
    public int fireAfterDaysPassed = 50;
    public int retryDelayDays = 30;
    public bool skipIfOnExtremeBiome;

    public StorytellerCompProperties_NavigatorDowned()
    {
        compClass = typeof(StorytellerComp_NavigatorDowned);
    }
}
