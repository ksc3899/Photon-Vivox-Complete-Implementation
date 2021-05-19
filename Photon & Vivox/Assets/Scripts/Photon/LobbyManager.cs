using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public VivoxManager vivoxManager;
    public TMP_InputField usernameInput;

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.NickName = usernameInput.text;
        vivoxManager.LoginUser();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
    }

    private void CreateRoom()
    {
        string roomName = "Room" + UnityEngine.Random.Range(1, 1000).ToString();
        RoomOptions roomOptions = new RoomOptions()
        {
            IsOpen = true,
            IsVisible = true
        };
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CreateRoom();
    }
}
