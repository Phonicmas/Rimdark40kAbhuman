using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Abhuman40k;

// This postfix runs for every memory thought of every pawn on every mood recalc, so the doubled
// thoughts are resolved once at startup and the common case costs a single dictionary lookup.
[HarmonyPatch(typeof(Thought_Memory), "MoodOffset")]
[StaticConstructorOnStartup]
public class GregariousPatch
{
    private static readonly Dictionary<ThoughtDef, List<GeneDef>> doublingGenes = new Dictionary<ThoughtDef, List<GeneDef>>();

    static GregariousPatch()
    {
        foreach (var geneDef in DefDatabase<GeneDef>.AllDefsListForReading)
        {
            var modExtension = geneDef.GetModExtension<DefModExtension_Gregarious>();
            if (modExtension?.thoughtEffectsDoubled == null)
            {
                continue;
            }

            foreach (var thoughtDef in modExtension.thoughtEffectsDoubled)
            {
                if (thoughtDef == null)
                {
                    continue;
                }

                if (!doublingGenes.TryGetValue(thoughtDef, out var genes))
                {
                    genes = new List<GeneDef>();
                    doublingGenes[thoughtDef] = genes;
                }

                genes.Add(geneDef);
            }
        }
    }

    public static void Postfix(ref float __result, Thought_Memory __instance)
    {
        if (doublingGenes.Count == 0 || !doublingGenes.TryGetValue(__instance.def, out var genes))
        {
            return;
        }

        var geneTracker = __instance.pawn?.genes;
        if (geneTracker == null)
        {
            return;
        }

        foreach (var geneDef in genes)
        {
            if (geneTracker.HasActiveGene(geneDef))
            {
                __result *= 2;
                return;
            }
        }
    }
}