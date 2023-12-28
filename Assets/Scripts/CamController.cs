using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
    public static CamController instance;

    private void Awake()
    {
        instance = this;
    }

    public void OnClickRotateLeft()
    {
        Vector3 rot = transform.eulerAngles;
        rot.z -= 90;
        transform.eulerAngles = rot;
    }
    public void OnClickRotateRight()
    {
        Vector3 rot = transform.eulerAngles;
        rot.z += 90;
        transform.eulerAngles = rot;
    }
}
