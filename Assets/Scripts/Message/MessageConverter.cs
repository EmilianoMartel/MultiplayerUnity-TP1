using System.Runtime.InteropServices;
using System;
using UnityEngine;
using System.Text;

public class MessageConverter
{
    public static byte[] MessageToBytes(MessageData message)
    {
        string msg = message.ClientID + "//*//" + message.Message;
        Debug.Log(msg);
        byte[] data = Encoding.UTF8.GetBytes(msg);

        return data;
    }

    public static MessageData BytesToMessage(byte[] data)
    {
        string msg = Encoding.UTF8.GetString(data);

        string[] parts = msg.Split(new[] { "//*//" }, StringSplitOptions.None);

        MessageData message = new MessageData
        {
            ClientID = parts.Length > 0 ? parts[0] : "",
            Message = parts.Length > 1 ? parts[1] : ""
        };

        return message;
    }
}