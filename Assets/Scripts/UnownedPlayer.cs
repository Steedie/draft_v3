using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UnownedPlayer : MonoBehaviour
{
    [SerializeField] private GameObject selectButton;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private GameObject deleteButton;

    private NetTier netTier;
    private NetworkObject netDraftPlayer;

    public void Setup(ulong netTierId, ulong netDraftPlayerId, string playerName)
    {
        playerNameText.text = playerName;

        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netTierId, out NetworkObject tierNetworkObject);
        netTier = tierNetworkObject.GetComponent<NetTier>();

        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netDraftPlayerId, out netDraftPlayer);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            SetSelectButtonActive(true);
            SetDeleteButtonActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            SetSelectButtonActive(false);
            SetDeleteButtonActive(false);
        }
    }

    // Button to let the Host force start bidding on player
    private void SetSelectButtonActive(bool active)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            selectButton.SetActive(active);
        }
    }

    private void SetDeleteButtonActive(bool active)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            deleteButton.SetActive(active);
        }
    }

    public void DeletePlayer() // Button
    {
        if (netDraftPlayer.IsSpawned)
            netDraftPlayer.Despawn();
    }

    public void SelectToBidButton() // Button
    {
        GameManager.BidManager.StartBidOnDraftPlayerRpc(netDraftPlayer.NetworkObjectId);
    }
}
