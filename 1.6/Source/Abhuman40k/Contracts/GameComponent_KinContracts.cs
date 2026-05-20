using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Abhuman40k;

public class GameComponent_KinContracts : GameComponent
{
    private List<KinContract> kinContracts = [];
    
    public List<KinContract> KinContracts => kinContracts;
    
    private Dictionary<StuffCategoryDef, List<ThingDef>> possibleItemsToGive;
    public Dictionary<StuffCategoryDef, List<ThingDef>> PossibleItemsToGive => possibleItemsToGive;

    private LinkedList<StuffCategoryDef> stuffWheel = [];

    private int refreshTick = 0;
    
    private bool setupComplete = false;

    public int CurrentMaxContractAmount => (int)Math.Floor(contractMaxAmountByPlayerDebt.Evaluate(playerDebt));
    public int CurrentMaxContractAmountForView => Math.Min(CurrentMaxContractAmount, kinContracts.Count);
    
    private readonly SimpleCurve contractMaxAmountByPlayerDebt =
    [
        new CurvePoint(1, 0),
        new CurvePoint(0, 3),
        new CurvePoint(-1000, 5),
        new CurvePoint(-5000, 7),
        new CurvePoint(-10000, 10)
    ];
    
    public float CurrentContractPriceMultAmount => contractPriceMultByPlayerDebt.Evaluate(playerDebt);
    
    private readonly SimpleCurve contractPriceMultByPlayerDebt =
    [
        new CurvePoint(1, 0.45f),
        new CurvePoint(0, 0.75f),
        new CurvePoint(-1000, 0.9f),
        new CurvePoint(-5000, 1f),
        new CurvePoint(-10000, 1.35f)
    ];
    
    public float CurrentGoodwillByPlayerDebt => (float)Math.Floor(goodwillByPlayerDebt.Evaluate(playerDebt));
    
    private readonly SimpleCurve goodwillByPlayerDebt =
    [
        new CurvePoint(1, -100),
        new CurvePoint(0, 0),
        new CurvePoint(-10000, 100)
    ];
    
    private readonly int minimumMarketValue = 500; //To Modsettings

    //Positive is debt, negative is good
    public float playerDebt = 0;
    
    public bool PlayerInDebt => playerDebt > 0;

    private bool PaymentWasNotReceived = false;

    public Faction KinFaction => Find.FactionManager?.FirstFactionOfDef(Abhuman40kDefOf.BEWH_OffworldKinFaction);

    private readonly int RefreshEveryGivenTick = 300000; //To Modsettings

    public string RefreshIn => (RefreshEveryGivenTick - refreshTick).ToStringTicksToPeriod();

    public bool CanRefresh = false;
    
    public GameComponent_KinContracts(Game game)
    {
        CalculateStuff();
    }

    private void CalculateStuff()
    {
        possibleItemsToGive = new Dictionary<StuffCategoryDef, List<ThingDef>>();
        stuffWheel = [];
        
        var stuffThings = DefDatabase<ThingDef>.AllDefs.Where(def => def.IsStuff).ToList();
        foreach (var stuff in stuffThings)
        {
            if (stuff.stuffProps.categories.NullOrEmpty())
            {
                continue;
            }
            foreach (var stuffCategory in stuff.stuffProps.categories)
            {
                if (!possibleItemsToGive.ContainsKey(stuffCategory))
                {
                    possibleItemsToGive.Add(stuffCategory, [stuff]);
                    stuffWheel.AddLast(stuffCategory);
                }
                else
                {
                    possibleItemsToGive[stuffCategory].Add(stuff);
                }
            }
        }
    }

    public override void GameComponentTick()
    {
        if (!Abhuman40kDefOf.TransportPod.IsFinished)
        {
            return;
        }

        if (!setupComplete)
        {
            TryMakeNewContracts();
            setupComplete = true;
        }
        
        if (refreshTick >= RefreshEveryGivenTick)
        {
            CanRefresh = true;
        }

        refreshTick++;
    }

    public void RefreshContracts()
    {
        CanRefresh = false;
        refreshTick = 0;
        TryMakeNewContracts();
    }

