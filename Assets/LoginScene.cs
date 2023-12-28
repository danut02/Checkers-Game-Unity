using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoginScene : MonoBehaviour
{
    public GameObject _signInPanel;
    public GameObject _playfabConnectingScreen;
    public GameObject _displayNamePanel;
    public GameObject _loadingScreen;
    public GameObject _loginError;
    public TMP_InputField _displayInputField;
    private PlayfabManager playfabScript;

    void Start()
    {
        playfabScript = PlayfabManager.instance;
        playfabScript.signInPanel = _signInPanel;
        playfabScript.playfabConnectingScreen = _playfabConnectingScreen;
        playfabScript.displayNamePanel = _displayNamePanel;
        playfabScript.loadingScreen = _loadingScreen;
        playfabScript.loginError = _loginError;
        playfabScript.displayInputField = _displayInputField;
        PlayfabManager.instance.autoLogin();
    }
    public void OnClickSignIn()
    {
        playfabScript.loginWithGoogle();
    }
    public void OnClickGuest()
    {
        playfabScript.LoginAsGuest();
    }
    public void OnUpdateName()
    {
        playfabScript.updateDisplayName();
    }
    public void OnClickQuit()
    {
        Application.Quit();
    }
}
