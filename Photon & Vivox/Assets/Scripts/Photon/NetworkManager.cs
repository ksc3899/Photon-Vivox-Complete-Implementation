using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public Button loginButton;
    public TMP_InputField usernameInput;

    public void Login()
    {
        if (usernameInput.text == "" || usernameInput.text == null)
            return;

        loginButton.interactable = false;
        loginButton.GetComponentInChildren<TextMeshProUGUI>().text = "Logging In...";
        PhotonNetwork.ConnectUsingSettings();
    }

    /*public override void OnDisconnected(DisconnectCause cause)
    {
        loginButton.interactable = true;
        loginButton.GetComponentInChildren<TextMeshProUGUI>().text = "Login";
    }*/

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected To Photon Servers!");
    }
}
