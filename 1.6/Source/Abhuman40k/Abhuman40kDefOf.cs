using RimWorld;
using Verse;


namespace Abhuman40k;

[DefOf]
public static class Abhuman40kDefOf
{
    public static WorldObjectDef BEWH_NavigatorWarpTravel;
    
    public static MentalBreakDef Tantrum;
    
    public static GeneDef BEWH_KinGrudgy;
    public static GeneDef BEWH_KinClanLoyalty;
    
    public static GeneDef BEWH_NavigtorUnsettlingPresence;
    public static GeneDef BEWH_NavigtorHouseCastana;
    public static GeneDef BEWH_NavigtorHouseAchelieux;
    public static GeneDef BEWH_NavigtorWarpNavigation;
    
    public static GeneDef BEWH_OgrynSlowWitted;
    public static GeneDef BEWH_OgrynToddlerLogic;
    public static GeneDef BEWH_OgrynBoneEad;
    
    public static GeneDef BEWH_RatlingGregarious;
    public static GeneDef BEWH_RatlingScavengerInstinct;

    public static DamageDef BEWH_WarpGaze;

    public static LetterDef BEWH_WarpTravel;
    
    public static ThingDef BEWH_AncestorCore;
    public static ThingDef BEWH_KinCrucible;
    
    public static XenotypeDef BEWH_Kin;

    static Abhuman40kDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(Abhuman40kDefOf));
    }
}