using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Message : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _message;
    [SerializeField] private Image _background;

    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _unselectedColor;

    private int _messageDataID;

    public int ID {  get { return _messageDataID; } }

    public Action<Message> SelectedMessageToRespond;

    public void SetMessage(MessageData data)
    {
        _name.text = data.ClientID;
        _message.text = data.Message;
        _messageDataID = data.MessageID;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"{name} Message was selected");
        SelectedMessageToRespond?.Invoke(this);
    }

    public void Selected()
    {
        _background.color = _selectedColor;
    }

    public void Unselected()
    {
        _background.color= _unselectedColor;
    }
}
