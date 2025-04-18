using System.Net;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;

public class NetworkSetupScreen : MonoBehaviour
{
    [SerializeField] private TMP_InputField _name;
    [SerializeField] private TMP_InputField _serverIpField;
    [SerializeField] private TMP_InputField _serverPortField;
    [SerializeField] private TMP_Dropdown _connectionTypeSelector;
    [SerializeField] private Button _startSeverButton;

    private Role _selectedRole = Role.None;

    private void OnEnable()
    {
        _connectionTypeSelector.onValueChanged.AddListener(OnDropdownValueChanged);
        _startSeverButton.onClick.AddListener(OnStartTcp);
    }

    private void OnDisable()
    {
        _connectionTypeSelector.onValueChanged.RemoveListener(OnDropdownValueChanged);
        _startSeverButton.onClick.RemoveListener(OnStartTcp);
    }

    private void Awake()
    {
        PopulateDropdown();
    }

    private void OnDestroy()
    {
        _startSeverButton.onClick.RemoveListener(OnStartTcp);
    }

    private void PopulateDropdown()
    {
        _connectionTypeSelector.ClearOptions();
        string[] roleNames = Enum.GetNames(typeof(Role));
        _connectionTypeSelector.AddOptions(new System.Collections.Generic.List<string>(roleNames));
    }

    private void OnStartTcp()
    {
        int port = Convert.ToInt32(_serverPortField.text);
        IPAddress ipAddress = (_selectedRole == Role.Server || _selectedRole == Role.ServerClient)? GetLocalIPAddress() : IPAddress.Parse(_serverIpField.text);

        switch (_selectedRole)
        {
            case Role.None:
                break;
            case Role.Client:
                TcpManager.Instance.TcpSetup(_selectedRole, ipAddress, port, _name.text);
                break;
            case Role.Server:
                TcpManager.Instance.TcpSetup(_selectedRole, ipAddress, port, _name.text);
                break;
            case Role.ServerClient:
                TcpManager.Instance.TcpSetup(_selectedRole, ipAddress, port, _name.text);
                break;
            default:
                break;
        }
    }

    private IPAddress GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip;
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    private void OnDropdownValueChanged(int index)
    {
        _selectedRole = (Role)index;
        _serverIpField.interactable = !(_selectedRole == Role.Server || _selectedRole == Role.ServerClient);
    }
}