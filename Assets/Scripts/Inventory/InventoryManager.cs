using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine.UI;
using TMPro;

public enum itemType
{
    board,
    pieces,
    dama,
    coins,
    emote
}
[Serializable]
public class purchasedItems
{
    public List<int> itemIds = new List<int>();
}


public class InventoryManager : MonoBehaviour
{
    [HideInInspector]public MenuManager menuManagerScript;
    public PlayfabManager playfabScript;
    public GameObject loadingPanel;

    public static InventoryManager Instance;

    [Header("Shop Items")]
    public GameObject[] boardItems;
    public GameObject[] piecesItems;
    public GameObject[] damasItems;
    public GameObject[] emotesItem;

    //shop classes
    private purchasedItems boards = new purchasedItems();
    private purchasedItems pieces = new purchasedItems();
    private purchasedItems damas = new purchasedItems();
    private purchasedItems emotes = new purchasedItems();

    Shop_Item selectedItem;
    public Purchasing_Item_Popup purchasingPopup;

    public GameObject inv_Board;
    public Transform inv_Baord_P;
    public Transform inv_Piece_P;
    public GameObject inv_Dama;
    public Transform inv_Dama_P;
    public GameObject inv_Emote;
    public Transform inv_Emote_P;

    public GameObject notEnoughCoinsPnl;

    [Header("UI")]
    public Sprite[] board_images;
    public Sprite[] pieces_images;
    public Sprite[] emotes_images;

    public TMP_Text[] allCoinsText;

    public RuntimeAnimatorController[] dama_Anims;

    public TMP_Text remVidTxt;


    private void Awake()
    {
        Instance = this;
        playfabScript = PlayfabManager.instance;
    }
    void Start()
    {
        //purchaseAnItem("Board_0", itemType.board);
        loadInventory();

        //Invoke("updateCoins", 2);
    }

    void Update()
    {

    }
    public void updateCoins(int coins)
    {
        playfabScript.coins = coins;
        for (int i = 0; i < allCoinsText.Length; i++)
        {
            allCoinsText[i].text = coins.ToString();
        }
    }
    void loadInventory()
    {
        if(playfabScript.userData.ContainsKey("Boards"))
        {
            boards = JsonUtility.FromJson<purchasedItems>(playfabScript.userData["Boards"].Value);
        }
        if (playfabScript.userData.ContainsKey("Pieces"))
        {
            pieces = JsonUtility.FromJson<purchasedItems>(playfabScript.userData["Pieces"].Value);
        }
        if (playfabScript.userData.ContainsKey("Damas"))
        {
            damas = JsonUtility.FromJson<purchasedItems>(playfabScript.userData["Damas"].Value);
        }
        if (playfabScript.userData.ContainsKey("Emotes"))
        {
            emotes = JsonUtility.FromJson<purchasedItems>(playfabScript.userData["Emotes"].Value);
        }

        foreach (int id in boards.itemIds)
        {
            boardItems[id].GetComponent<Shop_Item>().ownItem();
            GameObject board = Instantiate(inv_Board, inv_Baord_P);
            board.SetActive(true);
            Inventory_Item inv_Item = board.GetComponent<Inventory_Item>();
            menuManagerScript.boardBtns[id] = inv_Item.selectButton.image;
            inv_Item.selectButton.onClick.AddListener(() => menuManagerScript.selectBoard(id));
            inv_Item.image.sprite = board_images[id];
        }
        foreach (int id in pieces.itemIds)
        {
            piecesItems[id].GetComponent<Shop_Item>().ownItem();
            GameObject piece = Instantiate(inv_Board, inv_Piece_P);
            piece.SetActive(true);
            Inventory_Item inv_Item = piece.GetComponent<Inventory_Item>();
            menuManagerScript.piecesBtns[id] = inv_Item.selectButton.image;
            inv_Item.selectButton.onClick.AddListener(() => menuManagerScript.selectPiece(id));
            inv_Item.image.sprite = pieces_images[id];

        }
        foreach (int id in damas.itemIds)
        {
            damasItems[id].GetComponent<Shop_Item>().ownItem();
            GameObject dama = Instantiate(inv_Dama, inv_Dama_P);
            dama.SetActive(true);
            Inventory_Item inv_Item = dama.GetComponent<Inventory_Item>();
            menuManagerScript.damaBtns[id] = inv_Item.selectButton.image;
            inv_Item.selectButton.onClick.AddListener(() => menuManagerScript.selectDama(id));
            inv_Item.animator.runtimeAnimatorController = dama_Anims[id];
        }
        foreach (int id in emotes.itemIds)
        {
            emotesItem[id].GetComponent<Shop_Item>().ownItem();
            GameObject emote = Instantiate(inv_Emote, inv_Emote_P);
            emote.SetActive(true);
            Inventory_Item inv_Item = emote.GetComponent<Inventory_Item>();
            menuManagerScript.emotesBtn[id] = inv_Item.selectButton.image;
            inv_Item.selectButton.onClick.AddListener(() => menuManagerScript.addEmote(id));
            inv_Item.image.sprite = emotes_images[id];
        }
    }

