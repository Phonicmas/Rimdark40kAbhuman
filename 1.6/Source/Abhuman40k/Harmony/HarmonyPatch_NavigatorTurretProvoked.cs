using HarmonyLib;
using RimWorld;
using Verse;

namespace Abhuman40k;

[HarmonyPatch(typeof(Building_Turret), nameof(Building_Turret.PreApplyDamage))]
public class NavigatorTurretProvokedPatch
{
    public static void Postfix(Building_Turret __instance, DamageInfo dinfo)
    {
        if (!__instance.Spawned || __instance.Faction != null)
        {
            return;
        }

        if (dinfo.Instigator?.Faction != Faction.OfPlayer)
        {
            return;
        }

        var rescueComp = __instance.Map.GetComponent<MapComponent_NavigatorRescue>();
        if (rescueComp == null || !rescueComp.IsShipTurret(__instance))
        {
            return;
        }

        rescueComp.Notify_TurretsProvoked();
    }
}
