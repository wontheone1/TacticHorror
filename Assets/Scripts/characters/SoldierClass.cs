// ReSharper disable once CheckNamespace
public class SoldierClass : Unit
{
    protected override void Initialize()
    {
        UnitCount++;
        MaxActionPoint = ActionPoint = 2500;
        MaxHp = Hp = 5;
        MaxAp = Ap = 5;
        MaxMp = Mp = 5;
        AttackRange = 50;
        Unitname = "Soldier" + UnitCount;
        DieEvent = "event:/Characters/Soldier/soldier_counter_attack";
        WalkEvent = "event:/Characters/Soldier/soldier_walk";
        LadderUpdownEvent = "event:/Characters/Soldier/soldier_up_down_ladder";
        GetHitEvent = "event:/Characters/Soldier/soldier_hitted";
        AttackEvent = "event:/Characters/Soldier/soldier_shoot";
    }

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    public override void Die()
    {
        base.Die();
    }
}
