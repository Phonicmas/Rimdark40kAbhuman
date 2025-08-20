using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Abhuman40k;

public class Building_AncestorCore : Building
{
    private Pawn ancestorCorePawn = null;

    public Pawn AncestorCorePawn
    {
        get
        {
            if (ancestorCorePawn == null)
            {
                var request = new PawnGenerationRequest(PawnKindDefOf.Colonist, null,
                    PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false,
                    allowDowned: true, canGeneratePawnRelations: false, mustBeCapableOfViolence: false, 0f,
                    forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true,
                    allowAddictions: false, inhabitant: false, certainlyBeenInCryptosleep: false,
                    forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 0f,
                    null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: true,
                    forceNoBackstory: false, forbidAnyTitle: false, false, null, null, Abhuman40kDefOf.BEWH_Kin);
                ancestorCorePawn = PawnGenerator.GeneratePawn(request);
                ancestorCorePawn.Name = new NameSingle("BEWH.Abhuman.Kin.AncestorCore".Translate());
                ancestorCorePawn.Kill(null);
            }

            return ancestorCorePawn;
        }
    }
    
    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }
    }
    
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref ancestorCorePawn, "ancestorCorePawn");
    }
}