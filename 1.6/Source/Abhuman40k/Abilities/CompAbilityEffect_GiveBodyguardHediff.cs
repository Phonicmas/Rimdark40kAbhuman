using RimWorld;
using Verse;

namespace Abhuman40k;

public class CompAbilityEffect_GiveBodyguardHediff : CompAbilityEffect
{
    public new CompProperties_AbilityGiveBodyguardHediff Props => (CompProperties_AbilityGiveBodyguardHediff)props;
    
    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        ApplyInner(target.Pawn, parent.pawn);
    }

    private void ApplyInner(Pawn target, Pawn other)
    {
        if (target == null)
        {
            return;
        }

        var hediff = HediffMaker.MakeHediff(Props.hediffDef, target, target.health.hediffSet.GetBodyPartRecord(BodyPartDefOf.Torso));

        var gene = parent.pawn?.genes?.GetFirstGeneOfType<Gene_Belisarius>();

        gene?.SetNewBodyguard(target, Props.hediffDef);
        target.health.AddHediff(hediff);
    }

    public override bool AICanTargetNow(LocalTargetInfo target)
    {
        return false;
    }
}