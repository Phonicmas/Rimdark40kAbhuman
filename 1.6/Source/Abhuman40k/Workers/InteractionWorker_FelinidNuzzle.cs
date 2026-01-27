using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Abhuman40k;

public class InteractionWorker_FelinidNuzzle : InteractionWorker
{
    public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
    {
        AddNuzzledThought(initiator, recipient);
        letterText = null;
        letterLabel = null;
        letterDef = null;
        lookTargets = null;
    }

    private void AddNuzzledThought(Pawn initiator, Pawn recipient)
    {
        var thoughtMemoryRecipient = (Thought_MemoryFelinid)ThoughtMaker.MakeThought(Abhuman40kSecondDefOf.BEWH_FelinidNuzzle);
        thoughtMemoryRecipient.SetForcedStage(0);
        thoughtMemoryRecipient.initiator = initiator;
        recipient.needs.mood?.thoughts.memories.TryGainMemory(thoughtMemoryRecipient);
        
        var thoughtMemoryInitiator = (Thought_Memory)ThoughtMaker.MakeThought(Abhuman40kSecondDefOf.BEWH_FelinidNuzzle);
        thoughtMemoryInitiator.SetForcedStage(1);
        initiator.needs.mood?.thoughts.memories.TryGainMemory(thoughtMemoryInitiator);
    }
}