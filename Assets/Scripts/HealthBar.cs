public class HealthBar : Bar
{
    protected override void Awake()
    {
        barPrefabName = "RedBar";
        barStartingPositionName = "HealthBarStartPos";
        verticalOrientation = 0;
        horizontalOrientation = 1;
        base.Awake();
    }
}
