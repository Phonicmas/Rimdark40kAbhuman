using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Abhuman40k;

/// <summary>
/// Lord job for a wave of beastmen currently amassing around a raid herdstone.
/// The pawns stage at the stone until <see cref="ReleaseMemo"/> arrives (sent by
/// <see cref="Building_HerdstoneEnemy"/> once the wave has hit critical mass), at which
/// point the whole lord flips over to assaulting the colony.
/// </summary>
public class LordJob_BeastmanHerdstoneWave : LordJob
{
	public const string ReleaseMemo = "BeastmanHerdstoneWaveRelease";

	private Faction faction;

	private IntVec3 herdstoneSpot;

	public override bool GuiltyOnDowned => true;

	// Herdstone-born beastmen never rout, they are spent freely by the shamans.
	public override bool AddFleeToil => false;

	public LordJob_BeastmanHerdstoneWave()
	{
	}

	public LordJob_BeastmanHerdstoneWave(Faction faction, IntVec3 herdstoneSpot)
	{
		this.faction = faction;
		this.herdstoneSpot = herdstoneSpot;
	}

	public override StateGraph CreateGraph()
	{
		var stateGraph = new StateGraph();

		var stageToil = (LordToil_Stage)(stateGraph.StartingToil = new LordToil_Stage(herdstoneSpot));

		var assaultToil = stateGraph.AttachSubgraph(new LordJob_AssaultColony(
			faction,
			canKidnap: false,
			canTimeoutOrFlee: false,
			sappers: false,
			useAvoidGridSmart: false,
			canSteal: false).CreateGraph()).StartingToil;

		var transition = new Transition(stageToil, assaultToil);
		// The herdstone is the only thing that unleashes a wave. Deliberately no
		// Trigger_FractionPawnsLost here: the lord gains pawns one birth at a time, so any
		// fraction trigger fires on the first casualty while the wave is still 2-3 strong.
		// A poked wave defends itself via LordToil_Stage's Defend duty instead, and the stone
		// always releases it eventually (critical mass, last shaman down, or stone destroyed).
		transition.AddTrigger(new Trigger_Memo(ReleaseMemo));
		transition.AddPreAction(new TransitionAction_Message(
			"BEWH.Abhuman.Beastman.HerdstoneWaveAttacks".Translate(faction.def.pawnsPlural.CapitalizeFirst(), faction.Name),
			MessageTypeDefOf.ThreatBig));
		transition.AddPostAction(new TransitionAction_WakeAll());
		stateGraph.AddTransition(transition);

		// Keep the "stopped being hostile" escape hatch reachable from the staging toil too.
		var nonHostileTransition = stateGraph.transitions.Find(x => x.triggers.Any(y => y is Trigger_BecameNonHostileToPlayer));
		nonHostileTransition?.AddSource(stageToil);

		return stateGraph;
	}

	public override void ExposeData()
	{
		Scribe_References.Look(ref faction, "faction");
		Scribe_Values.Look(ref herdstoneSpot, "herdstoneSpot");
	}
}
