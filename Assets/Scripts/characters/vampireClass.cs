 // ReSharper disable once CheckNamespace
public class VampireClass : Unit
{

    protected override void Awake()
    {
        base.Awake();
        _maxActionPoint = ActionPoint = 100;
        _maxHp = Hp = 5;
        _ap = 5;
        _attackRange = 30;

        Unitname = "Vampire";
        projectileName = "rock";

        // FMOD Events assign
        DieEvent = "event:/Characters/Vampire/vamp_die";
        WalkEvent = "event:/Characters/Vampire/vamp_walk";
        LadderUpdownEvent = "event:/Characters/Vampire/vamp_up_down_ladder";
        GetHitEvent = "event:/Characters/Vampire/vamp_hitted";
        AttackEvent = "event:/Characters/Vampire/vamp_sucking";
    }

}
