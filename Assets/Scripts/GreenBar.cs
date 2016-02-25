using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GreenBar : Bar
{

    protected override void Start()
    {
        barPrefabName = "GreenBar";
        barStartingPositionName = "ActionBarStartPos";
        verticalOrientation = 0;
        horizontalOrientation = 1;
        base.Start();
    }

}
