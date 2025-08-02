using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Abhuman40k;

public class CompAbilityEffect_WarpEyeGaze : CompAbilityEffect
{
    public new CompProperties_AbilityWarpEyeGaze Props => (CompProperties_AbilityWarpEyeGaze)props;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        Pawn pawn = (Pawn)target;
        DamageInfo dinfo = new DamageInfo
        {
            Def = Abhuman40kDefOf.BEWH_WarpGaze
        };
        pawn.Kill(dinfo);
    }

    public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
    {
        List<BodyPartGroupDef> invalidates = new List<BodyPartGroupDef>
        {
            BodyPartGroupDefOf.UpperHead,
            BodyPartGroupDefOf.FullHead
        };
        foreach (Apparel apparel in parent.pawn.apparel.WornApparel)
        {
            if (apparel.def.apparel.bodyPartGroups.Exists(x => invalidates.Contains(x)))
            {
                Messages.Message(text: "WarpEyeCovered".Translate(), new TargetInfo(parent.pawn.PositionHeld, parent.pawn.MapHeld), MessageTypeDefOf.NegativeEvent);
                return false;
            }
        }
        return base.CanApplyOn(target, dest);
    }

}