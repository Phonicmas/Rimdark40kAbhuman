using HarmonyLib;
using Verse;

namespace Abhuman40k;

public class Abhuman40kMod : Mod
{
    public static Harmony harmony;

    public Abhuman40kMod(ModContentPack content) : base(content)
    {
        harmony = new Harmony("Abhuman40k.Mod");
        harmony.PatchAll();
    }
}