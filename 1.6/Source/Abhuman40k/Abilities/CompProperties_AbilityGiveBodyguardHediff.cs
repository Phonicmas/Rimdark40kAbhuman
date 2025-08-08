using RimWorld;
using Verse;

namespace Abhuman40k;

public class CompProperties_AbilityGiveBodyguardHediff : CompProperties_AbilityEffect
{
    public HediffDef hediffDef;
    
    public CompProperties_AbilityGiveBodyguardHediff()
    {
        compClass = typeof(CompAbilityEffect_GiveBodyguardHediff);
    }
}