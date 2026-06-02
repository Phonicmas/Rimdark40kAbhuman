using RimWorld;
using Verse;


namespace Abhuman40k;

[DefOf]
public static class Abhuman40kDefOf
{
    //BodypartGroupDef
    public static BodyPartGroupDef Feet;
    
    //JobDef
    public static JobDef BEWH_FelinidNuzzle;
    public static JobDef BEWH_DoMinotaurRitual;
    
    //WorldObjectDef
    public static WorldObjectDef BEWH_NavigatorWarpTravel;
    
    //MentalBreakDef
    public static MentalBreakDef Tantrum;
    
    //InteractionDef
    public static InteractionDef BEWH_Nuzzle;
    
    //GeneDef
    public static GeneDef BEWH_FelinidCatlikeMindset;
    
    public static GeneDef BEWH_BeastmanHerdstoneAffinity;
    public static GeneDef BEWH_MinotaurAdrenalSurgeNode;

    public static GeneDef BEWH_NavigtorUnsettlingPresence;
    public static GeneDef BEWH_NavigtorHouseCastana;
    public static GeneDef BEWH_NavigtorHouseAchelieux;
    public static GeneDef BEWH_NavigtorWarpNavigation;
    
    public static GeneDef BEWH_OgrynSlowWitted;
    public static GeneDef BEWH_OgrynToddlerLogic;
    public static GeneDef BEWH_OgrynBoneEad;
    
    public static GeneDef BEWH_RatlingGregarious;
    public static GeneDef BEWH_RatlingScavengerInstinct;

    //DamageDef
    public static DamageDef BEWH_WarpGaze;

    //LetterDef
    public static LetterDef BEWH_WarpTravel;
    
    public static ThingDef BEWH_HerdstonePlayer;
    public static ThingDef BEWH_HerdstoneConduitPlayer;
    
    public static ThingDef BEWH_ReactorConduit;
    
    //XenotypeDef
    public static XenotypeDef BEWH_Navigator;
    
    //PawnKindDef
    public static PawnKindDef BEWH_HerdstoneBornMinotaur;
    public static PawnKindDef BEWH_NavigatorRescue;

    static Abhuman40kDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(Abhuman40kDefOf));
    }
}