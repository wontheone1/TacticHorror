 // ReSharper disable once CheckNamespace
public class FrankenClass : Unit
{

    protected override void Awake()
    {
        base.Awake();
        _maxActionPoint = ActionPoint = 100;
        _maxHp = Hp = 15;
        _ap = 5;
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
    }

}
