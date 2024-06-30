using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using System.Net;
using TMPro;

public class GameFlow : MonoBehaviour
{
    [SerializeField] private List<GameObject> screenMainMenu;
    [SerializeField] private List<GameObject> screenMainDraft;
    [SerializeField] private List<GameObject> screenDraftSetup;

    [SerializeField] private TMP_InputField captainNameInputField;
    [SerializeField] private TMP_InputField joinCodeInputField;

    private string captainName;

    public enum Screens
    {
        MainMenu = 1,
        MainDraft = 2,
        DraftSetup = 3
    }

    private void Awake()
    {
        if (PlayerPrefs.HasKey("CaptainName"))
        {
            captainName = PlayerPrefs.GetString("CaptainName");
            captainNameInputField.text = captainName;
        }

        ShowScreen(Screens.MainMenu);
    }

    public void UpdateCaptainName(string name)
    {
        captainName = name;
        PlayerPrefs.SetString("CaptainName", captainName);
    }

    public string GetCaptainName()
    {
        return captainName;
    }

    public void ShowScreen(Screens screen)
    {
        HideAllScreens();

        switch (screen)
        {
            case Screens.MainMenu:
                foreach (GameObject g in screenMainMenu)
                    g.SetActive(true);
                break;
            case Screens.MainDraft:
                foreach (GameObject g in screenMainDraft)
                    g.SetActive(true);
                break;
            case Screens.DraftSetup:
                foreach (GameObject g in screenDraftSetup)
                    g.SetActive(true);
                break;
        }
    }

    private void HideAllScreens()
    {
        foreach (GameObject g in screenMainMenu)
        {
            g.SetActive(false);
        }
        foreach (GameObject g in screenMainDraft)
        {
            g.SetActive(false);
        }
        foreach (GameObject g in screenDraftSetup)
        {
            g.SetActive(false);
        }
    }

    public async void StartHost()
    {
        await StartHostWithRelay();
    }

    public async void StartClient()
    {
        await StartClientWithRelay();
    }

    public async Task<string> StartHostWithRelay(int maxConnections = 20)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        GameManager.Instance.joinCode = joinCode;
        GUIUtility.systemCopyBuffer = joinCode;

        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    public async Task<bool> StartClientWithRelay()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        string joinCode = joinCodeInputField.text;

        Debug.Log($"Joining Relay with code: [{joinCode}]");
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }
}
