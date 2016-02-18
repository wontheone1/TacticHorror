using UnityEngine;
using System.Collections;

public class FrankenClass : Unit
{

    public void initialize()
    {
        MAX_ACTION_POINT = actionPoint = 100;
        MAX_HP = hp = 15;
        MAX_AP = ap = 3;
        MAX_MP = mp = 5;
        attackRange = 50;
        unitname = "Frankenstein";

        /// FMOD Events assign
        dieEvent = "event:/Characters/Frankenstein/frank_die";
        walkEvent = "event:/Characters/Frankenstein/frank_walk";
        ladderUpdownEvent = "event:/Characters/Frankenstein/frank_up_down_ladder";
        getHitEvent = "event:/Characters/Frankenstein/frank_hitted";
        attackEvent = "event:/Characters/Frankenstein/frank_sucking";
        jumpEvent = "event:/Characters/Frankenstein/frank_jump";
    }
    protected override void Awake()
    {
        base.Awake();
        initialize();
    }

}
