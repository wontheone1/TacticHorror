 // ReSharper disable once CheckNamespace
public class FrankenClass : Unit
{

    protected override void Initialize()
    {
        MaxActionPoint = ActionPoint = 100;
        MaxHp = Hp = 5;
        MaxAp = Ap = 5;
        MaxMp = Mp = 5;
        AttackRange = 50;
        Unitname = "Frankenstein";

        // FMOD Events assign
        DieEvent = "event:/Characters/Frankenstein/frank_die";
        WalkEvent = "event:/Characters/Frankenstein/frank_walk";
        LadderUpdownEvent = "event:/Characters/Frankenstein/frank_up_down_ladder";
        GetHitEvent = "event:/Characters/Frankenstein/frank_hitted";
        AttackEvent = "event:/Characters/Frankenstein/frank_sucking";
        JumpEvent = "event:/Characters/Frankenstein/frank_jump";
    }
    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

}
