using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Abhuman40k;

public class Recipe_Boner : Recipe_Surgery
{
    public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
    {
        if (thing is not Pawn pawn || pawn.genes == null)
        {
            return false;
        }
        
        if (!pawn.genes.HasActiveGene(Abhuman40kDefOf.BEWH_OgrynSlowWitted))
        {
            return false;
        }
        
        return base.AvailableOnNow(thing, part);
    }
    
    public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
    {
        if (thing is not Pawn pawn || pawn.genes == null)
        {
            return false;
        }

        if (!pawn.genes.HasActiveGene(Abhuman40kDefOf.BEWH_OgrynSlowWitted))
        {
            return new AcceptanceReport("BEWH.Abhuman.Ogryn.MissingSlowWitted".Translate(pawn, Abhuman40kDefOf.BEWH_OgrynSlowWitted));
        }
        
        return AvailableOnNow(thing, part);
    }
    
    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
    {
        if (billDoer != null)
        {
            if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
            {
                return;
            }
            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
        }
        
        OnSurgerySuccess(pawn, part, billDoer, ingredients, bill);
        if (IsViolationOnPawn(pawn, part, Faction.OfPlayerSilentFail))
        {
            ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
        }
    }

    protected override void OnSurgerySuccess(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
    {
        pawn.genes.AddGene(Abhuman40kDefOf.BEWH_OgrynBoneEad, true);
    }
}