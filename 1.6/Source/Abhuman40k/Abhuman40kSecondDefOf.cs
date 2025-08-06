using RimWorld;
using Verse;


namespace Abhuman40k;

[DefOf]
public static class Abhuman40kSecondDefOf
{
    public static MentalStateDef Tantrum;

    static Abhuman40kSecondDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(Abhuman40kDefOf));
    }
}