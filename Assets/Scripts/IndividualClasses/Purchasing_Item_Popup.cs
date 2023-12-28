using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Purchasing_Item_Popup : MonoBehaviour
{
    public Shop_Item itemDetails;
    public Image itemImage;
    currencyType purchasingCurrencyType = currencyType.VertualCurrency;

    public TMPro.TMP_Text v_price, r_price;

    public Image coinsBtn, realMoneyBtn;

    public void selectCoins()
    {
        Color c = new Color();
        c.a = 0;
        realMoneyBtn.color = c;
        coinsBtn.color = Color.white;

        purchasingCurrencyType = currencyType.VertualCurrency;
    }
    public void selectRealMoney()
    {
        Color c = new Color();
        c.a = 0;
        coinsBtn.color = c;
        realMoneyBtn.color = Color.white;

        purchasingCurrencyType = currencyType.RealCurrency;
    }
    public void confirmPurchase()
    {
        InventoryManager.Instance.purchaseAnItem(itemDetails, purchasingCurrencyType);
    }

    private void OnEnable()
    {
        if(itemDetails.type != itemType.dama)
            itemImage.sprite = itemDetails.itemImage.sprite;

        if (itemDetails.currency == currencyType.VertualCurrency)
        {
            v_price.transform.parent.gameObject.SetActive(true);
            r_price.transform.parent.gameObject.SetActive(false);
            v_price.text = itemDetails.price.ToString();
            selectCoins();
        }
        else if (itemDetails.currency == currencyType.RealCurrency)
        {
            //Debug.Log("REal");
            v_price.transform.parent.gameObject.SetActive(false);
            r_price.transform.parent.gameObject.SetActive(true);
            r_price.text = "$" + itemDetails.r_Price.ToString();
            selectRealMoney();
        }
        else
        {
            v_price.transform.parent.gameObject.SetActive(true);
            r_price.transform.parent.gameObject.SetActive(true);
            v_price.text = itemDetails.price.ToString();
            r_price.text = "$" + itemDetails.r_Price.ToString();
            selectCoins();
        }
    }
}
