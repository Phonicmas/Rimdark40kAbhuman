using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Abhuman40k;

public class LordToil_Beastman : LordToil
{
	private const float BaseRadiusMin = 14f;

	private const float BaseRadiusMax = 25f;

	private const int StartBuildingDelay = 50;
	
	private const int WantedShamans = 4;

	/// <summary>
	/// How far a shaman may stray from the herdstone. JobGiver_AIDefendPoint takes this from the
	/// duty, so it doubles as their target-acquire range: roughly bow reach, and they will never
	/// chase past it.
	/// </summary>
	private const float ShamanDefendRadius = 24f;

	/// <summary>Shamans whose work settings have already been locked to construction.</summary>
	private readonly HashSet<Pawn> shamanWorkConfigured = [];

	public override IntVec3 FlagLoc => Data.siegeCenter;

	private LordToilData_SiegeBeastman Data => (LordToilData_SiegeBeastman)data;

	private IEnumerable<Frame> Frames
	{
		get
		{
			var data = Data;
			var radSquared = (data.baseRadius + 10f) * (data.baseRadius + 10f);
			var framesList = Map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame);
			if (framesList.Count == 0)
			{
				yield break;
			}
			for (var i = 0; i < framesList.Count; i++)
			{
				var frame = (Frame)framesList[i];
				if (frame.Faction == lord.faction && (float)(frame.Position - data.siegeCenter).LengthHorizontalSquared < radSquared)
				{
					yield return frame;
				}
			}
		}
	}

	public override bool ForceHighStoryDanger => true;

	public LordToil_Beastman(IntVec3 siegeCenter, float blueprintPoints)
	{
		data = new LordToilData_SiegeBeastman();
		Data.siegeCenter = siegeCenter;
		Data.blueprintPoints = blueprintPoints;
	}

	public override void Init()
	{
		base.Init();
		Data.baseRadius = Mathf.InverseLerp(BaseRadiusMin, BaseRadiusMax, lord.ownedPawns.Count / 50f);
		Data.baseRadius = Mathf.Clamp(Data.baseRadius, BaseRadiusMin, BaseRadiusMax);
		var costList = new List<Thing>();
		var placedBlueprint = BeastmanSiegeUtility.PlaceBlueprint(Data, Map, lord.faction);
		if (placedBlueprint == null)
		{
			Log.Warning("[RimDark Abhumans] Could not place a herdstone blueprint for the beastman siege; the raid will fall through to assaulting.");
			return;
		}

		Data.blueprints.Add(placedBlueprint);
		foreach (var cost in placedBlueprint.TotalMaterialCost())
		{
			var thing = costList.FirstOrDefault(t => t.def == cost.thingDef);
			if (thing != null)
			{
				thing.stackCount += cost.count;
				continue;
			}
			var thing2 = ThingMaker.MakeThing(cost.thingDef);
			thing2.stackCount = cost.count;
			costList.Add(thing2);
		}
		
		foreach (var costThing in costList)
		{
			costThing.stackCount = Mathf.CeilToInt(costThing.stackCount * Rand.Range(1f, 1.2f));
		}
		
		var list2 = new List<List<Thing>>();
		for (var j = 0; j < costList.Count; j++)
		{
			while (costList[j].stackCount > costList[j].def.stackLimit)
			{
				var num = Mathf.CeilToInt(costList[j].def.stackLimit * Rand.Range(0.9f, 0.999f));
				var thing4 = ThingMaker.MakeThing(costList[j].def);
				thing4.stackCount = num;
				costList[j].stackCount -= num;
				costList.Add(thing4);
			}
		}
		
		var list3 = new List<Thing>();
		for (var k = 0; k < costList.Count; k++)
		{
			list3.Add(costList[k]);
			if (k % 2 != 1 && k != costList.Count - 1)
			{
				continue;
			}
			
			list2.Add(list3);
			list3 = [];
		}
		
		var list4 = new List<Thing>();
		list2.Add(list4);
		
		foreach (var group in list2)
		{
			if (!DropCellFinder.TryFindDropSpotNear(Data.siegeCenter, Map, out var pos, allowFogged: false, canRoofPunch: false))
			{
				continue;
			}
			foreach (var thing5 in group)
			{
				thing5.SetForbidden(value: true, warnOnFail: false);
				GenPlace.TryPlaceThing(thing5, pos, Map, ThingPlaceMode.Near);
			}
		}
	}

	public override void UpdateAllDuties()
	{
		if (lord.ticksInToil < StartBuildingDelay)
		{
			foreach (var t in lord.ownedPawns)
			{
				SetAsDefender(t);
			}

			return;
		}

		// Shamans are identified by pawn kind, and there are only ever WantedShamans of them in
		// the group, so every shaman-kind pawn chants and everyone else guards the site.
		var shamanAmount = 0;
		foreach (var pawn in lord.ownedPawns)
		{
			if (shamanAmount < WantedShamans && CanBeShaman(pawn))
			{
				SetAsShaman(pawn);
				shamanAmount++;
				continue;
			}

			SetAsDefender(pawn);
		}

		if (shamanAmount == 0)
		{
			lord.ReceiveMemo("NoShaman");
		}
	}

	public override void Notify_PawnLost(Pawn victim, PawnLostCondition cond)
	{
		UpdateAllDuties();
		base.Notify_PawnLost(victim, cond);
	}

	public override void Notify_ConstructionFailed(Pawn pawn, Frame frame, Blueprint_Build newBlueprint)
	{
		base.Notify_ConstructionFailed(pawn, frame, newBlueprint);
		if (frame.Faction == lord.faction && newBlueprint != null)
		{
			Data.blueprints.Add(newBlueprint);
		}
	}

	public override void Notify_ConstructionCompleted(Pawn pawn, Building building)
	{
		base.Notify_ConstructionCompleted(pawn, building);
		if (building is not Building_HerdstoneEnemy raidHerdstone || building.Faction != lord.faction)
		{
			return;
		}

		// Frame.CompleteConstruction already ran lord.AddBuilding for us, don't add it twice.
		Data.herdstone = raidHerdstone;
		raidHerdstone.Register(lord, Data.blueprintPoints, ShamanCount);
	}

	/// <summary>Shamans of this lord still on their feet.</summary>
	public int ShamanCount
	{
		get
		{
			var count = 0;
			foreach (var pawn in lord.ownedPawns)
			{
				if (CanBeShaman(pawn) && pawn is { Dead: false, Downed: false })
				{
					count++;
				}
			}

			return count;
		}
	}

	private static bool CanBeShaman(Pawn p)
	{
		return p.kindDef == Abhuman40kDefOf.BEWH_BeastmanFactionBeastmanShaman;
	}

	private void SetAsShaman(Pawn p)
	{
		var data = Data;

		// Anchor on the stone itself once it is standing, so the prayer ring and the leash agree
		// even when the blueprint got placed off to one side of the siege spot.
		var focus = data.herdstone is { Spawned: true } ? data.herdstone.Position : data.siegeCenter;

		p.mindState.duty = new PawnDuty(Abhuman40kDefOf.BEWH_BestmanShamanChant, focus)
		{
			radius = ShamanDefendRadius
		};

		// The duty is refreshed every pass because the focus moves once the stone is up, but the
		// work settings only ever need doing once per shaman.
		if (!shamanWorkConfigured.Add(p))
		{
			return;
		}

		p.skills.GetSkill(SkillDefOf.Construction).EnsureMinLevelWithMargin(5);
		p.workSettings.EnableAndInitialize();
		var allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
		foreach (var workTypeDef in allDefsListForReading)
		{
			if (workTypeDef == WorkTypeDefOf.Construction)
			{
				p.workSettings.SetPriority(workTypeDef, 1);
			}
			else
			{
				// Without this they'd happily wander off hauling and cleaning inside the flag
				// radius instead of raising the stone. Vanilla LordToil_Siege does the same.
				p.workSettings.Disable(workTypeDef);
			}
		}
		
	}

	private void SetAsDefender(Pawn p)
	{
		var data = Data;
		p.mindState.duty = new PawnDuty(DutyDefOf.Defend, data.siegeCenter)
		{
			radius = data.baseRadius
		};
	}

	public override void LordToilTick()
	{
		base.LordToilTick();
		if (lord.ticksInToil == StartBuildingDelay)
		{
			lord.CurLordToil.UpdateAllDuties();
		}
		if (lord.ticksInToil > StartBuildingDelay && lord.ticksInToil % 500 == 0)
		{
			UpdateAllDuties();
		}
		if (Find.TickManager.TicksGame % 500 != 0)
		{
			return;
		}

		// Once the stone is up, the shamans are the only thing keeping the siege going, and the
		// herdstone itself sends "NoShaman" when the last of them falls. Here we only cover the
		// two cases the stone can't report: the stone was destroyed out from under a living
		// shaman, and the shamans died before the stone was ever finished.
		if (Data.herdstone != null)
		{
			if (!Data.herdstone.Spawned)
			{
				lord.ReceiveMemo("NoShaman");
			}

			return;
		}

		// Init never got a blueprint down, so there is nothing here for the shamans to raise.
		if (Data.blueprints.Count == 0)
		{
			lord.ReceiveMemo("NoShaman");
			return;
		}

		if (ShamanCount == 0)
		{
			lord.ReceiveMemo("NoShaman");
		}
	}

	public override void Cleanup()
	{
		shamanWorkConfigured.Clear();
		var data = Data;
		data.blueprints.RemoveAll(blue => blue.Destroyed);
		foreach (var t in data.blueprints)
		{
			t.Destroy(DestroyMode.Cancel);
		}
		var frameList = Frames.ToList();
		foreach (var frame in frameList)
		{
			frame.Destroy(DestroyMode.Cancel);
		}
	}
}