    private void TryMakeNewContracts()
    {
        var inProgress = GetKinContractsInProgressCount();
        if (inProgress.Count > CurrentMaxContractAmount)
        {
            kinContracts = inProgress;
            return;
        }

        if (inProgress.Count < CurrentMaxContractAmount)
        {
            kinContracts.RemoveAll(contract => !inProgress.Contains(contract));
            var diff = CurrentMaxContractAmount - inProgress.Count;
            for (var i = 0; i < diff; i++)
            {
                var newContract = new KinContract();
                kinContracts.Add(newContract);
            }
        }
        
        foreach (var kinContract in kinContracts)
        {
            RefreshContracts(kinContract);
            if (PaymentWasNotReceived)
            {
                break;
            }
        }
        
        kinContracts.RemoveWhere(contract => contract.Invalid);

        if (PaymentWasNotReceived)
        {
            kinContracts = [];
            PaymentWasNotReceived = false;
        }
    }

    private List<KinContract> GetKinContractsInProgressCount()
    {
        return kinContracts.Where(contract => contract.ItemSentToPlayer && !contract.ItemReceivedFromPlayer && Find.TickManager.TicksGame < contract.expiresOn).ToList();
    }

    private void RefreshContracts(KinContract kinContract)
    {
        if (kinContract.ItemSentToPlayer && !kinContract.ItemReceivedFromPlayer)
        {
            if (Find.TickManager.TicksGame >= kinContract.expiresOn)
            {
                PaymentNotReceived(kinContract);
            }
            else
            {
                return;
            }
        }
        
        var map = Find.RandomPlayerHomeMap;
        var itemRequested = FindThingOnMapToRequest(map);
        if (itemRequested == null)
        {
            kinContract.InvalidateContract();
            return;
        }
        
        var itemsGiven = FindThingsToSend(itemRequested);
        kinContract.RefreshContract(itemsGiven, itemRequested, map);
    }
    
    private void PaymentNotReceived(KinContract kinContract)
    {
        PaymentWasNotReceived = true;
        
        KinFaction.SetRelationDirect(Find.FactionManager.OfPlayer, FactionRelationKind.Hostile, true, "BEWH.Abhuman.Contract.ItemNotReceived".Translate());
        KinFaction.ChangeGoodwill_Debug(Find.FactionManager.OfPlayer, KinFaction.GoodwillToMakeHostile(Find.FactionManager.OfPlayer));
        playerDebt = 0 + kinContract.givenItem.BaseMarketValue * 1.2f;
        
        var storytellerComp = Find.Storyteller.storytellerComps.First(x => x is StorytellerComp_OnOffCycle or StorytellerComp_RandomMain);
        var parms = storytellerComp.GenerateParms(IncidentCategoryDefOf.ThreatBig, Find.CurrentMap);
        parms.forced = true;
        parms.faction = KinFaction;
        parms.canSteal = true;
        parms.canKidnap = true;
        var incidentDef = IncidentDefOf.RaidEnemy;
        incidentDef.Worker.TryExecute(parms);
    }
    
    private Thing FindThingOnMapToRequest(Map map)
    {
        var thingsFound = map.listerThings.GetAllThings(AllowedThings(), true).ToList();
        return thingsFound.Count == 0 ? null : thingsFound.RandomElement();
    }

    private Predicate<Thing> AllowedThings()
    {
        return thing =>
        {
            if (thing is Pawn || thing.GetType().Name == "VehiclePawn")
            {
                return false;
            }

            if (thing.def.category != ThingCategory.Item)
            {
                return false;
            }

            if (thing.MarketValue <= minimumMarketValue)
            {
                return false;
            }

            if (!ThingNotInUse(thing))
            {
                return false;
            }

            var forbiddableComp = thing.TryGetComp<CompForbiddable>();
            if (forbiddableComp is { Forbidden: true })
            {
                return false;
            }

            if (!thing.EverSeenByPlayer)
            {
                return false;
            }

            return true;
        };
    }

    private bool ThingNotInUse(Thing thing)
    {
        return KinContracts.All(kinContract => kinContract.requestedItem != thing);
    }

    private ThingDef FindThingsToSend(Thing requestedItem)
    {
        var valueToMatch = requestedItem.MarketValue * CurrentContractPriceMultAmount;

        var foundItem = false;
        var backToInitial = false;
        var initialStuff = stuffWheel.First();

        ThingDef itemToSend = null;
        
        while (!foundItem && !backToInitial)
        {
            var matchesPrice = PossibleItemsToGive[stuffWheel.First()].Where(def => def.BaseMarketValue <= valueToMatch)
                .ToList();
            if (matchesPrice.NullOrEmpty())
            {
                RotateWheel();
                if (initialStuff == stuffWheel.First())
                {
                    backToInitial = true;
                }
                continue;
            }

            itemToSend = matchesPrice.RandomElement();
            foundItem = true;
        }

        if (itemToSend != null)
        {
            
        }
        
        return itemToSend;
    }

