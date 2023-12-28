using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum currencyType
{
    VertualCurrency,
    RealCurrency,
    both
}

public class Shop_Item : MonoBehaviour
{
    public int id;
    public currencyType currency = currencyType.RealCurrency;
    public itemType type = itemType.board;
    public int price;
    public float r_Price;
    public bool isOwn;
    public GameObject priceTag;
    public UnityEngine.UI.Image itemImage;
    public TMPro.TMP_Text purchasedTxt;

    [Header("For Coins Only")]
    [Tooltip("This is the coins will be added after this purchase")]public int addAmount;

    public void ownItem()
    {
        isOwn = true;
        priceTag.SetActive(false);
        purchasedTxt.gameObject.SetActive(true);
    }
    public void clickItem()
    {
        if(isOwn)
        {
            //select item
        }
        else
        {
            //purchase item
            InventoryManager.Instance.selectItem(this);
        }
    }


}
