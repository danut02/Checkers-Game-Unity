using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using PlayFab;
using PlayFab.ClientModels;

public class FriendUI : MonoBehaviour
{
    public Action<string> OnSendMessage;
    public Action<FriendInfo> OnUnfriend;

    private string friendId;
    public FriendInfo friendInfo;
    [SerializeField] private TMP_Text nameText;


    public void UpdateInfo(FriendInfo info)
    {
        friendInfo = info;
        UpdateName(info.TitleDisplayName);
    }
    public void UpdateName(string name)
    {
        friendId = name;
        nameText.text = name;
    }


    public void OnClickInvite()
    {
        OnSendMessage?.Invoke(friendId);
    }
    public void OnClickUnFriend()
    {
        OnUnfriend?.Invoke(friendInfo);
    }
}
