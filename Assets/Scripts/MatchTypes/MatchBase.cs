using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MatchBase : MonoBehaviourPunCallbacks
{
    public WaitingLobby waitingLobbyScript;
    public GameObject matchFoundDetails;
    public Sprite[] ranks;

    [HideInInspector] public bool isStartMatch = false;
}
