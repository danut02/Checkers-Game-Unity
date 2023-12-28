using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using System.Threading.Tasks;

public class PlayfabCustomRequests
{
    public static async Task AddVirtualCurrency
        (int CoinsAmount, Action<ModifyUserVirtualCurrencyResult> OnSuccesAction = null, Action<PlayFabError> OnErrorAction = null)
    {
        await Task.Delay(0);
        PlayFabClientAPI.AddUserVirtualCurrency(new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = "CN",
            Amount = CoinsAmount
        },
        OnSuccesAction,
        OnErrorAction);
    }
    public static async Task GetUserInventory(Action<GetUserInventoryResult> OnSuccesAction = null, Action<PlayFabError> OnErrorAction = null)
    {
        await Task.Delay(0);
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnSuccesAction, OnErrorAction);
    }

    public static async Task GetUserCustomData 
        (Action<GetUserDataResult> OnSuccesAction = null, Action<PlayFabError> OnErrorAction = null)
    {
        await Task.Delay(0);
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnSuccesAction, OnErrorAction);
    }

    public static async Task UpdateUserData(string key, string value, Action<UpdateUserDataResult> OnUserDataUpdated, Action<PlayFabError> OnError = null)
    {
        await Task.Delay(0);
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {
                    key, value
                }
            }
        },
        OnUserDataUpdated,
        OnError);
    }
}
