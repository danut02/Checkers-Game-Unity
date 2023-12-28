using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollAutoContent : MonoBehaviour
{
    public float offset;
    public float itemHeight;
    public bool skipFirstItem;
    private float rectHeight;
    public int NoOfItemsInRow = 1;

    //public bool adjustWithGrid;
    //private GridLayout grid;
    private void OnEnable()
    {
        rectHeight = 0;
        adjustContentSize();
    }

    private void adjustContentSize()
    {
        //grid = GetComponent<GridLayout>();
        //if (adjustWithGrid && grid != null)
        //{
        //    offset = grid.cellGap.y;
        //    itemHeight = grid.cellSize.y;
        //    Debug.LogFormat("{0},{1}", "Grid", itemHeight);
        //}

        int NoOfItems = transform.childCount;
        float totalHeightOfitem = offset + itemHeight;
        if (skipFirstItem) NoOfItems -= 1;
        for (int i = 0; i < NoOfItems; i += NoOfItemsInRow)
        {
            rectHeight += totalHeightOfitem;
        }
        transform.GetComponent<RectTransform>().sizeDelta = new Vector2(0, rectHeight);
    }
}
