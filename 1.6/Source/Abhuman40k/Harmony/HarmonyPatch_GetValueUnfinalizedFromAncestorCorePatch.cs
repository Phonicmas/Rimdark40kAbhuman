using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using RimWorld;
using Verse;

namespace Abhuman40k;

[HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
public static class GetValueUnfinalizedFromAncestorCorePatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var addedOffset = false;
        var addedFactor = false;
        var codeInstructions = instructions.ToList();
        foreach (var instruction in codeInstructions)
        {
            if (!addedFactor && instruction.opcode == OpCodes.Ret)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StatWorker), "stat"));
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GetValueUnfinalizedFromAncestorCorePatch), "GetStatFactorForRank"));
                yield return new CodeInstruction(OpCodes.Stloc_0);
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                addedFactor = true;
            }
                
            yield return instruction;
                
            if (!addedOffset && instruction.opcode == OpCodes.Stloc_0)
            {
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Ldarg_1);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StatWorker), "stat"));
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GetValueUnfinalizedFromAncestorCorePatch), "GetStatOffsetForRank"));
                yield return new CodeInstruction(OpCodes.Stloc_0);
                addedOffset = true;
            }
        }
    }

    public static float GetStatOffsetForRank(float num, StatRequest req, StatDef stat)
    {
        if (stat == null || stat != StatDefOf.GlobalLearningFactor)
        {
            return num;
        }
        
        if (req.Thing is not Pawn pawn)
        {
            return num;
        }

        if (pawn.genes == null || !pawn.genes.HasActiveGene(Abhuman40kDefOf.BEWH_KinCloneskein))
        {
            return num;
        }

        var totalBuriedKin = Abhuman40kUtils.TotalBuriedKin();

        if (totalBuriedKin == 0)
        {
            return num;
        }

        var totalGlobalGain = totalBuriedKin * 2f / 100f;
        
        num += totalGlobalGain;

        return num;
    }
}