using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CaptainManager : MonoBehaviour
{
    public NetworkVariable<int> m_StartingCaptainMoney = new NetworkVariable<int>(10000);

    [SerializeField] private CaptainCard captainCardPrefab;
    [SerializeField] private Transform captainCardParent;

    public CaptainCard CreateCaptainCard(ulong netPlayerId)
    {
        CaptainCard captainCard = Instantiate(captainCardPrefab, captainCardParent);
        captainCard.SetCaptainNetPlayerId(netPlayerId);
        return captainCard;
    }
}
