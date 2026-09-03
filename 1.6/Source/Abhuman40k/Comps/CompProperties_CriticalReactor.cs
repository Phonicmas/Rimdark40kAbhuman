using System.Collections.Generic;
using Verse;

namespace Abhuman40k;

public class ReactorGlowStop
{
    public float progress;
    public ColorInt color;
}

public class CompProperties_CriticalReactor : CompProperties
{
    public IntRange ticksToExplode = new(15000, 20000);

    public GraphicData unstableGraphicData;

    public List<ReactorGlowStop> glowStops = new();
    public int glowUpdateInterval = 60;

    public float explosionRadius = 30f;
    public DamageDef damageDef;
    public int damageAmount = 1500;
    public float chanceToStartFire = 0.35f;
    public ThingDef craterDef;
    public LetterDef detonationLetter;
    public float screenShakeFactor = 4f;

    public float toughnessThreshold = 2f;
    public float survivorHitPointsFactor = 0.15f;
    public List<ThingDef> toughThingDefs = new();

    public CompProperties_CriticalReactor()
    {
        compClass = typeof(Comp_CriticalReactor);
    }

    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        foreach (var error in base.ConfigErrors(parentDef))
        {
            yield return error;
        }

        if (damageDef == null)
        {
            yield return parentDef.defName + " has a CompProperties_CriticalReactor with no damageDef.";
        }

        if (glowStops.NullOrEmpty())
        {
            yield return parentDef.defName + " has a CompProperties_CriticalReactor with no glowStops.";
        }
    }
}
