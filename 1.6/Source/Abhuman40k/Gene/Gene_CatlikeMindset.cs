using Verse;

namespace Abhuman40k;

public class Gene_CatlikeMindset : Gene
{
    private const int MtbNuzzleTicks = 90000;
    private bool canNuzzle = false;
    public bool CanNuzzle => canNuzzle;
    
    public override void TickInterval(int delta)
    {
        base.TickInterval(delta);
        if (!pawn.IsHashIntervalTick(MtbNuzzleTicks, delta))
        {
            return;
        }
        canNuzzle = true;
    }

    public void Nuzzled()
    {
        canNuzzle = false;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref canNuzzle, "canNuzzle");
    }
}