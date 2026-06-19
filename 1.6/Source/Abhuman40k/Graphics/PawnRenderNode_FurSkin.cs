using UnityEngine;
using Verse;

namespace Abhuman40k;

public class PawnRenderNode_FurSkin : PawnRenderNode_Fur
{
    public PawnRenderNode_FurSkin(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
    {
    }

    public override Color ColorFor(Pawn pawn)
    {
        return pawn.story.SkinColor;
    }
}