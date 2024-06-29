using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetTier : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> m_TierName = new NetworkVariable<FixedString32Bytes>("");
    public NetworkVariable<FixedString32Bytes> m_TierColor = new NetworkVariable<FixedString32Bytes>("#ffffff");
    public NetworkVariable<int> m_MinBid = new NetworkVariable<int>(0);

    private TieredPlayerList tieredPlayerList;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_TierName.OnValueChanged += OnTierNameChanged;
        m_TierColor.OnValueChanged += OnTierColorChanged;
        m_MinBid.OnValueChanged += OnMinBidChanged;

        tieredPlayerList = GameManager.SetupManager.AddTieredPlayerList(NetworkObjectId);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        m_TierName.OnValueChanged -= OnTierNameChanged;
        m_TierColor.OnValueChanged -= OnTierColorChanged;
        m_MinBid.OnValueChanged -= OnMinBidChanged;

        if (tieredPlayerList != null)
        {
            tieredPlayerList.DeleteAllPlayers();
            Destroy(tieredPlayerList.gameObject);
        }
    }

    private void OnTierNameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        if (oldValue != newValue)
            tieredPlayerList.SetTierName(newValue.ToString());
    }

    private void OnTierColorChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        if (oldValue != newValue)
        {
            ColorUtility.TryParseHtmlString(newValue.ToString(), out Color targetColor);
            tieredPlayerList.SetTierNameColor(targetColor);
        }
    }

    private void OnMinBidChanged(int oldValue, int newValue)
    {
        if (oldValue != newValue)
            tieredPlayerList.SetMinBid(newValue);
    }
}
