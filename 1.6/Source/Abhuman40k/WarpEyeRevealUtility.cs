using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Abhuman40k;

//Backs HarmonyPatch_WarpEyeReveal. Answers "is some pawn with a revealing gene close enough to drag this
//one back into perception", which the patch feeds into HediffComp_Invisibility.ForcedVisible.
[StaticConstructorOnStartup]
public static class WarpEyeRevealUtility
{
    private const int RefreshIntervalTicks = 30;

    private static readonly List<GeneDef> revealerGenes = new List<GeneDef>();
    private static readonly Dictionary<Map, List<Pawn>> seersByMap = new Dictionary<Map, List<Pawn>>();
    private static int cachedBucket = -1;

    static WarpEyeRevealUtility()
    {
        foreach (var geneDef in DefDatabase<GeneDef>.AllDefsListForReading)
        {
            if (geneDef.HasModExtension<DefModExtension_RevealsInvisible>())
            {
                revealerGenes.Add(geneDef);
            }
        }
    }

    public static bool IsRevealed(Pawn target)
    {
        if (revealerGenes.Count == 0 || target == null || !target.Spawned)
        {
            return false;
        }

        var map = target.Map;
        var tickManager = Find.TickManager;
        if (map == null || tickManager == null)
        {
            return false;
        }

        var bucket = tickManager.TicksGame / RefreshIntervalTicks;
        if (bucket != cachedBucket)
        {
            cachedBucket = bucket;
            RebuildSeers();
        }

        if (!seersByMap.TryGetValue(map, out var seers))
        {
            return false;
        }

        var targetPosition = target.Position;
        foreach (var seer in seers)
        {
            //The seer list is up to RefreshIntervalTicks stale, so it can hold pawns that have
            //since despawned or moved to another map.
            if (seer == target || !seer.Spawned || seer.Map != map)
            {
                continue;
            }

            var modExtension = ActiveModExtensionFor(seer);
            if (modExtension == null)
            {
                continue;
            }

            //Faction level check on purpose. Thing.HostileTo runs through IsPsychologicallyInvisible,
            //which comes straight back into the patch that called us and blows the stack.
            if (modExtension.hostileOnly && !target.Faction.HostileTo(seer.Faction))
            {
                continue;
            }

            if (!targetPosition.InHorDistOf(seer.Position, RadiusFor(seer, modExtension)))
            {
                continue;
            }

            if (modExtension.requiresLineOfSight && !GenSight.LineOfSight(seer.Position, targetPosition, map, true))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static float RadiusFor(Pawn seer, DefModExtension_RevealsInvisible modExtension)
    {
        if (modExtension.psychicSensitivityFactor <= 0f)
        {
            return modExtension.radius;
        }

        var psychicSensitivity = seer.GetStatValue(StatDefOf.PsychicSensitivity);
        return Mathf.Max(0f, modExtension.radius * (1f + ((psychicSensitivity - 1f) * modExtension.psychicSensitivityFactor)));
    }

    private static DefModExtension_RevealsInvisible ActiveModExtensionFor(Pawn seer)
    {
        if (seer.genes == null || !seer.Spawned || seer.Dead || seer.Downed || !seer.Awake())
        {
            return null;
        }

        foreach (var geneDef in revealerGenes)
        {
            if (!seer.genes.HasActiveGene(geneDef))
            {
                continue;
            }

            var modExtension = geneDef.GetModExtension<DefModExtension_RevealsInvisible>();
            if (BlockedByApparel(seer, modExtension.blockedIfCovered))
            {
                continue;
            }

            return modExtension;
        }

        return null;
    }

    private static bool BlockedByApparel(Pawn seer, List<BodyPartGroupDef> bodyPartGroups)
    {
        if (bodyPartGroups.NullOrEmpty())
        {
            return false;
        }

        var wornApparel = seer.apparel?.WornApparel;
        if (wornApparel == null)
        {
            return false;
        }

        foreach (var apparel in wornApparel)
        {
            var coveredGroups = apparel.def.apparel?.bodyPartGroups;
            if (coveredGroups == null)
            {
                continue;
            }

            foreach (var bodyPartGroup in coveredGroups)
            {
                if (bodyPartGroups.Contains(bodyPartGroup))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void RebuildSeers()
    {
        seersByMap.Clear();

        var maps = Find.Maps;
        if (maps == null)
        {
            return;
        }

        foreach (var map in maps)
        {
            List<Pawn> seers = null;
            foreach (var pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (ActiveModExtensionFor(pawn) == null)
                {
                    continue;
                }

                seers ??= new List<Pawn>();
                seers.Add(pawn);
            }

            if (seers != null)
            {
                seersByMap[map] = seers;
            }
        }
    }
}
