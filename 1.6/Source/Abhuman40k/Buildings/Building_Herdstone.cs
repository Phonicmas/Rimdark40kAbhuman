using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Abhuman40k;

public class Building_Herdstone : Building
{
    public bool canBeWorked = false;
    
    private int totalTime = 0;
    private float TotalTimeAdjusted
    {
        get
        {
            var multiplier = 1f;
            var affinityPawns = Map.mapPawns.FreeColonists
                .Count(pawn => pawn.genes.HasActiveGene(Abhuman40kDefOf.BEWH_MinotaurAdrenalSurgeNode));

            multiplier += 0.2f * affinityPawns;
            
            return (int)(totalTime * multiplier);
        }
    }
    private float progressInt = 0;
    public float TimeLeft => TotalTimeAdjusted - progressInt;
    public bool Finished => TotalTimeAdjusted - progressInt <= 0;
    private float PercentFinished => progressInt / TotalTimeAdjusted;
    
    private int cooldownInt;

    [Unsaved(false)]
    private Effecter progressBar;

    protected override void Tick()
    {
        base.Tick();
        if (cooldownInt > 0)
        {
            cooldownInt--;
        }
    }

    public void WorkTick()
    {
        if (!canBeWorked)
        {
            return;
        }

        if (Finished)
        {
            Finish();
        }
        else
        {
            progressInt++;
            TickEffects();
        }
    }
    
    private void TickEffects()
    {
        progressBar ??= EffecterDefOf.ProgressBarAlwaysVisible.Spawn();
        progressBar.EffectTick(new TargetInfo(this.TrueCenter().ToIntVec3(), Map), TargetInfo.Invalid);
        var mote = ((SubEffecter_ProgressBar)progressBar.children[0]).mote;
        if (mote == null)
        {
            return;
        }

        mote.progress = Mathf.Clamp01(PercentFinished);
        mote.offsetZ = Rotation == Rot4.North ? 0.5f : -0.5f;
    }
    
    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        if (progressBar != null)
        {
            progressBar.Cleanup();
            progressBar = null;
        }
        base.DeSpawn(mode);
    }
    
    private void Finish()
    {
        var pawn = PawnGenerator.GeneratePawn(Abhuman40kDefOf.BEWH_HerdstoneBornMinotaur, Faction.OfPlayer);
        var minotaur = (Pawn)GenSpawn.Spawn(pawn, Position.RandomAdjacentCell8Way(), Map);

        var letter = LetterMaker.MakeLetter(
            "BEWH.Abhuman.Beastman.SpawnedMinotaurLetter".Translate(), 
            "BEWH.Abhuman.Beastman.SpawnedMinotaurMessage".Translate(minotaur), 
            LetterDefOf.PositiveEvent, minotaur);
        
        Find.LetterStack.ReceiveLetter(letter);

        cooldownInt = 30000;
        
        Reset();
    }
    
    private void Reset()
    {
        totalTime = 0;
        progressInt = 0;
        
        if (progressBar != null)
        {
            progressBar.Cleanup();
            progressBar = null;
        }

        canBeWorked = false;
    }
    
    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        stringBuilder.AppendLineIfNotEmpty();
        if (TimeLeft > 0)
        {
            if (TimeLeft > 60000)
            {
                stringBuilder.AppendTagged("BEWH.Abhuman.Beastman.HerdstoneTimeUntillBirth".Translate() + ": " + ((int)TimeLeft).ToStringTicksToDays().Colorize(ColoredText.DateTimeColor));
            }
            else
            {
                stringBuilder.AppendTagged("BEWH.Abhuman.Beastman.HerdstoneTimeUntillBirth".Translate() + ": " + ((int)TimeLeft).ToStringTicksToPeriod(allowYears: false).Colorize(ColoredText.DateTimeColor));
            }
        }
        else if (cooldownInt > 0)
        {
            if (cooldownInt > 60000)
            {
                stringBuilder.AppendTagged("BEWH.Abhuman.Beastman.HerdstoneCooldown".Translate() + ": " + ((int)cooldownInt).ToStringTicksToDays().Colorize(ColoredText.DateTimeColor));
            }
            else
            {
                stringBuilder.AppendTagged("BEWH.Abhuman.Beastman.HerdstoneCooldown".Translate() + ": " + ((int)cooldownInt).ToStringTicksToPeriod(allowYears: false).Colorize(ColoredText.DateTimeColor));
            }
        }
        else
        {
            stringBuilder.AppendTagged("BEWH.Abhuman.Beastman.HerdstoneReadyForRitual".Translate());
        }
            
        return stringBuilder.ToString();
    }
    
    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        if (!canBeWorked && cooldownInt <= 0)
        {
            var gestateCommand = new Command_Action
            {
                defaultLabel = "BEWH.Abhuman.Beastman.GestateMinotaur".Translate(),
                action = delegate
                {
                    canBeWorked = true;
                    totalTime = 600000;
                }
            };
            yield return gestateCommand;
        }
        else
        {
            if (DebugSettings.ShowDevGizmos)
            {
                var almostFinnishCommand = new Command_Action
                {
                    defaultLabel = "DEV: Almost Finish",
                    action = delegate
                    {
                        progressInt = TotalTimeAdjusted - 500;
                    }
                };
                yield return almostFinnishCommand;
            }
        }

        
    }
    
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref totalTime, "totalTime");
        Scribe_Values.Look(ref progressInt, "progressInt");
        Scribe_Values.Look(ref cooldownInt, "cooldownInt");
        
        Scribe_Values.Look(ref canBeWorked, "canBeWorked");
    }
}