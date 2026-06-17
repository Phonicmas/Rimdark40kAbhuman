using System.Text;
using Verse;

namespace Abhuman40k;

public class Building_HerdstoneEnemy : Building
{
    protected override void Tick()
    {
        base.Tick();
    }
    
    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        stringBuilder.AppendLineIfNotEmpty();
        return stringBuilder.ToString();
    }
    
    //When eneough are summoned make the previous "Group" attack, so one always "defends" basically and one always run toward the enemy
    
    //Makae curve for amaount, and curvee for amount that will be minotaurs
    
    public override void ExposeData()
    {
        base.ExposeData();
    }
}