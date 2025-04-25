using System.Runtime.InteropServices;
using System;
using UnityEngine;
using System.Text;

public class MessageConverter
{
    public static byte[] MessageToBytes(MessageData message)
    {
        string msg = message.ClientID + "//*//" + message.Message + "//*//" + message.MessageToRespondID + "//*//" + message.MessageID.ToString();
        Debug.Log(msg);
        byte[] data = Encoding.UTF8.GetBytes(msg);

        return data;
    }

    public static MessageData BytesToMessage(byte[] data)
    {
        string msg = Encoding.UTF8.GetString(data);

        string[] parts = msg.Split(new[] { "//*//" }, StringSplitOptions.None);

        int messageToRespondID = -1;
        int messageID = -1;

        if (parts.Length > 2)
            int.TryParse(parts[2], out messageToRespondID);

        if (parts.Length > 3)
            int.TryParse(parts[3], out messageID);

        MessageData message = new MessageData
        {
            ClientID = parts.Length > 0 ? parts[0] : "",
            Message = parts.Length > 1 ? parts[1] : "",
            MessageToRespondID = messageToRespondID,
            MessageID = messageID,
        };

        return message;
    }
}