﻿// ReSharper disable once CheckNamespace
public class SoldierClass : Unit
{
    public static int UnitCount = 0;

    protected override void Awake()
    {
        base.Awake();
        UnitCount++;
        _maxActionPoint = ActionPoint = 100;
        _maxHp = Hp = 15;
        _apMax = 1;
		_apMin = 10;
        _attackRange = 50;

        projectileName = "rocket";
        Unitname = "Soldier" + UnitCount;

        DieEvent = "event:/Characters/Soldier/soldier_counter_attack";
        WalkEvent = "event:/Characters/Soldier/soldier_walk";
        LadderUpdownEvent = "event:/Characters/Soldier/soldier_up_down_ladder";
        GetHitEvent = "event:/Characters/Soldier/soldier_hitted";
        AttackEvent = "event:/Characters/Soldier/soldier_shoot";
    }

}
