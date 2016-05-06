using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{

    private Image barImage;
    private List<GameObject> bars = new List<GameObject>();

    private float barWidth;
    private float barHeight;
    private float barSpacing = 4.5f;

    protected string barPrefabName { get; set; }
    protected string barStartingPositionName { get; set; }
    protected int verticalOrientation { get; set; }
    protected int horizontalOrientation { get; set; }

    protected virtual void Awake()
    {
        barImage = GetComponent<Image>();
        barWidth = barImage.sprite.bounds.size.x;
        barHeight = barImage.sprite.bounds.size.y;
    }

    public virtual void FillBar(int currentHP, int maxHP)
    {
        Debug.Log(maxHP + "dgadsg" + currentHP);
        barImage.fillAmount = (float) currentHP/maxHP;
    }

    //public virtual void FillBar(int n)
    //{
    //    foreach (GameObject bar in bars)
    //    {
    //        Destroy(bar);
    //    }
    //    bars.Clear();
    //    Transform barStartPosTransfrom = gameObject.transform.FindChild(barStartingPositionName);
    //    Vector3 barPos, barStartPos;
    //    barPos = barStartPos = barStartPosTransfrom.position;
    //    for (int i = 0; i < n; i++)
    //    {
    //        barPos = new Vector3(barStartPos.x + barWidth + (i * (barWidth + barSpacing * 1.35f) * horizontalOrientation)
    //            , barStartPos.y + (i * (barHeight + barSpacing * 0.75f) * verticalOrientation)
    //            , barPos.z);
    //        //Debug.Log(barPrefabName);
    //        bars.Add((GameObject)Instantiate((GameObject)Resources.Load(barPrefabName)
    //            , barPos
    //            , Quaternion.identity));
    //        // RedBars.Add(Instantiate((GameObject)Resources.Load("RedBar")));
    //        bars[i].transform.SetParent(barStartPosTransfrom, false);
    //        // RedBars[i].transform.position = barPos;
    //        // RedBars[i].transform.localScale = Vector3.one;
    //    }
    //}
}
