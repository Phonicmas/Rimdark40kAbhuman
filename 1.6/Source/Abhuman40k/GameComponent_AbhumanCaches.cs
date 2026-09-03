using Verse;

namespace Abhuman40k;

/// <summary>
/// Resets the mod's tick-keyed static caches. TicksGame restarts when a save is loaded, so a
/// cache bucket from the previous session can otherwise match and be reused against pawns that
/// no longer exist.
/// </summary>
public class GameComponent_AbhumanCaches : GameComponent
{
    public GameComponent_AbhumanCaches(Game game)
    {
    }

    public override void LoadedGame()
    {
        base.LoadedGame();
        WarpEyeRevealUtility.ResetCache();
    }

    public override void StartedNewGame()
    {
        base.StartedNewGame();
        WarpEyeRevealUtility.ResetCache();
    }
}
