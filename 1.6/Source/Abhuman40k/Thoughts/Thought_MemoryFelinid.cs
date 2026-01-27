using RimWorld;
using Verse;

namespace Abhuman40k;

public class Thought_MemoryFelinid : Thought_Memory
{
    public Pawn initiator = null;
    
    public override string Description
    {
        get
        {
            if (CurStageIndex == 0 || initiator == null)
            {
                return "BEWH.Abhuman.Felinid.Nuzzled".Translate(initiator);
            }

            return base.Description;
        }
    }
}