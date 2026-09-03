using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Abhuman40k;

public class CompAbilityEffect_WarpEyeGaze : CompAbilityEffect
{
    public new CompProperties_AbilityWarpEyeGaze Props => (CompProperties_AbilityWarpEyeGaze)props;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        var targetPawn = target.Pawn;
        if (targetPawn == null || targetPawn.Dead)
        {
            return;
        }

        if (StunNotKill(targetPawn))
        {
            var stunDuration = (int)Math.Max(300, targetPawn.GetStatValue(StatDefOf.PsychicSensitivity) * 300);
            targetPawn.stances?.stunner.StunFor(stunDuration, parent.pawn);
        }
        else
        {
            targetPawn.Kill(new DamageInfo(Abhuman40kDefOf.BEWH_WarpGaze, 9999f, instigator: parent.pawn));
        }
    }

    private bool StunNotKill(Pawn pawn)
    {
        if (pawn == null)
        {
            return true;
        }

        if (Props.stunnedNotKilledPawnKindDef.Contains(pawn.kindDef))
        {
            return true;
        }

        if (pawn.genes == null)
        {
            return false;
        }

        return Enumerable.Any(pawn.genes.GenesListForReading, gene => Props.stunnedNotKilledGeneDef.Contains(gene.def));
    }
    
    public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
    {
        if (target.Pawn == null)
        {
            return base.ExtraLabelMouseAttachment(target);
        }
        
        return StunNotKill(target.Pawn) ? "BEWH.Abhuman.Navigator.WillBeStunned".Translate() : "BEWH.Abhuman.Navigator.WillBeKilled".Translate();
    }
}