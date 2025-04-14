using System;
using System.Text;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class ChatScreen : MonoBehaviour
{
    [SerializeField] private ScrollRect chatScroll;
    [SerializeField] private TMP_Text chatText;
    [SerializeField] private TMP_InputField messageInputField;
    [SerializeField] private Button sendButton;


    void Start()
    {
        chatText.text = string.Empty;
        TcpManager.Instance.OnDataReceived += OnReceiveData;
        sendButton.onClick.AddListener(OnSendMessage);
    }

    void OnDestroy()
    {
        TcpManager.Instance.OnDataReceived -= OnReceiveData;
        sendButton.onClick.RemoveListener(OnSendMessage);
    }


    private void UpdateScroll()
    {
        chatScroll.verticalNormalizedPosition = 0f;
    }

    private void OnReceiveData(MessageData data)
    {
        if (TcpManager.Instance.IsServer)
            //TcpManager.Instance.BroadcastData(data);

        //chatText.text += Encoding.UTF8.GetString(data, 0, data.Length) + Environment.NewLine;
        UpdateScroll();
    }

    private void OnSendMessage()
    {
        if (string.IsNullOrEmpty(messageInputField.text))
            return;

        string data = messageInputField.text;

        if (TcpManager.Instance.IsServer)
        {
            chatText.text += messageInputField.text + Environment.NewLine;
            UpdateScroll();
            TcpManager.Instance.BroadcastData(data);
        }
        else
        {
            TcpManager.Instance.SendDataToServer(messageInputField.text);
        }

        messageInputField.text = string.Empty;
    }
}