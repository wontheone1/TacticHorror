using UnityEngine;
using System.Collections;

public class vampireClass : Unit
{
    public void initialize()
    {
        MAX_ACTION_POINT = actionPoint = 100;
        MAX_HP = hp = 10;
        MAX_AP = ap = 5;
        MAX_MP = mp = 5;
        attackRange = 14;
    }
    void Awake()
    {
        initialize();
    }
    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
