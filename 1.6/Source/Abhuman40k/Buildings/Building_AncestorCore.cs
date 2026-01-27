using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Abhuman40k;

public class Building_AncestorCore : Building
{
    private int storedKin = 0;
    public int StoredKin => storedKin;
    
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
    
    private Corpse targetCorpse = null;
    public Corpse TargetCorpse => targetCorpse;

    public void UploadKinToCore(Corpse corpse)
    {
        storedKin++;
        corpse.Destroy();
        targetCorpse = null;
    }
    
    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }
        
        var corpses = Map.listerThings
            .ThingsInGroup(ThingRequestGroup.Corpse)
            .Cast<Corpse>()
            .Where(corpse => 
                corpse.InnerPawn.RaceProps.Humanlike 
                && corpse.InnerPawn.Faction == Faction.OfPlayer 
                && corpse.InnerPawn.genes.HasActiveGene(Abhuman40kDefOf.BEWH_KinCloneskein))
            .ToList();
        
        var funeralCommand = new Command_Action
        {
            defaultLabel = "BEWH.Abhuman.Kin.AncestorCoreFuneral".Translate(),
            action = delegate
            {
                var list = new List<FloatMenuOption>();
                foreach (var corpse in corpses)
                {
                    list.Add(new FloatMenuOption(corpse.InnerPawn.LabelShort, delegate
                    {
                        targetCorpse = corpse;
                    }));
                }
                if (!list.NullOrEmpty())
                {
                    Find.WindowStack.Add(new FloatMenu(list));
                }
            }
        };
        if (corpses.EnumerableNullOrEmpty())
        {
            funeralCommand.Disabled = true;
            funeralCommand.disabledReason = "BEWH.Abhuman.Kin.AncestorCoreNoDeadKin".Translate();
        }
        yield return funeralCommand;
    }
    
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref ancestorCorePawn, "ancestorCorePawn");
        Scribe_Deep.Look(ref targetCorpse, "targetCorpse");
        Scribe_Values.Look(ref storedKin, "storedKin");
    }
}