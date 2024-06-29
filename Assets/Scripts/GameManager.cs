using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public static NetPlayer NetPlayer;
    public static GameFlow GameFlow;
    public static SetupManager SetupManager;

    private void Awake()
    {
        Instance = this;

        GameFlow = GetComponentInChildren<GameFlow>();
        SetupManager = GetComponentInChildren<SetupManager>();
    }

    public void AssignLocalPlayer(NetPlayer netPlayer)
    {
        NetPlayer = netPlayer;

        GameFlow.ShowScreen(GameFlow.Screens.MainDraft);
    }
}
