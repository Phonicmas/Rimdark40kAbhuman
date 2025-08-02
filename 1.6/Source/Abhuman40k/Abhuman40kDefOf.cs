using RimWorld;
using Verse;


namespace Abhuman40k;

[DefOf]
public static class Abhuman40kDefOf
{
    public static GeneDef BEWH_KinGrudgy;

    public static DamageDef BEWH_WarpGaze;

    public static VEF.Abilities.AbilityDef BEWH_WarpEyeWarpTravel;

    public static LetterDef BEWH_WarpTravel;

    static Abhuman40kDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(Abhuman40kDefOf));
    }
}