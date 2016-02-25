// ReSharper disable once CheckNamespace
public class SoldierClass : Unit
{
    

    protected override void Awake()
    {
        base.Awake();
        UnitCount++;
        _maxActionPoint = ActionPoint = 100;
        _maxHp = Hp = 5;
        _ap = 5;
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
