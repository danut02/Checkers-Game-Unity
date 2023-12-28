using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SavedMatchItem : MonoBehaviour
{
    public TMP_Text matchNameText;
    public void ShowMatchItem(string matchName, string type, string result)
    {
        matchNameText.text = string.Format("{0} {1} {2}", matchName, type, result + " View");
        gameObject.SetActive(true);
    }
}
