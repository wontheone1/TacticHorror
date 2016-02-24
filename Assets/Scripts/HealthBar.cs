using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Unit unit;
    private int startingHealthPoint;

    private Image healthBarImage;
    // private GUITexture healthBarGui;
    private List<GameObject> RedBars = new List<GameObject>();

    private float barWidth;
    private float barHeight;
    private float barSpacing = 5f;
	void Start ()
	{ 
	    unit = GameObject.Find("frankenstein1").GetComponent<Unit>();
        startingHealthPoint = unit.MaxHp;
	    healthBarImage = ((GameObject) Resources.Load("RedBar")).GetComponent<Image>();
	    barWidth = healthBarImage.sprite.bounds.size.x;
        barHeight = healthBarImage.sprite.bounds.size.y;
        
        FillHealthBar(startingHealthPoint);
	}

    public void FillHealthBar(int n)
    {
        Transform healthBarStartPosTransfrom = gameObject.transform.FindChild("HealthBarStartPos");
        Vector3 barPos, healthBarStartPos;
        barPos = healthBarStartPos = healthBarStartPosTransfrom.position;
        for (int i = 0; i < n; i++)
        {
            barPos = new Vector3(healthBarStartPos.x + barWidth + 10, healthBarStartPos.y + i*(barHeight + barSpacing) - 5, barPos.z);
            RedBars.Add((GameObject) Instantiate((GameObject) Resources.Load("RedBar")
                , barPos
                , Quaternion.identity));
            // RedBars.Add(Instantiate((GameObject)Resources.Load("RedBar")));
            RedBars[i].transform.SetParent(healthBarStartPosTransfrom, false);
            // RedBars[i].transform.position = barPos;
            // RedBars[i].transform.localScale = Vector3.one;
        }
        
    }

    public void ModifyHealthBar(int amount)
    {
        
    }

}
