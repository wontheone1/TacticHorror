using UnityEngine;
using System.Collections;

public class VampireClass : Unit
{
    public void initialize()
    {
        MAX_ACTION_POINT = actionPoint = 100;
        MAX_HP = hp = 5;
        MAX_AP = ap = 5;
        MAX_MP = mp = 5;
        attackRange = 15;
        name = "Vampire";

        /// FMOD Events assign
        dieEvent = "event:/Characters/Vampire/vamp_die";
        walkEvent = "event:/Characters/Vampire/vamp_walk";
        ladderUpdownEvent = "event:/Characters/Vampire/vamp_up_down_ladder";
        getHitEvent = "event:/Characters/Vampire/vamp_hitted";
        attackEvent = "event:/Characters/Vampire/vamp_sucking";
    }
    protected override void Awake()
    {
        base.Awake();
        initialize();
    }

}
