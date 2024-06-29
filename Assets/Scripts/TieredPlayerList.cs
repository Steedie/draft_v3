using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class TieredPlayerList : MonoBehaviour
{
    private ulong tierNetworkObjectId;

    [SerializeField] private TMP_Text tierNameText;
    [SerializeField] private TMP_Text minBidText;
    [SerializeField] private Transform content;

    public void SetId(ulong id)
    {
        tierNetworkObjectId = id;
        Debug.Log($"SetupTier tierNetworkObjectId set to: {tierNetworkObjectId}");
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
}
