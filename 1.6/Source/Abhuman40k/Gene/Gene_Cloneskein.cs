using System.Linq;
using Verse;

namespace Abhuman40k;

public class Gene_Cloneskein : Gene
{
    private GeneDef chosenGene = null;
    
    public override void PostMake()
    {
        var cloneskeinMutations = DefDatabase<GeneDef>.AllDefsListForReading.Where(geneDef => geneDef.HasModExtension<DefModExtension_CloneskeinMutation>());
        chosenGene = cloneskeinMutations.RandomElement();
        base.PostMake();
    }
    
    public override void PostAdd()
    {
        base.PostAdd();
        pawn.genes.AddGene(chosenGene, true);
    }
    
    public override void PostRemove()
    {
        base.PostAdd();
        var gene = pawn.genes.GetGene(chosenGene);
        if (gene != null)
        {
            pawn.genes.RemoveGene(gene);
        }
    }
    
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref chosenGene, "chosenGene");
    }
}