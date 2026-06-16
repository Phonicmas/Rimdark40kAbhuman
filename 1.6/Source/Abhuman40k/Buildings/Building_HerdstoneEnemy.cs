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
    
    public override void ExposeData()
    {
        base.ExposeData();
    }
}