using UnityEngine;
using System.Collections;

public class vampireClass : Unit
{
    public void initialize()
    {
        MAX_ACTION_POINT = actionPoint = 100;
        MAX_HP = hp = 5;
        MAX_AP = ap = 5;
        MAX_MP = mp = 5;
        attackRange = 15;
    }
    protected override void Awake()
    {
        base.Awake();
        initialize();
    }
}