    private void RotateWheel()
    {
        var first = stuffWheel.First;
        stuffWheel.RemoveFirst();
        stuffWheel.AddLast(first);
    }

    public void AcceptSentItem(Thing thing)
    {
        foreach (var kinContract in KinContracts.Where(kinContract => kinContract.requestedItem == thing))
        {
            kinContract.ItemReceivedFromPlayer = true;
            return;
        }
    }

    public void PayOffDebt(Thing thing)
    {
        if (thing.def == ThingDefOf.Silver)
        {
            playerDebt -= thing.MarketValue;
        }
        else
        {
            playerDebt -= thing.MarketValue * 0.75f;
        }
    }
    
    public override void ExposeData()
    {
        Scribe_Collections.Look(ref kinContracts, "kinContracts", LookMode.Deep);
        Scribe_Values.Look(ref setupComplete, "setupComplete");
        Scribe_Values.Look(ref CanRefresh, "CanRefresh");
        Scribe_Values.Look(ref playerDebt, "playerDebt");
        Scribe_Values.Look(ref refreshTick, "refreshTick");

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            CalculateStuff();
        }
    }

    public class KinContract : IExposable
    {
        private readonly IntRange RandomTickAmount = new(60000, 24000);
        
        //Item should be sent by this tick
        public int sendItemLatest;
        
        //Contract expires on this tick
        public int expiresOn;
        
        public Thing requestedItem = null;
        public Map requestedItemMap = null;
        public ThingDef givenItem = null;

        public bool ItemSentToPlayer = false;
        public bool ItemReceivedFromPlayer = false;
        
        public bool Invalid => requestedItem == null || givenItem == null;
        private bool Expired => Find.TickManager.TicksGame >= expiresOn;
        
        public KinContract()
        {
            expiresOn = Find.TickManager.TicksGame + RandomTickAmount.RandomInRange;
            sendItemLatest = Find.TickManager.TicksGame + RandomTickAmount.RandomInRange;
        }

        public KinContract(ThingDef itemGiven, Thing itemRequested)
        {
            givenItem = itemGiven;
            requestedItem = itemRequested;
            expiresOn = Find.TickManager.TicksGame + RandomTickAmount.RandomInRange;
            sendItemLatest = Find.TickManager.TicksGame + RandomTickAmount.RandomInRange;
        }
        
        public void RefreshContract(ThingDef itemsGiven, Thing itemRequested, Map map)
        {
            givenItem = itemsGiven;
            requestedItem = itemRequested;
            requestedItemMap = map;
            
            expiresOn = Find.TickManager.TicksGame + RandomTickAmount.RandomInRange;
            sendItemLatest = Find.TickManager.TicksGame + RandomTickAmount.RandomInRange;
        }

        public int GetStackAmount()
        {
            return (int)Math.Floor(requestedItem.MarketValue/givenItem.BaseMarketValue);
        }

        public string GetContractStatus()
        {
            if (ItemSentToPlayer && ItemReceivedFromPlayer)
            {
                return "BEWH.Abhuman.Contract.StatusCompleted".Translate();
            }
            if (ItemSentToPlayer)
            {
                return "BEWH.Abhuman.Contract.StatusAccepted".Translate();
            }
            if (!ItemSentToPlayer && Expired)
            {
                return "BEWH.Abhuman.Contract.StatusExpired".Translate();
            }
            
            return "BEWH.Abhuman.Contract.StatusAvailable".Translate();
        }

        public void InvalidateContract()
        {
            givenItem = null;
            requestedItem = null;
        }
        
        public void JumpToThingRequested()
        {
            CameraJumper.TryJump(requestedItem);
        }
        
        public void SendGivenThingToMap(Map map)
        {
            if (ItemSentToPlayer)
            {
                return;
            }
            var thingToSend = ThingMaker.MakeThing(givenItem);
            thingToSend.stackCount = GetStackAmount();

            ItemSentToPlayer = true;
            
            //TODO: Send letter with jumpto action
            
            TradeUtility.SpawnDropPod(DropCellFinder.TradeDropSpot(map), map, thingToSend);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref sendItemLatest, "sendItemLatest");
            Scribe_Values.Look(ref expiresOn, "expiresOn");
            Scribe_References.Look(ref requestedItem, "requestedItem");
            Scribe_References.Look(ref requestedItemMap, "requestedItemMap");
            Scribe_Defs.Look(ref givenItem, "givenItems");
            Scribe_Values.Look(ref ItemSentToPlayer, "ItemSentToPlayer");
            Scribe_Values.Look(ref ItemReceivedFromPlayer, "ItemReceivedFromPlayer");
        }
    }
}