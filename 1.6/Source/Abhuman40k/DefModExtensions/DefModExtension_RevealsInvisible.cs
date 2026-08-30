using System.Collections.Generic;
using Verse;

namespace Abhuman40k;

public class DefModExtension_RevealsInvisible : DefModExtension
{
    //Base reveal radius in cells.
    public float radius = 12.9f;

    //Scales the radius by (1 + (PsychicSensitivity - 1) * this). 0 disables scaling.
    public float psychicSensitivityFactor = 0f;

    //Whether walls block the reveal.
    public bool requiresLineOfSight = true;

    //Only reveal pawns whose faction is hostile to the seer's faction.
    public bool hostileOnly = false;

    //Reveal is suppressed while worn apparel covers any of these body part groups.
    public List<BodyPartGroupDef> blockedIfCovered = new();
}
