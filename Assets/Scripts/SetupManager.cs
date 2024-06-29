using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SetupManager : NetworkBehaviour
{
    [SerializeField] private GameObject draftSetup;

    // TIER SETUP
    [SerializeField] private SetupTier setupTierPrefab;
    [SerializeField] private Transform setupTierParent;

    // MAIN DRAFT
    [SerializeField] private NetTier netTierPrefab;
    [SerializeField] private TieredPlayerList tieredPlayerList;
    [SerializeField] private Transform tieredPlayerListParent;

    public void ToggleSetupMenu()
    {
        if (!IsHost)
            return;

        if (draftSetup.activeInHierarchy)
        {
            GameManager.GameFlow.ShowScreen(GameFlow.Screens.MainDraft);
        }
        else
        {
            GameManager.GameFlow.ShowScreen(GameFlow.Screens.DraftSetup);
        }
    }

    public void AddNewTierButton()
    {
        SetupTier setupTier = Instantiate(setupTierPrefab, setupTierParent);

        NetTier netTier = Instantiate(netTierPrefab);
        netTier.GetComponent<NetworkObject>().Spawn();
        ulong netTierObjectId = netTier.NetworkObjectId;

        setupTier.SetId(netTierObjectId);
    }

    public TieredPlayerList AddTieredPlayerList(ulong networkObjectId)
    {
        TieredPlayerList newTieredPlayerList = Instantiate(tieredPlayerList, tieredPlayerListParent);
        newTieredPlayerList.SetId(networkObjectId);
        return newTieredPlayerList;
    }

    public void SetAsLastSibling(Transform t)
    {
        t.SetAsLastSibling();
    }
}
