using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SaveLastState_Toggle : MonoBehaviour
{
    [SerializeField] private string KEY = "";
    private Toggle Toggle;

    private void Awake()
    {
        Toggle = GetComponent<Toggle>();
        Toggle.onValueChanged.AddListener(SaveToggleState);

        Toggle.isOn = PlayerPrefs.GetInt(KEY, 0) == 1 ? true : false;
    }

    private void SaveToggleState(bool isOn)
    {
        PlayerPrefs.SetInt(KEY, isOn == true ? 1 : 0);
    }

    private void OnDestroy()
    {
        Toggle.onValueChanged.RemoveListener(SaveToggleState);
    }
}
