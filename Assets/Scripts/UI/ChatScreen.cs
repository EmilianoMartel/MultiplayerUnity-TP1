using System;
using System.Text;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class ChatScreen : MonoBehaviour
{
    [SerializeField] private ScrollRect _chatScroll;
    [SerializeField] private Transform _contentParent;
    [SerializeField] private Message _chatText;
    [SerializeField] private TMP_InputField _messageInputField;
    [SerializeField] private Button _sendButton;

    private void Start()
    {
        TcpManager.Instance.OnDataReceived += OnReceiveData;

        _sendButton.onClick.AddListener(OnSendMessage);
    }

    private void OnDestroy()
    {
        TcpManager.Instance.OnDataReceived -= OnReceiveData;

        _sendButton.onClick.RemoveListener(OnSendMessage);
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

        UpdateScroll();
    }

    private void OnSendMessage()
    {
        if (string.IsNullOrEmpty(_messageInputField.text))
            return;

        MessageData msgData = new MessageData
        {
            ClientID = TcpManager.Instance.NameID,
            Message = _messageInputField.text
        };

        byte[] data = MessageConverter.MessageToBytes(msgData);


        if (TcpManager.Instance.IsServer)
        {
            Message temp = Instantiate(_chatText, _contentParent);
            temp.SetMessage(msgData);
            UpdateScroll();
        }
        else
        {
            TcpManager.Instance.SendDataToServer(data);
        }

        _messageInputField.text = string.Empty;
    }
}