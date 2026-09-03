using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Abhuman40k;

/// <summary>
/// The raid herdstone. Once raised by the shamans it continuously births beastmen that stage
/// around the stone. When the staged group reaches critical mass it is released as an assault
/// wave and a new, larger wave starts amassing. Killing shamans shrinks the wave cap and slows
/// the birth rate; killing all of them shuts the stone down entirely.
/// </summary>
public class Building_HerdstoneEnemy : Building
{
    // ----- Tuning -----

    /// <summary>Ticks between births at full shaman strength.</summary>
    private const int BaseSpawnIntervalTicks = 1200;

    /// <summary>Fraction of the original raid points a wave is worth at wave 1.</summary>
    private const float WavePointsFraction = 0.5f;

    private const float MinWavePoints = 100f;

    private const float MaxWavePoints = 5000f;

    /// <summary>How far from the stone newborn beastmen appear.</summary>
    private const int SpawnRadius = 4;

    private const int ShamanCheckInterval = 250;

    private const int FallbackLordSearchInterval = 500;

    /// <summary>Shamans pray in a ring this far out from the stone, at least.</summary>
    private const int PrayerRingRadiusMin = 2;

    /// <summary>...and at most this far out.</summary>
    private const int PrayerRingRadiusMax = 4;

    /// <summary>Chance a newborn is a minotaur rather than a plain beastman, by wave number.</summary>
    private static readonly SimpleCurve MinotaurPercentage =
    [
        new CurvePoint(1, 0f),
        new CurvePoint(3, 0.2f),
        new CurvePoint(5, 0.4f),
        new CurvePoint(8, 0.6f),
        new CurvePoint(10, 1f)
    ];

    /// <summary>Multiplier on the wave point target, by wave number.</summary>
    private static readonly SimpleCurve GroupPointStrength =
    [
        new CurvePoint(1, 1f),
        new CurvePoint(3, 1.2f),
        new CurvePoint(5, 1.4f),
        new CurvePoint(8, 1.6f),
        new CurvePoint(10, 2f)
    ];

    // ----- State -----

    /// <summary>The siege lord that raised this stone. Its surviving shamans power the stone.</summary>
    private Lord shamanLord;

    /// <summary>The lord holding the wave that is currently amassing. Null until the first birth.</summary>
    private Lord waveLord;

    /// <summary>Combat points of the raid that raised the stone, used to size waves.</summary>
    private float basePoints = 500f;

    /// <summary>Shaman count when the stone was raised, used as the denominator for scaling.</summary>
    private int initialShamanCount;

    private int waveNumber = 1;

    private float currentWavePoints;

    private int nextSpawnTick = -1;

    private bool inert;

    /// <summary>The spot each shaman has claimed to chant on, so the four don't stack up.</summary>
    private Dictionary<Pawn, IntVec3> prayerCells = new();

    private List<Pawn> prayerCellsKeysWorking;

    private List<IntVec3> prayerCellsValuesWorking;

    // ----- Derived -----

    /// <summary>Shamans of the raising lord still on their feet.</summary>
    public int AliveShamanCount
    {
        get
        {
            if (shamanLord == null)
            {
                return 0;
            }

            var count = 0;
            foreach (var pawn in shamanLord.ownedPawns)
            {
                if (pawn is { Dead: false, Destroyed: false, Spawned: true, Downed: false }
                    && pawn.kindDef == Abhuman40kDefOf.BEWH_BeastmanFactionBeastmanShaman)
                {
                    count++;
                }
            }

            return count;
        }
    }

    /// <summary>1 while every shaman lives, 0 once they are all gone.</summary>
    private float ShamanFactor => initialShamanCount <= 0
        ? 0f
        : Mathf.Clamp01(AliveShamanCount / (float)initialShamanCount);

