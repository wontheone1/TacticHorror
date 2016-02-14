using UnityEngine;
using System.Collections;

public class SoldierClass : Unit
{
    public void initialize()
    {
        UNIT_COUNT++;
        MAX_ACTION_POINT = actionPoint = 100;
        MAX_HP = hp = 5;
        MAX_AP = ap = 5;
        MAX_MP = mp = 5;
        attackRange = 15;
        unitname = "Soldier" + UNIT_COUNT;
        dieEvent = "event:/Characters/Soldier/soldier_counter_attack";
        walkEvent = "event:/Characters/Soldier/soldier_walk";
        ladderUpdownEvent = "event:/Characters/Soldier/soldier_up_down_ladder";
        getHitEvent = "event:/Characters/Soldier/soldier_hitted";
        attackEvent = "event:/Characters/Soldier/soldier_shoot";
    }

    protected override void Awake()
    {
        base.Awake();
        initialize();
    }

    public override void die()
    {
        base.die();
    }
}
