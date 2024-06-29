using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SetupTier : MonoBehaviour
{
    private ulong tierNetworkObjectId;

    [SerializeField] private TMP_InputField tierNameInputField;
    [SerializeField] private TMP_InputField tierColorInputField;
    [SerializeField] private TMP_InputField minimumBidInputField;
    [SerializeField] private TMP_InputField playersInputField;

    [SerializeField] private Image hexColorImage;

    public void SetId(ulong id)
    {
        tierNetworkObjectId = id;
        Debug.Log($"SetupTier tierNetworkObjectId set to: {tierNetworkObjectId}");
    }

    public void DestroyTier()
    {
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(tierNetworkObjectId, out NetworkObject netTierObject);
        netTierObject.Despawn();
        Destroy(gameObject);
    }

    public void UpdateImageColor(string value)
    {
        ColorUtility.TryParseHtmlString(value, out Color targetColor);
        hexColorImage.color = targetColor;
    }

    public void SaveTierSettings()
    {
        if (tierColorInputField.text.Length == 0)
            tierColorInputField.text = $"#ffffff";

        if (minimumBidInputField.text.Length == 0)
            minimumBidInputField.text = 0.ToString();

        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(tierNetworkObjectId, out NetworkObject tierNetworkObject);
        NetTier netTier = tierNetworkObject.GetComponent<NetTier>();
        netTier.m_TierName.Value = tierNameInputField.text;
        netTier.m_TierColor.Value = tierColorInputField.text;
        int.TryParse(minimumBidInputField.text, out int minBid);
        netTier.m_MinBid.Value = minBid;
    }

    public void ExportToDraft()
    {

    }
}
