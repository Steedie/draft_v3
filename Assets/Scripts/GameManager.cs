using System.Collections;
using System.Collections.Generic;
using Unity.Services.Relay;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public static NetPlayer NetPlayer;
    public static GameFlow GameFlow;
    public static SetupManager SetupManager;
    public static CaptainManager CaptainManager;
    public static BidManager BidManager;

    public string joinCode;

    private void Awake()
    {
        Instance = this;

        GameFlow = GetComponentInChildren<GameFlow>();
        SetupManager = GetComponentInChildren<SetupManager>();
        CaptainManager = GetComponentInChildren<CaptainManager>();
        BidManager = GetComponentInChildren<BidManager>();
    }

    public void AssignLocalPlayer(NetPlayer netPlayer)
    {
        NetPlayer = netPlayer;

        GameFlow.ShowScreen(GameFlow.Screens.MainDraft);
    }

    public void CopyJoinCode()
    {
        GUIUtility.systemCopyBuffer = joinCode;
    }
}
