using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VivoxUnity;

public class VivoxServerCredentials
{
    public Uri server = new Uri("https://mt1s.www.vivox.com/api2");
    public string issuer = "caarya6953-ca10-dev";
    public string domain = "mt1s.vivox.com";
    public string tokenKey = "haze536";
    public VivoxUnity.Client client;
    public TimeSpan timeSpan = TimeSpan.FromSeconds(90);
    public ILoginSession loginSession;
    public IChannelSession channelSession;
}
