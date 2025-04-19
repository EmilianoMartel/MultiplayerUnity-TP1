using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Canvas _networkSetupCanvas;
    [SerializeField] private Canvas _messageSetupCanvas;
    [SerializeField] private Canvas _serverSetupCanvas;

    private void OnEnable()
    {
        TcpManager.Instance.GoToChatScreen += OnChatScreen;
        TcpManager.Instance.GoToServerScren += OnServerScreen;
    }

    private void OnDisable()
    {
        TcpManager.Instance.GoToChatScreen -= OnChatScreen;
        TcpManager.Instance.GoToServerScren -= OnServerScreen;
    }

    private void OnChatScreen()
    {
        _networkSetupCanvas.gameObject.SetActive(false);
        _messageSetupCanvas.gameObject.SetActive(true);
    }

    private void OnServerScreen()
    {
        _networkSetupCanvas.gameObject.SetActive(false);
        _serverSetupCanvas.gameObject.SetActive(true);
    }
}