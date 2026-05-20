using RimWorld;
using Verse;

namespace Abhuman40k;

public class Comp_TimedExplosion : ThingComp 
{
    private int ticksRemaining;

    private CompProperties_TimedExplosion Props => (CompProperties_TimedExplosion)props;
    
    public override void PostSpawnSetup(bool respawningAfterLoad) 
    {
        ticksRemaining = Props.ticksToExplode.RandomInRange;
    }
    
    public override void CompTick()
    {
        if (ticksRemaining > 0)
        {
            ticksRemaining--;
            return;
        }
        
        DoExplosion(parent.Map);
        parent.Destroy();
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        DoExplosion(map);
    }


    private void DoExplosion(Map map)
    {
        GenExplosion.DoExplosion(parent.Position, map, Props.explosionRadius, Props.damageDef, parent, Props.damageAmount);
    }
    
    public override string CompInspectStringExtra()
    {
        return "BEWH.Abhuman.Misc.ExplosionIn".Translate(ticksRemaining.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor));
    }
}