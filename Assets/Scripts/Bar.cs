using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Bar : MonoBehaviour {

    private Image barImage;
    private List<GameObject> bars = new List<GameObject>();

    private float barWidth;
    private float barHeight;
    private float barSpacing = 5f;

    protected string barPrefabName;
    protected string barStartingPositionName;
    protected int verticalOrientation;
    protected int horizontalOrientation;

    protected virtual void Start()
    {
        barImage = ((GameObject)Resources.Load(barPrefabName)).GetComponent<Image>();
        barWidth = barImage.sprite.bounds.size.x;
        barHeight = barImage.sprite.bounds.size.y;
    }

    public void FillBar(int n)
    {
        foreach (GameObject bar in bars)
        {
            Destroy(bar);
        }
        bars.Clear();
        Transform barStartPosTransfrom = gameObject.transform.FindChild(barStartingPositionName);
        Vector3 barPos, barStartPos;
        barPos = barStartPos = barStartPosTransfrom.position;
        for (int i = 0; i < n; i++)
        {
            barPos = new Vector3(barStartPos.x + barWidth + (i * (barWidth + barSpacing) * horizontalOrientation)
                , barStartPos.y + (i * (barHeight + barSpacing) * verticalOrientation)
                , barPos.z);

            bars.Add((GameObject)Instantiate((GameObject)Resources.Load(barPrefabName)
                , barPos
                , Quaternion.identity));
            // RedBars.Add(Instantiate((GameObject)Resources.Load("RedBar")));
            bars[i].transform.SetParent(barStartPosTransfrom, false);
            // RedBars[i].transform.position = barPos;
            // RedBars[i].transform.localScale = Vector3.one;
        }
    }
}