    /// <summary>Combat points the amassing wave must reach before it is unleashed.</summary>
    public float WavePointsTarget =>
        Mathf.Clamp(basePoints * WavePointsFraction * GroupPointStrength.Evaluate(waveNumber), MinWavePoints, MaxWavePoints)
        * ShamanFactor;

    private int SpawnIntervalTicks
    {
        get
        {
            var reproductionFactor = Find.Storyteller.difficulty.enemyReproductionRateFactor;
            var scale = ShamanFactor * Mathf.Max(reproductionFactor, 0.01f);
            return Mathf.RoundToInt(BaseSpawnIntervalTicks / Mathf.Max(scale, 0.01f));
        }
    }

    /// <summary>Average combat points a single birth is worth at the current wave number.</summary>
    private float ExpectedPointsPerBirth
    {
        get
        {
            var minotaurChance = Mathf.Clamp01(MinotaurPercentage.Evaluate(waveNumber));
            var minotaur = Abhuman40kDefOf.BEWH_BeastmanFactionMinotaur?.combatPower ?? 80f;
            var beastman = Abhuman40kDefOf.BEWH_BeastmanFactionBeastman?.combatPower ?? 50f;
            return Mathf.Max(minotaurChance * minotaur + (1f - minotaurChance) * beastman, 1f);
        }
    }

    /// <summary>
    /// Estimated ticks until the amassing wave reaches critical mass and charges. The stone
    /// releases on points rather than a timer, so this projects the remaining births forward
    /// at the current birth rate. -1 when the stone isn't running.
    /// </summary>
    public int TicksUntilWave
    {
        get
        {
            if (!Active)
            {
                return -1;
            }

            var remaining = WavePointsTarget - currentWavePoints;
            if (remaining <= 0f)
            {
                return 0;
            }

            var birthsLeft = Mathf.CeilToInt(remaining / ExpectedPointsPerBirth);
            var untilNextBirth = Mathf.Max(nextSpawnTick - Find.TickManager.TicksGame, 0);
            return Mathf.Max((birthsLeft - 1) * SpawnIntervalTicks + untilNextBirth, 0);
        }
    }

    public bool Active => !inert && shamanLord != null && AliveShamanCount > 0;

    // ----- Prayer spots -----

    /// <summary>
    /// The cell this shaman chants on. Assigned once and remembered, so each of the four keeps
    /// his own spot in the ring instead of all of them piling onto the same tile.
    /// </summary>
    public IntVec3 GetPrayerCellFor(Pawn pawn)
    {
        if (pawn == null || !Spawned)
        {
            return IntVec3.Invalid;
        }

        PrunePrayerCells();

        if (prayerCells.TryGetValue(pawn, out var claimed) && PrayerCellValidFor(claimed, pawn))
        {
            return claimed;
        }

        prayerCells.Remove(pawn);

        if (!TryFindPrayerCell(pawn, out var cell))
        {
            return IntVec3.Invalid;
        }

        prayerCells[pawn] = cell;
        return cell;
    }

    /// <summary>Drop claims held by shamans who are dead, gone, or no longer on this map.</summary>
    private void PrunePrayerCells()
    {
        if (prayerCells.Count == 0)
        {
            return;
        }

        var stale = new List<Pawn>();
        foreach (var entry in prayerCells)
        {
            var pawn = entry.Key;
            if (pawn is not { Spawned: true, Dead: false } || pawn.Map != Map)
            {
                stale.Add(pawn);
            }
        }

        foreach (var pawn in stale)
        {
            prayerCells.Remove(pawn);
        }
    }

