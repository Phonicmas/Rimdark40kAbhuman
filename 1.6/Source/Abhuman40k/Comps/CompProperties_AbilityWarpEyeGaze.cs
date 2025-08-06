using System.Collections.Generic;
using RimWorld;
using Verse;


namespace Abhuman40k;

public class CompProperties_AbilityWarpEyeGaze : CompProperties_AbilityEffect
{
    public List<GeneDef> stunnedNotKilledGeneDef = new ();
    public List<PawnKindDef> stunnedNotKilledPawnKindDef = new ();
    
    public CompProperties_AbilityWarpEyeGaze()
    {
        compClass = typeof(CompAbilityEffect_WarpEyeGaze);
    }
}