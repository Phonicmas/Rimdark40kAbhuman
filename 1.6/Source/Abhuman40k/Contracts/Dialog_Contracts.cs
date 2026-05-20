using Core40k;
using RimWorld;
using UnityEngine;
using VEF.Utils;
using Verse;

namespace Abhuman40k;

public class Dialog_Contracts : Window
{
    private static readonly Vector2 ButSize = new(200f, 40f);

    public override Vector2 InitialSize => new(950f, 750f);

    private GameComponent_KinContracts gameComponentKinContracts = Current.Game.GetComponent<GameComponent_KinContracts>();
    private GameComponent_KinContracts GameComponent_KinContracts => gameComponentKinContracts;
    
    private float viewRectHeight;
    
    private Vector2 apparelColorScrollPosition;
    
    private GameComponent_KinContracts.KinContract currentKinContract;
    
    public override void DoWindowContents(Rect inRect)
    {
        var titleRect = new Rect(inRect)
        {
            height = Text.LineHeight * 2f
        };
        
        TitleSpaceWindow(titleRect);
        
        
        var infoRect = new Rect(inRect);
        infoRect.yMin += titleRect.height;
        
        var scrollRect = infoRect.TakeLeftPart(infoRect.width * 0.7f);
        var resetRect = scrollRect.TakeBottomPart(scrollRect.width * 0.1f);
        
        scrollRect = scrollRect.ContractedBy(10);
        infoRect = infoRect.ContractedBy(10);
        resetRect = resetRect.ContractedBy(10);

        var kinContractHeight = scrollRect.height / 6f;
        
        ContractWindow(scrollRect, kinContractHeight);
        RefreshWindow(resetRect);
        InfoWindow(infoRect);
        
        
    }

    private void TitleSpaceWindow(Rect titleRect)
    {
        titleRect = titleRect.ContractedBy(10);
        Text.Font = GameFont.Medium;
        Text.Anchor = TextAnchor.MiddleLeft;
        var contractAvailableRect = titleRect.TakeRightPart(titleRect.width * 0.75f);
        Widgets.Label(titleRect, "BEWH.Abhuman.Contract.Title".Translate().CapitalizeFirst());
        
        Text.Font = GameFont.Small;
        var priceMultRect = contractAvailableRect.TakeRightPart(contractAvailableRect.width * 0.66f);
        Widgets.Label(contractAvailableRect, "BEWH.Abhuman.Contract.MaximumContracts".Translate(gameComponentKinContracts.CurrentMaxContractAmount));
        TooltipHandler.TipRegion(contractAvailableRect, "BEWH.Abhuman.Contract.MaximumContractsTooltip".Translate());
        
        var playerDebtRect = priceMultRect.TakeRightPart(priceMultRect.width * 0.3f);
        Widgets.Label(priceMultRect, "BEWH.Abhuman.Contract.PriceMult".Translate(gameComponentKinContracts.CurrentContractPriceMultAmount.ToStringDecimalIfSmall()));
        TooltipHandler.TipRegion(priceMultRect, "BEWH.Abhuman.Contract.PriceMultTooltip".Translate());
        
        Widgets.Label(playerDebtRect, "BEWH.Abhuman.Contract.Goodwill".Translate(gameComponentKinContracts.CurrentGoodwillByPlayerDebt));
        TooltipHandler.TipRegion(playerDebtRect, "BEWH.Abhuman.Contract.GoodwillTooltip".Translate());
        
        Text.Anchor = TextAnchor.UpperLeft;
        Text.Font = GameFont.Medium;
    }

