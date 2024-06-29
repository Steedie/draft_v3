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
    private bool isInitialized = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            m_TierName.OnValueChanged += OnTierNameChanged;
            m_TierColor.OnValueChanged += OnTierColorChanged;
            m_MinBid.OnValueChanged += OnMinBidChanged;
        }

        tieredPlayerList = GameManager.SetupManager.AddTieredPlayerList(NetworkObjectId);

        if (IsClient)
        {
            StartCoroutine(InitializeClient());
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            m_TierName.OnValueChanged -= OnTierNameChanged;
            m_TierColor.OnValueChanged -= OnTierColorChanged;
            m_MinBid.OnValueChanged -= OnMinBidChanged;
        }

        if (tieredPlayerList != null)
        {
            tieredPlayerList.DeleteAllPlayers();
            Destroy(tieredPlayerList.gameObject);
        }
    }

    private IEnumerator InitializeClient()
    {
        yield return new WaitUntil(() => m_TierName.IsDirty() == false && m_TierColor.IsDirty() == false && m_MinBid.IsDirty() == false);
        UpdateClientState();
        isInitialized = true;
    }

    private void UpdateClientState()
    {
        tieredPlayerList.SetTierName(m_TierName.Value.ToString());
        if (ColorUtility.TryParseHtmlString(m_TierColor.Value.ToString(), out Color targetColor))
        {
            tieredPlayerList.SetTierNameColor(targetColor);
        }
        tieredPlayerList.SetMinBid(m_MinBid.Value);
    }

    private void OnTierNameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        if (isInitialized && oldValue != newValue)
            tieredPlayerList.SetTierName(newValue.ToString());
    }

    private void OnTierColorChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        if (isInitialized && oldValue != newValue)
        {
            if (ColorUtility.TryParseHtmlString(newValue.ToString(), out Color targetColor))
            {
                tieredPlayerList.SetTierNameColor(targetColor);
            }
        }
    }

    private void OnMinBidChanged(int oldValue, int newValue)
    {
        if (isInitialized && oldValue != newValue)
            tieredPlayerList.SetMinBid(newValue);
    }
}
