using RimWorld;
using Verse;


namespace Abhuman40k;

[DefOf]
public static class Abhuman40kSecondDefOf
{
    public static MentalStateDef Tantrum;
    public static ThoughtDef BEWH_FelinidNuzzle;
    
    static Abhuman40kSecondDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(Abhuman40kDefOf));
    }
}