using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HostPlayer : MonoBehaviour
{
    private CamController Camera_Script;
    private InGameUI InGameUI_Script;

    void Start()
    {
        Camera_Script = CamController.instance;
        InGameUI_Script = InGameUI.instance;
    }
}