    private void ContractWindow(Rect scrollRect, float kinContractHeight)
    {
        var adjustedWidth = gameComponentKinContracts.CurrentMaxContractAmountForView > 6 ? 16f : 0f;
        
        var statusRect = scrollRect.TakeTopPart(scrollRect.height * 0.1f);
        statusRect.width -= adjustedWidth;
        
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleCenter;
        //status, requested, given, expires
        var requestedRect = statusRect.TakeRightPart(statusRect.width * 0.75f);
        var givenRect = requestedRect.TakeRightPart(requestedRect.width * 0.66f);
        var expiresInRect = givenRect.TakeRightPart(givenRect.width * 0.5f);    
        expiresInRect.width += adjustedWidth;
        
        Widgets.DrawMenuSection(statusRect);
        Widgets.DrawMenuSection(requestedRect);
        Widgets.DrawMenuSection(givenRect);
        Widgets.DrawMenuSection(expiresInRect);
        
        Widgets.Label(statusRect, "BEWH.Abhuman.Contract.Status".Translate());
        Widgets.Label(requestedRect, "BEWH.Abhuman.Contract.RequestedItem".Translate());
        Widgets.Label(givenRect, "BEWH.Abhuman.Contract.GivenItem".Translate());
        Widgets.Label(expiresInRect, "BEWH.Abhuman.Contract.ExpiresIn".Translate());
        
        Text.Anchor = TextAnchor.UpperLeft;
        Text.Font = GameFont.Medium;
        
        viewRectHeight = kinContractHeight * gameComponentKinContracts.CurrentMaxContractAmountForView;

        var viewRect = new Rect(scrollRect.x, scrollRect.y, scrollRect.width - adjustedWidth, viewRectHeight);
        
        Widgets.DrawMenuSection(scrollRect);
        
        Widgets.BeginScrollView(scrollRect, ref apparelColorScrollPosition, viewRect);
        
        //Hostile, only show one contract with debt repayment
        if (gameComponentKinContracts.KinFaction.HostileTo(Find.FactionManager.OfPlayer))
        {
            //Make specific view here, doesnt need to be a contract, should be glowing red and say debt repayment with the amount of worth of silver missing
            var contractRect = new Rect(viewRect)
            {
                height = kinContractHeight
            };
            contractRect = contractRect.ContractedBy(3f);
            Widgets.DrawRectFast(contractRect, Color.red);
            Widgets.EndScrollView();
            return;
        }
        
        var curY = scrollRect.y + 5f;
        
        foreach (var kinContract in GameComponent_KinContracts.KinContracts)
        {
            var contractRect = new Rect(viewRect)
            {
                height = kinContractHeight,
                y =  curY,
            };
            curY += kinContractHeight;
            contractRect = contractRect.ContractedBy(3f);
            Color? secondaryCol = null;
            if (currentKinContract == kinContract)
            {
                secondaryCol = Color.yellow;
            }
            if (Mouse.IsOver(contractRect))
            {
                Core40kUtils.DrawColoredMenuSection(contractRect, GenUI.MouseoverColor, secondaryCol);
            }
            else
            {
                Core40kUtils.DrawColoredMenuSection(contractRect, null, secondaryCol);
            }

            if (Widgets.ButtonInvisible(contractRect))
            {
                currentKinContract = kinContract;
            }
            var requestedRectInternal = contractRect.TakeRightPart(contractRect.width * 0.75f);
            var givenRectInternal = requestedRectInternal.TakeRightPart(requestedRectInternal.width * 0.66f);
            var expiresInRectInternal = givenRectInternal.TakeRightPart(givenRectInternal.width * 0.5f);
            
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(contractRect, kinContract.GetContractStatus());
            var requestedRectIcon = requestedRectInternal.TakeLeftPart(requestedRectInternal.width * 0.5f);
            Widgets.Label(requestedRectInternal, kinContract.requestedItem.LabelCap);
            Widgets.ThingIcon(requestedRectIcon, kinContract.requestedItem);
            var givenRectIcon = givenRectInternal.TakeLeftPart(givenRectInternal.width * 0.5f);
            Widgets.Label(givenRectInternal, "BEWH.Abhuman.Contract.GivenItemContract".Translate(kinContract.givenItem.LabelCap, kinContract.GetStackAmount()));
            Widgets.ThingIcon(givenRectIcon, kinContract.givenItem);
            Widgets.Label(expiresInRectInternal, "BEWH.Abhuman.Contract.ExpiresInContract".Translate(kinContract.expiresOn.ToStringTicksToPeriod()));
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Medium;
        }
        
        Widgets.EndScrollView();
    }

    private void RefreshWindow(Rect resetRect)
    {
        if (Mouse.IsOver(resetRect))
        {
            Core40kUtils.DrawColoredMenuSection(resetRect, GenUI.MouseoverColor, null);
        }
        else
        {
            Widgets.DrawMenuSection(resetRect);
        }
        
        var resetButRect = resetRect.TakeLeftPart(resetRect.height);
        GUI.DrawTexture(resetButRect, Abhuman40kUtils.RefreshIcon.Texture);
        
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleLeft;
        Widgets.Label(resetRect,
            !gameComponentKinContracts.CanRefresh
                ? "BEWH.Abhuman.Contract.RefreshCooldown".Translate(gameComponentKinContracts.RefreshIn)
                : "BEWH.Abhuman.Contract.CanRefresh".Translate());
        Text.Anchor = TextAnchor.UpperLeft;
        Text.Font = GameFont.Medium;
        
        if ((gameComponentKinContracts.CanRefresh || DebugSettings.godMode) && Widgets.ButtonInvisible(resetRect) )
        {
            gameComponentKinContracts.RefreshContracts();
        }
    }

    private void InfoWindow(Rect infoRect)
    {
        Widgets.DrawMenuSection(infoRect);
        if (currentKinContract == null)
        {
            return;
        }
        var nameRect = infoRect.TakeTopPart(infoRect.height * 0.10f);
        Widgets.Label(nameRect, currentKinContract.givenItem.LabelCap + currentKinContract.requestedItem.Label);
        
        var jumpToBut = infoRect.TakeBottomPart(infoRect.height * 0.2f);
        var acceptBut = jumpToBut.TakeRightPart(jumpToBut.width * 0.5f);

        jumpToBut = jumpToBut.ContractedBy(15f);
        acceptBut = acceptBut.ContractedBy(15f);

        if (Widgets.ButtonText(jumpToBut, "Jump to"))
        {
            currentKinContract.JumpToThingRequested();
        }

        if (Widgets.ButtonText(acceptBut, "accept stuff", active: !currentKinContract.ItemSentToPlayer))
        {
            currentKinContract.SendGivenThingToMap(Find.CurrentMap);
        }
    }
}