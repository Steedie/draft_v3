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
    private OwnedPlayer ownedPlayer;

    private bool isInitialized = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_DraftPlayerName.OnValueChanged += OnNameAssigned;
        m_HasBeenSold.OnValueChanged += OnHasBeenSoldChanged;

        if (IsClient)
        {
            StartCoroutine(InitializeClient());
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        m_DraftPlayerName.OnValueChanged -= OnNameAssigned;
        m_HasBeenSold.OnValueChanged -= OnHasBeenSoldChanged;

        if (unownedPlayer != null)
            Destroy(unownedPlayer.gameObject);
    }

    private IEnumerator InitializeClient()
    {
        yield return new WaitUntil(() => !m_DraftPlayerName.IsDirty() && !m_NetTierId.IsDirty());
        UpdateClientState();
        isInitialized = true;
    }

    private void UpdateClientState()
    {
        PlaceDraftPlayer(m_DraftPlayerName.Value);
    }

    private void OnNameAssigned(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        if (!isInitialized)
            return;

        PlaceDraftPlayer(newValue);
    }

    private void OnHasBeenSoldChanged(bool oldValue, bool newValue)
    {
        StartCoroutine(WaitForDirtyAfterSold());
    }

    IEnumerator WaitForDirtyAfterSold()
    {
        yield return new WaitForSeconds(.2f);
        PlaceDraftPlayer(m_DraftPlayerName.Value);
    }

    private void PlaceDraftPlayer(FixedString32Bytes newValue)
    {
        if (m_HasBeenSold.Value)
        {
            // CREATE PLAYER IN CAPTAIN'S PLAYER LIST

            if (unownedPlayer != null)
            {
                Destroy(unownedPlayer.gameObject);
                unownedPlayer = null;
            }

            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(m_NetPlayerOwnerId.Value, out NetworkObject netPlayerOwnerObject);
            ownedPlayer = netPlayerOwnerObject.GetComponent<NetPlayer>().AddDraftPlayer(m_DraftPlayerName.Value.ToString(), m_FinalPrice.Value);
        }
        else
        {
            // CREATE PLAYER IN MAIN DRAFT LIST

            if (ownedPlayer != null)
            {
                Destroy(ownedPlayer.gameObject);
                ownedPlayer = null;
            }

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