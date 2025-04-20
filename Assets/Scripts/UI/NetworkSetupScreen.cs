using System.Net;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;

public enum Role
{
    None,
    Client,
    Server,
    ServerClient
}

public class NetworkSetupScreen : MonoBehaviour
{
    [SerializeField] private TMP_InputField _name;
    [SerializeField] private TMP_InputField _serverIpField;
    [SerializeField] private TMP_InputField _serverPortField;
    [SerializeField] private TMP_Dropdown _connectionBehaviour;
    [SerializeField] private TMP_Dropdown _connectionType;
    [SerializeField] private Button _startSeverButton;

    private Role _selectedRole = Role.None;

    private void OnEnable()
    {
        _connectionBehaviour.onValueChanged.AddListener(OnDropdownValueChanged);
        _startSeverButton.onClick.AddListener(OnStartConnection);
    }

    private void OnDisable()
    {
        _connectionBehaviour.onValueChanged.RemoveListener(OnDropdownValueChanged);
        _startSeverButton.onClick.RemoveListener(OnStartConnection);
    }

    private void Awake()
    {
        PopulateDropdown();
    }

    private void OnDestroy()
    {
        _startSeverButton.onClick.RemoveListener(OnStartConnection);
    }

    private void PopulateDropdown()
    {
        _connectionBehaviour.ClearOptions();
        string[] roleNames = Enum.GetNames(typeof(Role));
        _connectionBehaviour.AddOptions(new System.Collections.Generic.List<string>(roleNames));
    }

    private void OnStartConnection()
    {
        int port = Convert.ToInt32(_serverPortField.text);
        IPAddress ipAddress = (_selectedRole == Role.Server || _selectedRole == Role.ServerClient)? GetLocalIPAddress() : IPAddress.Parse(_serverIpField.text);

        TcpManager.Instance.TcpSetup(_selectedRole, ipAddress, port, _name.text, _connectionType.itemText.text);
    }

    private IPAddress GetLocalIPAddress()
    {
        return IPAddress.Any;
    }

    private void OnDropdownValueChanged(int index)
    {
        _selectedRole = (Role)index;
        _serverIpField.interactable = !(_selectedRole == Role.Server || _selectedRole == Role.ServerClient);
    }
}