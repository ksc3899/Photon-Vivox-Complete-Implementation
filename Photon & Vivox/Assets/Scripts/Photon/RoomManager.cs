using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public VivoxManager vivoxManager;
    public GameObject loginPage;
    public GameObject characterMenu;

    public override void OnJoinedRoom()
    {
        GameObject g = PhotonNetwork.Instantiate("Character", new Vector3(-5f, -3.25f, -45f), Quaternion.identity, 0);
        g.GetComponent<PlayerManager>().SetName(PhotonNetwork.NickName);

        /*if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate("Dolphin", Vector3.zero, Quaternion.identity, 0);
        }*/

        loginPage.SetActive(false);
        characterMenu.SetActive(true);
        
        vivoxManager.JoinChannel(PhotonNetwork.CurrentRoom.Name);
    }

    /*public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if(newMasterClient.IsLocal)
        {
            PhotonNetwork.Instantiate("Dolphin", Vector3.zero, Quaternion.identity, 0);
        }
    }*/
}
