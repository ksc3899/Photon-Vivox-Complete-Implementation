using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VivoxUnity;
using UnityEngine.UI;

public class VivoxLocalVoiceManager : MonoBehaviour
{
    public Sprite mute;
    public Sprite unmute;
    public Image muteButton;
    
    VivoxUnity.Client client = new Client();

    public void LocalMuteSelf()
    {
        if(client.AudioInputDevices.Muted)
        {
            client.AudioInputDevices.Muted = false;
            muteButton.sprite = mute;
        }
        else
        {
            client.AudioInputDevices.Muted = true;
            muteButton.sprite = unmute;
        }
    }
}