    public void selectItem(Shop_Item item)
    {
        selectedItem = item;
        purchasingPopup.itemDetails = item;
        purchasingPopup.gameObject.SetActive(true);
    }
    public void purchaseAnItem(Shop_Item item, currencyType currencyType)
    {
        if (currencyType == currencyType.VertualCurrency)
        {
            if (item.price > playfabScript.coins)
            {
                //Not enough coins
                notEnoughCoinsPnl.SetActive(true);
                purchasingPopup.gameObject.SetActive(false);
                return;
            }
        }
        else
        {

        }

        loadingPanel.SetActive(true);


        string key = "", data = "";
        if(item.type == itemType.board)
        {
            boards.itemIds.Add(item.id);
            key = "Boards";
            data = JsonUtility.ToJson(boards);
        }
        else if(item.type == itemType.pieces)
        {
            pieces.itemIds.Add(item.id);
            key = "Pieces";
            data = JsonUtility.ToJson(pieces);
        }
        else if (item.type == itemType.dama)
        {
            damas.itemIds.Add(item.id);
            key = "Damas";
            data = JsonUtility.ToJson(damas);
        }
        else if (item.type == itemType.emote)
        {
            emotes.itemIds.Add(item.id);
            key = "Emotes";
            data = JsonUtility.ToJson(emotes);
        }

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>()
            {
                { key, data }
            }

        }, response =>
        {
            if (item.type == itemType.board)
            {
                int i_id = boards.itemIds[boards.itemIds.Count - 1];
                boardItems[i_id].GetComponent<Shop_Item>().ownItem();
                GameObject board = Instantiate(inv_Board, inv_Baord_P);
                board.SetActive(true);
                Inventory_Item inv_Item = board.GetComponent<Inventory_Item>();
                menuManagerScript.boardBtns[i_id] = inv_Item.selectButton.image;
                inv_Item.selectButton.onClick.AddListener(() => menuManagerScript.selectBoard(i_id));
                inv_Item.image.sprite = board_images[i_id];
            }
            else if (item.type == itemType.pieces)
            {
                int i_id = pieces.itemIds[pieces.itemIds.Count - 1];
                piecesItems[i_id].GetComponent<Shop_Item>().ownItem();
                GameObject piece = Instantiate(inv_Board, inv_Piece_P);
                piece.SetActive(true);
                Inventory_Item inv_Item = piece.GetComponent<Inventory_Item>();
                menuManagerScript.piecesBtns[i_id] = inv_Item.selectButton.image;
                inv_Item.selectButton.onClick.AddListener(() => menuManagerScript.selectPiece(i_id));
                inv_Item.image.sprite = pieces_images[i_id];
            }
            else if (item.type == itemType.dama)
            {
                int i_id = damas.itemIds[damas.itemIds.Count - 1];
                damasItems[i_id].GetComponent<Shop_Item>().ownItem();
                GameObject dama = Instantiate(inv_Dama, inv_Dama_P);
                dama.SetActive(true);
                Inventory_Item inv_Item = dama.GetComponent<Inventory_Item>();
                menuManagerScript.damaBtns[i_id] = inv_Item.selectButton.image;
                inv_Item.selectButton.onClick.AddListener(() => menuManagerScript.selectDama(i_id));
                inv_Item.animator.runtimeAnimatorController = dama_Anims[i_id];
            }
            else if (item.type == itemType.emote)
            {
                int i_id = emotes.itemIds[emotes.itemIds.Count - 1];
                emotesItem[i_id].GetComponent<Shop_Item>().ownItem();
                GameObject emote = Instantiate(inv_Emote, inv_Emote_P);
                emote.SetActive(true);
                Inventory_Item inv_Item = emote.GetComponent<Inventory_Item>();
                menuManagerScript.emotesBtn[i_id] = inv_Item.selectButton.image;
                inv_Item.selectButton.onClick.AddListener(() => menuManagerScript.addEmote(i_id));
                inv_Item.image.sprite = emotes_images[i_id];
            }
            else if (item.type == itemType.coins)
            {
                if (item.addAmount == 20)
                {
                    if (!AdsManager.instance.WatchRewardedVideoAd())
                    {
                        purchasingPopup.gameObject.SetActive(false);
                        loadingPanel.SetActive(false);
                        return;
                    }
                }
                else
                    addCoins(item.addAmount);
            }

            //Subtracting coins when an item is purchased
            if (currencyType == currencyType.VertualCurrency) subractCoins(item.price);
            else
            {
                loadingPanel.SetActive(false);
                purchasingPopup.gameObject.SetActive(false);
            }
                
        }, error =>
        {
        }); ;
    }

    void subractCoins(int price)
    {
        PlayFabClientAPI.SubtractUserVirtualCurrency(new SubtractUserVirtualCurrencyRequest
        {
            VirtualCurrency = "CN",
            Amount = price
        },
        response =>
        {
            updateCoins(response.Balance);
            purchasingPopup.gameObject.SetActive(false);
            loadingPanel.SetActive(false);
        },
        error =>
        {

        });
    }

    public void addCoins(int amount)
    {
        PlayFabClientAPI.AddUserVirtualCurrency(new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = "CN",
            Amount = amount
        },
        response =>
        {
            updateCoins(response.Balance);
            if (response.BalanceChange == 20) remVidTxt.text = PlayerPrefs.GetInt("RewardAdLimit", 5).ToString();
            purchasingPopup.gameObject.SetActive(false);
            loadingPanel.SetActive(false);
        },
        error =>
        {

        });
    }

    public void videoAdFailed()
    {
        loadingPanel.SetActive(false);
    }
}
