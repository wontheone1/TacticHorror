public class HealthBar : Bar
{
    protected override void Awake()
    {
        barPrefabName = "RedBar";
        barStartingPositionName = "HealthBarStartPos";
        verticalOrientation = 1;
        horizontalOrientation = 0;
        base.Awake();
    }
}
