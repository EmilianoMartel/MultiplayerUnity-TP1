using System;

[Serializable]
public struct MessageData
{
    public int MessageID;
    public string ClientID;
    public string Message;
    public int MessageToRespondID;
}