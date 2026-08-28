using RimWorld;
using Verse;

namespace Abhuman40k;

public class LordToilData_SiegeBeastman : LordToilData_Siege
{
	/// <summary>The raid herdstone, once the shamans have finished raising it.</summary>
	public Building_HerdstoneEnemy herdstone;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref herdstone, "herdstone");
	}
}
