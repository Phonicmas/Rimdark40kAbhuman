using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Core40k;
using RimWorld;
using Verse;

namespace Abhuman40k;

[HarmonyPatch(typeof(StatWorker), "GetExplanationUnfinalized")]
public static class GetExplanationUnfinalizedFromAncestorCorePatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codeInstructions = instructions.ToList();
        var patched = false;
        for (var i = 0; i < codeInstructions.Count; i++)
        {
            if (!patched && codeInstructions[i+1].opcode == OpCodes.Ret)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StatWorker), "stat"));
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GetExplanationUnfinalizedFromAncestorCorePatch), "GetExplanationForRank"));
                yield return new CodeInstruction(OpCodes.Stloc_0);
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                patched = true;
            }
            yield return codeInstructions[i];
        }
    }

    public static StringBuilder GetExplanationForRank(StringBuilder stringBuilder, StatRequest req, StatDef stat)
    {
        if (stat == null || stat != StatDefOf.GlobalLearningFactor)
        {
            return stringBuilder;
        }

        if (req.Thing is not Pawn pawn)
        {
            return stringBuilder;
        }
        
        if (pawn.genes == null || !pawn.genes.HasActiveGene(Abhuman40kDefOf.BEWH_KinCloneskein))
        {
            return stringBuilder;
        }

        var totalBuriedKin = Abhuman40kUtils.TotalBuriedKin();

        if (totalBuriedKin == 0)
        {
            return stringBuilder;
        }

        var totalGlobalGain = totalBuriedKin * 2f / 100f;

        stringBuilder.AppendLine("BEWH.Abhuman.Kin.AncestorCore".Translate());
        stringBuilder.AppendLine("    " + "BEWH.Abhuman.Kin.AncestorCoreSharedKnowledge".Translate() + ": " + Core40kUtils.ValueToString(stat, totalGlobalGain, finalized: false, ToStringNumberSense.Offset));

        return stringBuilder;
    }
}