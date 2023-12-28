using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTypeDropDown : MonoBehaviour
{
    public TMPro.TMP_Dropdown roomTypeInput;
    public GameObject roomIdInput;

    public void roomTypeChanged()
    {
        if (roomTypeInput.value == 0) roomIdInput.SetActive(false);
        else roomIdInput.SetActive(true);
    }
}
