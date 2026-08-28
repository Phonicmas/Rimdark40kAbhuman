using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Abhuman40k;

public class LordJob_Beastman : LordJob
{
    private Faction faction;

	private IntVec3 siegeSpot;

	private float blueprintPoints;

	public override bool GuiltyOnDowned => true;

	public LordJob_Beastman()
	{
	}

	public LordJob_Beastman(Faction faction, IntVec3 siegeSpot, float blueprintPoints)
	{
		this.faction = faction;
		this.siegeSpot = siegeSpot;
		this.blueprintPoints = blueprintPoints;
	}

	public override StateGraph CreateGraph()
	{
		var stateGraph = new StateGraph();
		var startingToil = stateGraph.AttachSubgraph(new LordJob_Travel(siegeSpot).CreateGraph()).StartingToil;
		var lordToil_Beastman = new LordToil_Beastman(siegeSpot, blueprintPoints);
		stateGraph.AddToil(lordToil_Beastman);
		var startingToil2 = stateGraph.AttachSubgraph(new LordJob_AssaultColony(faction).CreateGraph()).StartingToil;
		var transition = new Transition(startingToil, lordToil_Beastman);
		transition.AddTrigger(new Trigger_Memo("TravelArrived"));
		transition.AddTrigger(new Trigger_TicksPassed(5000));
		stateGraph.AddTransition(transition);
		// The herd stays at the stone for as long as a shaman still chants. Only the loss of
		// every shaman (signalled by Building_HerdstoneEnemy / LordToil_Beastman) breaks the
		// siege and sends the remaining garrison at the colony. Deliberately no timeout or
		// pawn-harmed trigger here: the stone is meant to be a persistent threat the player
		// has to go and put down.
		var transition2 = new Transition(lordToil_Beastman, startingToil2);
		transition2.AddTrigger(new Trigger_Memo("NoShaman"));
		transition2.AddPreAction(new TransitionAction_Message("BEWH.Abhuman.Beastman.HerdstoneShamansBroken".Translate(faction.def.pawnsPlural, faction), MessageTypeDefOf.ThreatBig));
		transition2.AddPostAction(new TransitionAction_WakeAll());
		stateGraph.AddTransition(transition2);
		return stateGraph;
	}

	public override void ExposeData()
	{
		Scribe_References.Look(ref faction, "faction");
		Scribe_Values.Look(ref siegeSpot, "siegeSpot");
		Scribe_Values.Look(ref blueprintPoints, "blueprintPoints", 0f);
	}
}