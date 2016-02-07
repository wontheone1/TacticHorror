using UnityEngine;
using System.Collections;

public class AmericanSoldierClass : Unit
{
    public void initialize()
    {
        MAX_ACTION_POINT = actionPoint = 100;
        MAX_HP = hp = 10;
        MAX_AP = ap = 5;
        MAX_MP = mp = 5;
        attackRange = 14;
    }

    // Use this for initialization
    void Start () {
	    initialize();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
