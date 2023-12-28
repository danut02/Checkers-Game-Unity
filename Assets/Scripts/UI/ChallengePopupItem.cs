using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class ChallengePopupItem : MonoBehaviour
{
    public string PlayerWhoChallenged = string.Empty;
    [SerializeField] private TMP_Text challangeText;

    public Action<string> OnAccept;
    public Action<string> OnDecline;

    public void ShowChallangePopup(string ch)
    {
        PlayerWhoChallenged = ch;
        challangeText.text = ch + ": Wanna Play!";

        gameObject.SetActive(true);
    }

    public void OnClickAccept()
    {
        OnAccept?.Invoke(PlayerWhoChallenged);
        gameObject.SetActive(false);
    }
    public void OnClickDecline()
    {
        OnDecline?.Invoke(PlayerWhoChallenged);
        gameObject.SetActive(false);
    }
}
