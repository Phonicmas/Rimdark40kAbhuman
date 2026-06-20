using System.Collections.Generic;
using System.Text;
using Verse;

namespace Abhuman40k;

public class Building_HerdstoneEnemy : Building
{

    private List<Pawn> newGroupToAttack = [];

    private int waveNumber = 1;

    private SimpleCurve minotaurPercentage =
    [
        new CurvePoint(1, 0f),
        new CurvePoint(3, 0.2f),
        new CurvePoint(5, 0.4f),
        new CurvePoint(8, 0.6f),
        new CurvePoint(10, 1f)
    ];
    
    private SimpleCurve groupPointStrength =
    [
        new CurvePoint(1, 1f),
        new CurvePoint(3, 1.2f),
        new CurvePoint(5, 1.4f),
        new CurvePoint(8, 1.6f),
        new CurvePoint(10, 2f)
    ];
    
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