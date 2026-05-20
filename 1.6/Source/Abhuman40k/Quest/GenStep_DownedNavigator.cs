using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Abhuman40k;

public class GenStep_DownedNavigator : GenStep_Scatterer
{
    public override int SeedPart => 931842770;

    protected override bool CanScatterAt(IntVec3 c, Map map)
    {
        if (base.CanScatterAt(c, map) && c.Standable(map))
        {
            return !c.Fogged(map);
        }
        return false;
    }

    protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
    {
        Pawn pawn;
        if (parms.sitePart is { things.Any: true })
        {
            pawn = (Pawn)parms.sitePart.things.Take(parms.sitePart.things[0]);
        }
        else
        {
            var component = map.Parent.GetComponent<DownedRefugeeComp>();
            pawn = component == null || !component.pawn.Any ? DownedRefugeeQuestUtility.GenerateRefugee(map.Tile) : component.pawn.Take(component.pawn[0]);
        }
        pawn.genes.SetXenotype(Abhuman40kDefOf.BEWH_Navigator);
        HealthUtility.DamageUntilDowned(pawn, allowBleedingWounds: false);
        HealthUtility.DamageLegsUntilIncapableOfMoving(pawn, allowBleedingWounds: false);
        loc = map.listerBuildings.AllBuildingsNonColonistOfDef(ThingDefOf.DeathrestCasket).First().Position;
        GenSpawn.Spawn(pawn, loc, map);
        pawn.mindState.WillJoinColonyIfRescued = true;
        MapGenerator.SetVar("RectOfInterest", CellRect.CenteredOn(loc, 1, 1));

        var lightballs = map.listerBuildings.AllBuildingsNonColonistOfDef(ThingDefOf.LightBall);
        foreach (var lightball in lightballs)
        {
            var spawnLoc = new IntVec3
            {
                x = lightball.Position.x,
                y = lightball.Position.y,
                z = lightball.Position.z
            };
            
            lightball.Destroy();
            
            GenSpawn.Spawn(Abhuman40kDefOf.BEWH_ReactorConduit, spawnLoc, map);
        }
        
        var turrets = map.listerBuildings.AllBuildingsNonColonistOfDef(ThingDefOf.Turret_MiniTurret);
        foreach (var turret in turrets)
        {
            turret.SetFaction(Faction.OfPirates);
        }
    }
}