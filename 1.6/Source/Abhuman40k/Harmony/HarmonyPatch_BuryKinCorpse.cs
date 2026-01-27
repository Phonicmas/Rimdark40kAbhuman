using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Abhuman40k;

[HarmonyPatch(typeof(ThingWithComps), "GetFloatMenuOptions")]
public class BuryKinCorpsePatch
{
    public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> __result, ThingWithComps __instance, Pawn selPawn)
    {
        foreach (var floatMenu in __result)
        {
            yield return floatMenu;
        }
        
        if (__instance is not Corpse corpse)
        {
            yield break;
        }

        var map = __instance.Map;
        var ancestorCore = map.listerThings.ThingsOfDef(Abhuman40kDefOf.BEWH_AncestorCore).FirstOrFallback();
        if (ancestorCore == null)
        {
            yield break;
        }
        
        var carryCorpseToAncestorCore = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("BEWH.Abhuman.Kin.UploadToAncestorCore".Translate(corpse.InnerPawn).CapitalizeFirst(), delegate
        {
            selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(Abhuman40kDefOf.BEWH_CarryKinToAncestorCore, ancestorCore, corpse), JobTag.Misc);
        }), selPawn, __instance);
        
        yield return carryCorpseToAncestorCore;
    }
}