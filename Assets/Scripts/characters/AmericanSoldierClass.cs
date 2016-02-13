using UnityEngine;
using System.Collections;

public class AmericanSoldierClass : Unit
{
    public void initialize()
    {
        UNIT_COUNT++;
        MAX_ACTION_POINT = actionPoint = 100;
        MAX_HP = hp = 5;
        MAX_AP = ap = 5;
        MAX_MP = mp = 5;
        attackRange = 15;
        name = "Soldier" + UNIT_COUNT;
    }

    protected override void Awake()
    {
        base.Awake();
        initialize();
    }
}