    private bool PrayerCellValidFor(IntVec3 cell, Pawn pawn)
    {
        return cell.IsValid
               && cell.InBounds(Map)
               && cell.Standable(Map)
               && !this.OccupiedRect().Contains(cell)
               && !cell.IsForbidden(pawn)
               && pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly);
    }

    private bool TryFindPrayerCell(Pawn pawn, out IntVec3 cell)
    {
        var taken = new HashSet<IntVec3>(prayerCells.Values);
        var occupied = this.OccupiedRect();
        var minDistSquared = PrayerRingRadiusMin * PrayerRingRadiusMin;

        return CellFinder.TryFindRandomCellNear(
            Position,
            Map,
            PrayerRingRadiusMax,
            candidate => candidate.InBounds(Map)
                         && candidate.Standable(Map)
                         && !occupied.Contains(candidate)
                         && !taken.Contains(candidate)
                         && (candidate - Position).LengthHorizontalSquared >= minDistSquared
                         && !candidate.IsForbidden(pawn)
                         && pawn.CanReach(candidate, PathEndMode.OnCell, Danger.Deadly),
            out cell);
    }

    // ----- Setup -----

    /// <summary>Called by <see cref="LordToil_Beastman"/> when the shamans finish raising the stone.</summary>
    public void Register(Lord lord, float raidPoints, int shamanCount)
    {
        shamanLord = lord;
        basePoints = raidPoints > 0f ? raidPoints : basePoints;
        initialShamanCount = Mathf.Max(shamanCount, 1);
        nextSpawnTick = Find.TickManager.TicksGame + SpawnIntervalTicks;
    }

    /// <summary>
    /// Save-safety net: if the stone somehow lost its lord (or was spawned by dev tools) find a
    /// beastman siege lord on the map and adopt it.
    /// </summary>
    private void TryFindShamanLord()
    {
        if (Faction == null || Map == null)
        {
            return;
        }

        foreach (var lord in Map.lordManager.lords)
        {
            if (lord.faction != Faction || lord.LordJob is not LordJob_Beastman)
            {
                continue;
            }

            var shamans = 0;
            foreach (var pawn in lord.ownedPawns)
            {
                if (pawn.kindDef == Abhuman40kDefOf.BEWH_BeastmanFactionBeastmanShaman)
                {
                    shamans++;
                }
            }

            if (shamans <= 0)
            {
                continue;
            }

            Register(lord, basePoints, shamans);
            return;
        }
    }

    // ----- Ticking -----

    protected override void TickInterval(int delta)
    {
        base.TickInterval(delta);

        if (!Spawned || Faction == null || Faction.IsPlayer || inert)
        {
            return;
        }

        if (shamanLord == null)
        {
            if (this.IsHashIntervalTick(FallbackLordSearchInterval, delta))
            {
                TryFindShamanLord();
            }

            return;
        }

        if (this.IsHashIntervalTick(ShamanCheckInterval, delta) && AliveShamanCount <= 0)
        {
            GoInert();
            return;
        }

        if (nextSpawnTick < 0)
        {
            nextSpawnTick = Find.TickManager.TicksGame + SpawnIntervalTicks;
            return;
        }

        if (Find.TickManager.TicksGame < nextSpawnTick)
        {
            return;
        }

        TrySpawnWavePawn();
        nextSpawnTick = Find.TickManager.TicksGame + SpawnIntervalTicks;
    }

    // ----- Wave handling -----

    private PawnKindDef NextPawnKind()
    {
        return Rand.Chance(MinotaurPercentage.Evaluate(waveNumber))
            ? Abhuman40kDefOf.BEWH_BeastmanFactionMinotaur
            : Abhuman40kDefOf.BEWH_BeastmanFactionBeastman;
    }

    private void TrySpawnWavePawn()
    {
        var kind = NextPawnKind();
        if (kind == null)
        {
            return;
        }

        if (!CellFinder.TryRandomClosewalkCellNear(Position, Map, SpawnRadius, out var cell))
        {
            return;
        }

        var request = new PawnGenerationRequest(
            kind,
            Faction,
            PawnGenerationContext.NonPlayer,
            Map.Tile,
            forceGenerateNewPawn: true,
            mustBeCapableOfViolence: true,
            forcedXenotype: XenotypeFor(kind));

        var pawn = PawnGenerator.GeneratePawn(request);
        if (pawn == null)
        {
            return;
        }

        GenSpawn.Spawn(pawn, cell, Map);
        EnsureWaveLord().AddPawn(pawn);

        currentWavePoints += kind.combatPower;

        if (currentWavePoints >= WavePointsTarget)
        {
            ReleaseWave();
        }
    }

    /// <summary>
    /// The xenotype the pawn kind asks for, forced onto the generation request.
    /// PawnGenerator.GetXenotypeForGeneratedPawn drops every xenotype whose
    /// canGenerateAsCombatant is false from the roll when the request is flagged
    /// mustBeCapableOfViolence, and BEWH_Beastman (plus BEWH_Minotaur, which inherits it) sets
    /// that to false, so left alone the roll empties out and we'd get plain baselines. Vanilla
    /// dodges this the same way: PawnGroupKindWorker_Normal picks the xenotype up front and
    /// passes it as the forced one, which returns before the combatant filter.
    /// </summary>
    private XenotypeDef XenotypeFor(PawnKindDef kind)
    {
        if (!ModsConfig.BiotechActive || kind == null)
        {
            return null;
        }

        var available = PawnGenerator.XenotypesAvailableFor(kind, null, Faction);
        return available.TryRandomElementByWeight(pair => pair.Value, out var chosen) ? chosen.Key : null;
    }

    private Lord EnsureWaveLord()
    {
        if (waveLord != null && Map.lordManager.lords.Contains(waveLord))
        {
            return waveLord;
        }

        waveLord = LordMaker.MakeNewLord(Faction, new LordJob_BeastmanHerdstoneWave(Faction, Position), Map);
        return waveLord;
    }

    /// <summary>Sends the currently amassed group at the colony and starts a fresh, larger wave.</summary>
    public void ReleaseWave()
    {
        if (waveLord != null && Map != null && Map.lordManager.lords.Contains(waveLord))
        {
            waveLord.ReceiveMemo(LordJob_BeastmanHerdstoneWave.ReleaseMemo);
        }

        waveLord = null;
        currentWavePoints = 0f;
        waveNumber++;
    }

    /// <summary>All shamans are down: the stone goes dark and everyone left charges the colony.</summary>
    private void GoInert()
    {
        if (inert)
        {
            return;
        }

        inert = true;
        ReleaseWave();

        if (shamanLord != null && Map != null && Map.lordManager.lords.Contains(shamanLord))
        {
            shamanLord.ReceiveMemo("NoShaman");
        }

        Messages.Message("BEWH.Abhuman.Beastman.HerdstoneWentDark".Translate(), this, MessageTypeDefOf.PositiveEvent);
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        // Don't strand a half-grown wave staging around a stone that no longer exists.
        ReleaseWave();
        prayerCells.Clear();
        base.DeSpawn(mode);
    }

    // ----- UI -----

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        stringBuilder.AppendLineIfNotEmpty();

        if (Faction is { IsPlayer: true })
        {
            return stringBuilder.ToString().TrimEndNewlines();
        }

        if (inert || (shamanLord != null && AliveShamanCount <= 0))
        {
            stringBuilder.AppendTagged("BEWH.Abhuman.Beastman.HerdstoneDormant".Translate());
            return stringBuilder.ToString().TrimEndNewlines();
        }

        if (shamanLord == null)
        {
            stringBuilder.AppendTagged("BEWH.Abhuman.Beastman.HerdstoneUnbound".Translate());
            return stringBuilder.ToString().TrimEndNewlines();
        }

        stringBuilder.AppendTagged("BEWH.Abhuman.Beastman.HerdstoneWave".Translate(
            waveNumber,
            Mathf.RoundToInt(currentWavePoints),
            Mathf.RoundToInt(WavePointsTarget)));
        stringBuilder.AppendLine();
        stringBuilder.AppendTagged("BEWH.Abhuman.Beastman.HerdstoneShamansAlive".Translate(
            AliveShamanCount, initialShamanCount));

        var ticksUntilWave = TicksUntilWave;
        if (ticksUntilWave >= 0)
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendTagged("BEWH.Abhuman.Beastman.HerdstoneNextWave".Translate()
                                       + ": " + PeriodString(ticksUntilWave));
        }

        if (DebugSettings.ShowDevGizmos)
        {
            var ticksUntilBirth = Mathf.Max(nextSpawnTick - Find.TickManager.TicksGame, 0);
            stringBuilder.AppendLine();
            stringBuilder.AppendTagged("BEWH.Abhuman.Beastman.HerdstoneNextBirth".Translate()
                                       + ": " + PeriodString(ticksUntilBirth));
        }

        return stringBuilder.ToString().TrimEndNewlines();
    }

    private static string PeriodString(int ticks)
    {
        var text = ticks > 60000
            ? ticks.ToStringTicksToDays()
            : ticks.ToStringTicksToPeriod(allowYears: false);
        return text.Colorize(ColoredText.DateTimeColor);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        if (!DebugSettings.ShowDevGizmos)
        {
            yield break;
        }

        yield return new Command_Action
        {
            defaultLabel = "DEV: Birth beastman",
            action = delegate
            {
                if (shamanLord == null)
                {
                    TryFindShamanLord();
                }

                TrySpawnWavePawn();
            }
        };

        yield return new Command_Action
        {
            defaultLabel = "DEV: Release wave now",
            action = ReleaseWave
        };

        yield return new Command_Action
        {
            defaultLabel = "DEV: Fill wave to critical mass",
            action = delegate
            {
                if (shamanLord == null)
                {
                    TryFindShamanLord();
                }

                // TrySpawnWavePawn releases (and resets) the wave the moment it hits the
                // target, so watch waveNumber rather than looping on the point total.
                var startWave = waveNumber;
                var guard = 0;
                while (waveNumber == startWave && guard++ < 200)
                {
                    TrySpawnWavePawn();
                }
            }
        };
    }

    public override void ExposeData()
    {
        base.ExposeData();

        // Lords are dropped from the LordManager once they run out of pawns, and unlike Things
        // a stale Lord reference is not silently nulled. Prune before writing so we never save
        // a reference to a lord that isn't deep-saved.
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            var lordManager = MapHeld?.lordManager;
            if (lordManager != null)
            {
                if (shamanLord != null && !lordManager.lords.Contains(shamanLord))
                {
                    shamanLord = null;
                }

                if (waveLord != null && !lordManager.lords.Contains(waveLord))
                {
                    waveLord = null;
                }
            }
        }

        Scribe_References.Look(ref shamanLord, "shamanLord");
        Scribe_References.Look(ref waveLord, "waveLord");
        Scribe_Values.Look(ref basePoints, "basePoints", 500f);
        Scribe_Values.Look(ref initialShamanCount, "initialShamanCount");
        Scribe_Values.Look(ref waveNumber, "waveNumber", 1);
        Scribe_Values.Look(ref currentWavePoints, "currentWavePoints");
        Scribe_Values.Look(ref nextSpawnTick, "nextSpawnTick", -1);
        Scribe_Values.Look(ref inert, "inert");

        if (Scribe.mode == LoadSaveMode.Saving)
        {
            PrunePrayerCells();
        }

        Scribe_Collections.Look(ref prayerCells, "prayerCells", LookMode.Reference, LookMode.Value,
            ref prayerCellsKeysWorking, ref prayerCellsValuesWorking);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            prayerCells ??= new Dictionary<Pawn, IntVec3>();
            var nullKeys = new List<Pawn>();
            foreach (var entry in prayerCells)
            {
                if (entry.Key == null)
                {
                    nullKeys.Add(entry.Key);
                }
            }

            foreach (var key in nullKeys)
            {
                prayerCells.Remove(key);
            }
        }
    }
}
