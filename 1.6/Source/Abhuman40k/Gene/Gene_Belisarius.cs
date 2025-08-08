using Verse;

namespace Abhuman40k;

public class Gene_Belisarius : Gene
{
    private Pawn currentBodyguard = null;

    public void SetNewBodyguard(Pawn newBodyguard, HediffDef hediffDef)
    {
        var hediff = currentBodyguard?.health.hediffSet.GetFirstHediffOfDef(hediffDef);
        if (hediff != null)
        {
            currentBodyguard.health.RemoveHediff(hediff);
        }
        currentBodyguard = newBodyguard;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref currentBodyguard, "currentBodyguard");
    }
}