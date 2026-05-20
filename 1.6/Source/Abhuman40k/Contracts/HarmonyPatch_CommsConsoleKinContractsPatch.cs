using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using VEF.Maps;
using Verse;
using Verse.AI;

namespace Abhuman40k;

[HarmonyPatch(typeof(Building_CommsConsole), "GetFloatMenuOptions")]
public class CommsConsoleKinContractsPatch
{
    public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> __result, Pawn myPawn, Building_CommsConsole __instance)
    {
        foreach (var floatMenu in __result)
        {
            yield return floatMenu;
        }
        
        if (!__instance.Powered())
        {
            yield break;
            //contractWindowFloatMenu.Disabled = true;
            //contractWindowFloatMenu.Label = contractWindowFloatMenu.Label + " (" + "CannotUseNoPower".Translate() + ")";
        }
        
        var contractWindowFloatMenu = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("BEWH.Abhuman.Contract.OpenContracts".Translate().CapitalizeFirst(), delegate
        {
            myPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(Abhuman40kDefOf.BEWH_OpenContractWindow, __instance), JobTag.Misc);
        }), myPawn, __instance);
        
        if (!Abhuman40kDefOf.TransportPod.IsFinished)
        {
            contractWindowFloatMenu.Disabled = true;
            contractWindowFloatMenu.Label = contractWindowFloatMenu.Label + " (" + "BEWH.Framework.Common.MissingResearch".Translate(Abhuman40kDefOf.TransportPod.LabelCap) + ")";
        }
        
        yield return contractWindowFloatMenu;
    }
}