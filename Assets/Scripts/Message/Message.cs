using TMPro;
using UnityEngine;

public class Message : MonoBehaviour
{
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _message;

    public void SetMessage(MessageData data)
    {
        _name.text = data.ClientID;
        _message.text = data.Message;
    }
}
