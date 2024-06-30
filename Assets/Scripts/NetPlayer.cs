using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class NetPlayer : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> m_PlayerName = new NetworkVariable<FixedString32Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> m_Money = new NetworkVariable<int>(0);
    public NetworkVariable<bool> m_IsConnected = new NetworkVariable<bool>(true);

    CaptainCard captainCard;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsHost)
        {
            m_Money.Value = GameManager.CaptainManager.m_StartingCaptainMoney.Value;
            m_IsConnected.Value = true;
        }

        if (IsOwner)
        {
            string captainName = PlayerPrefs.GetString("CaptainName");
            NetPlayer[] netPlayers = FindObjectsOfType<NetPlayer>();
            foreach(NetPlayer netPlayer in netPlayers)
            {
                Debug.Log($"Looking at player {netPlayer.m_PlayerName.Value}");
                if (captainName == netPlayer.m_PlayerName.Value && netPlayer.IsOwnedByServer && netPlayer != this)
                {
                    Debug.Log("FOUND ANOTHER CAPTAIN WITH THE SAME NAME!");
                    TransformToExistingCaptainRpc(netPlayer.NetworkObjectId);
                    GameManager.Instance.AssignLocalPlayer(netPlayer);
                    return;
                }
            }

            if (!IsHost)
                m_PlayerName.Value = captainName;
            GameManager.Instance.AssignLocalPlayer(this);
        }

        if (!IsOwnedByServer)
        {
            // Is Captain and not Host
            if (captainCard == null)
            {
                captainCard = GameManager.CaptainManager.CreateCaptainCard(NetworkObjectId);
            }

            m_PlayerName.OnValueChanged += OnCaptainNameChanged;
            m_Money.OnValueChanged += OnCaptainMoneyChanged;

            StartCoroutine(InitializeClient());
        }
    }

    [Rpc(SendTo.Server)]
    private void TransformToExistingCaptainRpc(ulong targetNetPlayerObjectId)
    {
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetPlayerObjectId, out NetworkObject targetNetObject);
        targetNetObject.ChangeOwnership(GetComponent<NetworkObject>().OwnerClientId);
        DeleteThisRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void DeleteThisRpc()
    {
        if (captainCard != null)
        {
            if (!captainCard.HasNoDraftPlayers())
                Debug.LogWarning("Deleted captainCard has draft players. This shouldn't happen...");

            Destroy(captainCard.gameObject);
        }

        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsHost)
        {
            m_IsConnected.Value = false;
        }

        if (!IsOwnedByServer)
        {
            m_PlayerName.OnValueChanged -= OnCaptainNameChanged;
            m_Money.OnValueChanged -= OnCaptainMoneyChanged;
        }
    }

    private IEnumerator InitializeClient()
    {
        yield return new WaitUntil(() => m_PlayerName.IsDirty() == false && m_Money.IsDirty() == false && captainCard != null);
        captainCard.UpdateCaptainName(m_PlayerName.Value.ToString());
        captainCard.UpdateCaptainMoney(m_Money.Value);
    }

    private void OnCaptainNameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        captainCard.UpdateCaptainName(newValue.ToString());
    }

    private void OnCaptainMoneyChanged(int oldValue, int newValue)
    {
        captainCard.UpdateCaptainMoney(newValue);
    }

    public OwnedPlayer AddDraftPlayer(string draftPlayerName, int moneySpent)
    {
        if (captainCard == null)
        {
            captainCard = GameManager.CaptainManager.CreateCaptainCard(NetworkObjectId);

            StartCoroutine(InitializeClient());
        }

        return captainCard.AddOwnedPlayer(draftPlayerName, moneySpent);
    }
}
