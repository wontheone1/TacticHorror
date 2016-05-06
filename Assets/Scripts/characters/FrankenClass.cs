 // ReSharper disable once CheckNamespace

using UnityEngine;

public class FrankenClass : Unit
{
    public static int UnitCount = 0;

    protected override void Awake()
    {
        base.Awake();
        UnitCount++;

        _maxActionPoint = ActionPoint = 100;
        _maxHp = Hp = 15;
        _apMin = 3;
		_apMax = 8;
        _attackRange = 50;

        Unitname = "Frankenstein";
        projectileName = "rock";

        // FMOD Events assign
        DieEvent = "event:/Characters/Frankenstein/frank_die";
        WalkEvent = "event:/Characters/Frankenstein/frank_walk";
        LadderUpdownEvent = "event:/Characters/Frankenstein/frank_up_down_ladder";
        GetHitEvent = "event:/Characters/Frankenstein/frank_hitted";
        AttackEvent = "event:/Characters/Frankenstein/frank_punch";
        JumpEvent = "event:/Characters/Frankenstein/frank_jump";

       // Debug.Log("FrankenStatus" + UnitCount);
        HealthBar = GameObject.Find("FrankenStatus" + UnitCount).transform.Find("HealthBar").GetComponent<HealthBar>();
        GreenBar = GameObject.Find("FrankenStatus" + UnitCount).GetComponent<GreenBar>();
    }

}
