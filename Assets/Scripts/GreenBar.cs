public class GreenBar : Bar
{
    protected override void Awake()
    {
        barPrefabName = "GreenBar";
        barStartingPositionName = "ActionBarStartPos";
        verticalOrientation = 0;
        horizontalOrientation = 1;
        base.Awake();
    }
}
