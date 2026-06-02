using HarmonyLib;
using Verse;

namespace Abhuman40k;

public class Abhuman40kMod : Mod
{
    public static string CurrentVersion;
    
    public static Harmony harmony;
    
    public Abhuman40kMod(ModContentPack content) : base(content)
    {
        harmony = new Harmony("Abhuman40k.Mod");
        CurrentVersion = content.ModMetaData.ModVersion;
        
        harmony.PatchAll();
    }
    
    public override string SettingsCategory()
    {
        return "BEWH.Abhuman.ModSettings.ModName".Translate(CurrentVersion);
    }
}