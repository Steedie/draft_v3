using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class TieredPlayerList : MonoBehaviour
{
    private ulong tierNetworkObjectId;

    [SerializeField] private TMP_Text tierNameText;
    [SerializeField] private TMP_Text minBidText;
    [SerializeField] private Transform content;

    [SerializeField] private UnownedPlayer unownedPlayerPrefab;

    public void SetId(ulong id)
    {
        tierNetworkObjectId = id;
    }

    public ulong GetNetTierId()
    {
        return tierNetworkObjectId;
    }

    public void SetTierName(string value)
    {
        tierNameText.text = $"{value} Tier";
    }

    public void SetMinBid(int value)
    {
        minBidText.text = $"Minimum Starting Bid: {value.ToString("C0", CultureInfo.CreateSpecificCulture("en-US"))}";
    }

    public void SetTierNameColor(Color value)
    {
        tierNameText.color = value;
    }

    public UnownedPlayer AddUnownedDraftPlayer(ulong draftPlayerId, string playerName)
    {
        UnownedPlayer player = Instantiate(unownedPlayerPrefab, content);
        player.Setup(tierNetworkObjectId, draftPlayerId, playerName);
        return player;
    }

    public void DeleteAllPlayers()
    {
        foreach(Transform t in content)
        {
            t.GetComponent<UnownedPlayer>().DeletePlayer();
        }
    }
}
