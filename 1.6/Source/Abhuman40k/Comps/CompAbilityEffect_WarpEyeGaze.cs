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
        if (StunNotKill(targetPawn))
        {
            var stunDuration = (int)Math.Max(300, targetPawn.GetStatValue(StatDefOf.PsychicSensitivity) * 300);
            targetPawn.stances.stunner.StunFor(stunDuration, parent.pawn);
        }
        else
        {
            var dinfo = new DamageInfo
            {
                Def = Abhuman40kDefOf.BEWH_WarpGaze
            };
            targetPawn.Kill(dinfo);
        }
    }

    private bool StunNotKill(Pawn pawn)
    {
        if (Props.stunnedNotKilledPawnKindDef.Contains(pawn.kindDef))
        {
            return true;
        }

        if (Enumerable.Any(pawn.genes.GenesListForReading, gene => Props.stunnedNotKilledGeneDef.Contains(gene.def)))
        {
            return true;
        }

        return false;
    }
    
    public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
    {
        if (target.Pawn == null)
        {
            return base.ExtraLabelMouseAttachment(target);
        }
        
        return StunNotKill(target.Pawn) ? "BEWH.Abhuman.Navigator.WillBeStunned".Translate() : "BEWH.Abhuman.Navigator.WillBeKilled".Translate();
    }

    public override bool GizmoDisabled(out string reason)
    {
        var invalidates = new List<BodyPartGroupDef>
        {
            BodyPartGroupDefOf.UpperHead,
            BodyPartGroupDefOf.FullHead
        };
        foreach (var apparel in parent.pawn.apparel.WornApparel)
        {
            if (!apparel.def.apparel.bodyPartGroups.Exists(x => invalidates.Contains(x)))
            {
                continue;
            }

            reason = "BEWH.Abhuman.Navigator.WarpEyeCovered".Translate(apparel.Label);
            return true;
        }

        reason = null;
        return false;
    }

}