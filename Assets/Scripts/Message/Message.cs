using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Message : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _message;

    private int _messageDataID;

    public Action<int> SelectedMessageToRespond;

    public void SetMessage(MessageData data)
    {
        _name.text = data.ClientID;
        _message.text = data.Message;
        _messageDataID = data.MessageID;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SelectedMessageToRespond?.Invoke(_messageDataID);
    }
}
