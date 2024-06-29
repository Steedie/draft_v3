using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class NetDraftPlayer : NetworkBehaviour
{
    public NetworkVariable<ulong> m_NetTierId = new NetworkVariable<ulong>(0);
    public NetworkVariable<FixedString32Bytes> m_DraftPlayerName = new NetworkVariable<FixedString32Bytes>("");

    public NetworkVariable<bool> m_HasBeenSold = new NetworkVariable<bool>(false);
    public NetworkVariable<int> m_FinalPrice = new NetworkVariable<int>(0);
    public NetworkVariable<ulong> m_NetPlayerOwnerId = new NetworkVariable<ulong>(0);

    private UnownedPlayer unownedPlayer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_DraftPlayerName.OnValueChanged += OnNameAssigned;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        m_DraftPlayerName.OnValueChanged -= OnNameAssigned;

        if (unownedPlayer != null)
            Destroy(unownedPlayer.gameObject);
    }

    private void OnNameAssigned(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        if (m_HasBeenSold.Value)
        {
            // CREATE PLAYER IN CAPTAIN'S PLAYER LIST
            // ...
        }
        else
        {
            // CREATE PLAYER IN MAIN DRAFT LIST

            TieredPlayerList[] tieredPlayerLists = FindObjectsOfType<TieredPlayerList>(true);
            foreach (TieredPlayerList tieredPlayerList in tieredPlayerLists)
            {
                if (tieredPlayerList.GetNetTierId() == m_NetTierId.Value)
                {
                    unownedPlayer = tieredPlayerList.AddUnownedDraftPlayer(NetworkObjectId, newValue.ToString());
                    break;
                }
            }
        }
    }

    public void SetupDraftPlayer(ulong netTierId, string draftPlayerName)
    {
        m_NetTierId.Value = netTierId;
        m_DraftPlayerName.Value = draftPlayerName;
    }
}