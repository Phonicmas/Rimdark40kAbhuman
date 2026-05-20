using RimWorld;
using Verse;

namespace Abhuman40k;

public class CompProperties_TimedExplosion : CompProperties 
{
    public IntRange ticksToExplode = new(15000, 20000); // 6 in-game hours
    public float explosionRadius = 4.9f;
    public DamageDef damageDef = DamageDefOf.Bomb;
    public int damageAmount = 120;
    
    public CompProperties_TimedExplosion()
    {
        compClass = typeof(Comp_TimedExplosion);
    }
}