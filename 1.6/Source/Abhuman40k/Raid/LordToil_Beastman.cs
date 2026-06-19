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
	
	public Dictionary<Pawn, DutyDef> rememberedDuties = new();

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
		rememberedDuties.Clear();
		var wantedShamans = 4;
		
		var shamanAmount = 0;
		foreach (var pawn in lord.ownedPawns)
		{
			if (pawn.mindState.duty.def != DutyDefOf.Build)
			{
				continue;
			}
			
			rememberedDuties.Add(pawn, DutyDefOf.Build);
			SetAsShaman(pawn);
			shamanAmount++;
		}
		var shamansToGet = wantedShamans - shamanAmount;
		for (var k = 0; k < shamansToGet; k++)
		{
			if (!lord.ownedPawns.Where(pa => !rememberedDuties.ContainsKey(pa) && CanBeShaman(pa)).TryRandomElement(out var pawn2))
			{
				continue;
			}
			rememberedDuties.Add(pawn2, DutyDefOf.Build);
			SetAsShaman(pawn2);
			shamanAmount++;
		}
		for (var l = 0; l < lord.ownedPawns.Count; l++)
		{
			var pawn3 = lord.ownedPawns[l];
			if (!rememberedDuties.ContainsKey(pawn3))
			{
				SetAsDefender(pawn3);
				rememberedDuties.Add(pawn3, DutyDefOf.Defend);
			}
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

	private static bool CanBeShaman(Pawn p)
	{
		return p.kindDef == Abhuman40kDefOf.BEWH_BeastmanFactionBeastmanShaman;
	}

	private void SetAsShaman(Pawn p)
	{
		var data = Data;
		p.mindState.duty = new PawnDuty(Abhuman40kDefOf.BEWH_BestmanShamanChant, data.siegeCenter)
		{
			radius = data.baseRadius
		};
		p.skills.GetSkill(SkillDefOf.Construction).EnsureMinLevelWithMargin(5);
		p.workSettings.EnableAndInitialize();
		var allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
		foreach (var workTypeDef in allDefsListForReading)
		{
			if (workTypeDef == WorkTypeDefOf.Construction)
			{
				p.workSettings.SetPriority(workTypeDef, 1);
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
		//Tick here
		if (false)
		{
			//TODO: If no shamans left, herdstone stop working and everyone attack player
			lord.ReceiveMemo("NoShaman");
		}
	}

	public override void Cleanup()
	{
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
