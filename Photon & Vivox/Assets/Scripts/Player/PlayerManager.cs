using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VivoxUnity;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public Sprite mute;
    public Sprite unmute;
    public Image muteButton;
    public TextMeshProUGUI usernameText;

    private VivoxManager vivoxManager;

    private void Awake()
    {
        if (GetComponent<PhotonView>().IsMine)
        {
            FindObjectOfType<GameManager>().player = this;
        }
    }

    private void Start()
    {
        vivoxManager = GameObject.FindGameObjectWithTag("Vivox").GetComponent<VivoxManager>();
    }

    #region RPCs
    public void SetName(string name)
    {
        GetComponent<PhotonView>().RPC("UpdateName", RpcTarget.AllBuffered, name);
    }

    [PunRPC]
    public void UpdateName(string name)
    {
        usernameText.text = name;
    }

    public void SetVisuals(int i)
    {
        GetComponent<PhotonView>().RPC("UpdateVisuals", RpcTarget.AllBuffered, i);
    }

    [PunRPC]
    public void UpdateVisuals(int i)
    {
        this.transform.GetChild(i).gameObject.SetActive(true);
        if (this.GetComponent<PlayerController>() != null)
        {
            this.GetComponent<PlayerController>().animator = this.transform.GetChild(i).GetComponent<Animator>();
        }
    }
    #endregion

    public void ToggleRemoteUserAudio()
    {
        IChannelSession channelSession = vivoxManager.serverCredentials.channelSession;
        var participants = channelSession.Participants;
        string participantToMute = $"sip:.{channelSession.Channel.Issuer}.{usernameText.text}.@{channelSession.Channel.Domain}";
        Debug.Log(participants[participantToMute].InAudio);
        if (participants[participantToMute].InAudio && !participants[participantToMute].IsSelf)
        {
            if (participants[participantToMute].LocalMute)
            {
                participants[participantToMute].LocalMute = false;
                muteButton.sprite = mute;
            }
            else
            {
                participants[participantToMute].LocalMute = true;
                muteButton.sprite = unmute;
            }
        }
    }
}
