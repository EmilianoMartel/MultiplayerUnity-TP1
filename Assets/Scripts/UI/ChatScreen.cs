using System;
using System.Text;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;

public class ChatScreen : MonoBehaviour
{
    [SerializeField] private ScrollRect _chatScroll;
    [SerializeField] private Transform _contentParent;
    [SerializeField] private Message _chatText;
    [SerializeField] private TMP_InputField _messageInputField;
    [SerializeField] private Button _sendButton;

    private List<Message> _messages = new();

    private int IDMessageToRespond = -1;

    private void Start()
    {
        TcpManager.Instance.OnDataReceived += OnReceiveData;

        _sendButton.onClick.AddListener(OnSendMessage);
    }

    private void OnDestroy()
    {
        TcpManager.Instance.OnDataReceived -= OnReceiveData;

        _sendButton.onClick.RemoveListener(OnSendMessage);

        for (int i = 0; i < _messages.Count; i++)
        {
            _messages[i].SelectedMessageToRespond -= SelectedMessage;
        }
    }

    private void UpdateScroll()
    {
        _chatScroll.verticalNormalizedPosition = 0f;
    }

    private void OnReceiveData(byte[] data)
    {
        MessageData message = MessageConverter.BytesToMessage(data);

        Message temp = Instantiate(_chatText, _contentParent);
        temp.SetMessage(message);
        _messages.Add(temp);
        temp.SelectedMessageToRespond += SelectedMessage;
        UpdateScroll();
    }

    private void OnSendMessage()
    {
        if (string.IsNullOrEmpty(_messageInputField.text))
            return;

        MessageData msgData = new MessageData
        {
            ClientID = TcpManager.Instance.NameID,
            Message = _messageInputField.text,
            MessageToRespondID = IDMessageToRespond
        };

        byte[] data = MessageConverter.MessageToBytes(msgData);

        TcpManager.Instance.SendDataToServer(data);

        _messageInputField.text = string.Empty;
        IDMessageToRespond = -1;
    }

    private void SelectedMessage(int ID)
    {
        IDMessageToRespond = ID;
    }
}