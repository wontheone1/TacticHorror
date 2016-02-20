 // ReSharper disable once CheckNamespace
public class VampireClass : Unit
{
    protected override void Initialize()
    {
        MaxActionPoint = ActionPoint = 100;
        MaxHp = Hp = 5;
        MaxAp = Ap = 5;
        MaxMp = Mp = 5;
        AttackRange = 30;
        Unitname = "Vampire";

        // FMOD Events assign
        DieEvent = "event:/Characters/Vampire/vamp_die";
        WalkEvent = "event:/Characters/Vampire/vamp_walk";
        LadderUpdownEvent = "event:/Characters/Vampire/vamp_up_down_ladder";
        GetHitEvent = "event:/Characters/Vampire/vamp_hitted";
        AttackEvent = "event:/Characters/Vampire/vamp_sucking";
    }
    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

}
