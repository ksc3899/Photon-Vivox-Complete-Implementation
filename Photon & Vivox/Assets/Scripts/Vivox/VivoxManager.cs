using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VivoxUnity;
using System.ComponentModel;
using System;
using Photon.Pun;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class VivoxManager : MonoBehaviour
{
    [HideInInspector] public VivoxServerCredentials serverCredentials = new VivoxServerCredentials();

    private void Awake()
    {
        serverCredentials.client = new Client();
        serverCredentials.client.Uninitialize();
        serverCredentials.client.Initialize();
        DontDestroyOnLoad(this);
    }

    private void OnApplicationQuit()
    {
        Client.Cleanup();
        serverCredentials.client.Uninitialize();
        serverCredentials.client = null;
    }

    #region Binding Functions
    private void BindLoginCallbackListeners(bool bind, ILoginSession loginSess)
    {
        if (bind)
        {
            loginSess.PropertyChanged += LoginStatus;
        }
        else
        {
            loginSess.PropertyChanged -= LoginStatus;
        }
    }

    private void BindAudioStatusCallbackListeners(bool bind, IChannelSession channelSess)
    {
        if (bind)
        {
            channelSess.PropertyChanged += AudioStatus;
        }
        else
        {
            channelSess.PropertyChanged -= AudioStatus;
        }
    }

    private void BindUserCallbackListeners(bool bind, IChannelSession channelSess)
    {
        if (bind)
        {
            channelSess.Participants.AfterKeyAdded += OnParticipantAdded;
            channelSess.Participants.BeforeKeyRemoved += OnParticipantRemoved;
        }
        else
        {
            channelSess.Participants.AfterKeyAdded -= OnParticipantAdded;
            channelSess.Participants.BeforeKeyRemoved -= OnParticipantRemoved;
        }
    }
    #endregion

    #region Login Methods
    public void LoginUser()
    {
        Login(PhotonNetwork.NickName);
    }

    private void Login(string userName)
    {
        AccountId accountId = new AccountId(serverCredentials.issuer, userName, serverCredentials.domain);
        serverCredentials.loginSession = serverCredentials.client.GetLoginSession(accountId);
        BindLoginCallbackListeners(true, serverCredentials.loginSession);
        serverCredentials.loginSession.BeginLogin(serverCredentials.server, serverCredentials.loginSession.GetLoginToken(serverCredentials.tokenKey, serverCredentials.timeSpan), ar =>
        {
            try
            {
                serverCredentials.loginSession.EndLogin(ar);
            }
            catch (Exception e)
            {
                BindLoginCallbackListeners(false, serverCredentials.loginSession);
                Debug.Log(e.Message);
            }
        });
    }

    private void LoginStatus(object sender, PropertyChangedEventArgs loginArgs)
    {
        var source = (ILoginSession)sender;

        switch (source.State)
        {
            case LoginState.LoggingIn:
                Debug.Log("Logging In");
                break;
            case LoginState.LoggedIn:
                Debug.Log($"Logged In {serverCredentials.loginSession.LoginSessionId.Name}");
#if PLATFORM_ANDROID
                if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
                {
                    Permission.RequestUserPermission(Permission.Microphone);
                }
#endif
                PhotonNetwork.JoinRandomRoom();
                break;
        }
    }

    public void Logout()
    {
        serverCredentials.loginSession.Logout();
        BindLoginCallbackListeners(false, serverCredentials.loginSession);
    }
    #endregion

    #region Join Channel Methods
    public void JoinChannel(string channelName)
    {
        ChannelId channelId = new ChannelId(serverCredentials.issuer, channelName, serverCredentials.domain, ChannelType.NonPositional);
        serverCredentials.channelSession = serverCredentials.loginSession.GetChannelSession(channelId);
        
        BindUserCallbackListeners(true, serverCredentials.channelSession);
        BindAudioStatusCallbackListeners(true, serverCredentials.channelSession);
        
        serverCredentials.channelSession.BeginConnect(true, false, true, serverCredentials.channelSession.GetConnectToken(serverCredentials.tokenKey, serverCredentials.timeSpan), ar =>
        {
            try
            {
                serverCredentials.channelSession.EndConnect(ar);
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
                BindUserCallbackListeners(false, serverCredentials.channelSession);
                BindAudioStatusCallbackListeners(false, serverCredentials.channelSession);
            }
        });
    }

    private void AudioStatus(object sender, PropertyChangedEventArgs audioArgs)
    {
        IChannelSession source = (IChannelSession)sender;

        switch(source.AudioState)
        {
            case ConnectionState.Connecting:
                Debug.Log("Audio Channel Connecting");
                break;
            case ConnectionState.Connected:
                Debug.Log("Audio Channel Connected");
                StartCoroutine(RemoteMuteUser(true, serverCredentials.channelSession));
                break;
            case ConnectionState.Disconnecting:
                Debug.Log("Audio Channel Disconnecting");
                break;
            case ConnectionState.Disconnected:
                Debug.Log("Audio Channel Disconnected");
                break;
        }
    }

    public void LeaveChannel(IChannelSession channelToLeave)
    {
        channelToLeave.Disconnect();
        serverCredentials.loginSession.DeleteChannelSession(new ChannelId(serverCredentials.issuer, "Channel1", serverCredentials.domain));
    }
    #endregion

    #region User Callbacks
    private void OnParticipantAdded(object sender, KeyEventArg<string> participantArgs)
    {
        var source = (VivoxUnity.IReadOnlyDictionary<string, IParticipant>)sender;
        var user = source[participantArgs.Key];
        //Send in-game notification
        Debug.Log($"{user.Account.Name} has joined the channel");
        StartCoroutine(RemoteMuteUser(false, serverCredentials.channelSession, user.Account.DisplayName));
    }

    private void OnParticipantRemoved(object sender, KeyEventArg<string> participantArgs)
    {
        var source = (VivoxUnity.IReadOnlyDictionary<string, IParticipant>)sender;

        var user = source[participantArgs.Key];
        //Send in-game notification
        Debug.Log($"{user.Account.Name} has left the channel");
    }
    #endregion

    #region Remote Mute On Joining
    private IEnumerator RemoteMuteUser(bool justJoinedChannel, IChannelSession channelSession, string userToMute = "")
    {
        if (justJoinedChannel)
        {
            foreach (IParticipant p in serverCredentials.channelSession.Participants)
            {
                var participants = serverCredentials.channelSession.Participants;
                string participantToMute = $"sip:.{serverCredentials.issuer}.{p.Account.DisplayName}.@{serverCredentials.domain}";
                if (!participants[participantToMute].IsSelf)
                {
                    participants[participantToMute].LocalMute = true;
                }
            }
        }
        else
        {
            var participants = channelSession.Participants;
            string participantToMute = $"sip:.{channelSession.Channel.Issuer}.{userToMute}.@{channelSession.Channel.Domain}";

            if (!participants[participantToMute].InAudio)
            {
                yield return new WaitUntil(() => participants[participantToMute].InAudio);
            }

            if (participants[participantToMute].InAudio && !participants[participantToMute].IsSelf)
            {
                if (participants[participantToMute].LocalMute)
                {
                    participants[participantToMute].LocalMute = false;
                }
                else
                {
                    participants[participantToMute].LocalMute = true;
                }
            }
        }
    }
    #endregion
}
