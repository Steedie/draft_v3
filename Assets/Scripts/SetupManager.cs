using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.IO;
using UnityEditor;

public class SetupManager : NetworkBehaviour
{
    [SerializeField] private GameObject draftSetup;
    [SerializeField] private GameObject hostButtons;

    // TIER SETUP
    [SerializeField] private SetupTier setupTierPrefab;
    [SerializeField] private Transform setupTierParent;
    [SerializeField] private Transform addTierButtonPanel;

    // MAIN DRAFT
    [SerializeField] private NetTier netTierPrefab;
    [SerializeField] private TieredPlayerList tieredPlayerList;
    [SerializeField] private Transform tieredPlayerListParent;

    [SerializeField] private NetDraftPlayer netDraftPlayerPrefab;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        hostButtons.SetActive(IsHost);
    }

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

    public void AddNewTierButton(TierData tierData)
    {
        SetupTier setupTier = Instantiate(setupTierPrefab, setupTierParent);
        addTierButtonPanel.SetAsLastSibling();
        setupTier.SetTierConfig(tierData);
        
        NetTier netTier = Instantiate(netTierPrefab);
        netTier.GetComponent<NetworkObject>().Spawn();
        ulong netTierObjectId = netTier.NetworkObjectId;

        setupTier.SetId(netTierObjectId);

        setupTier.SaveTierSettings();
        setupTier.AddPlayers();
    }

    public TieredPlayerList AddTieredPlayerList(ulong networkObjectId)
    {
        TieredPlayerList newTieredPlayerList = Instantiate(tieredPlayerList, tieredPlayerListParent);
        newTieredPlayerList.SetId(networkObjectId);
        return newTieredPlayerList;
    }

    public void CreateDraftPlayer(ulong tierId, string draftPlayerName)
    {
        NetDraftPlayer netDraftPlayer = Instantiate(netDraftPlayerPrefab);
        netDraftPlayer.GetComponent<NetworkObject>().Spawn();
        netDraftPlayer.SetupDraftPlayer(tierId, draftPlayerName);
    }

    public void SetAsLastSibling(Transform t)
    {
        t.SetAsLastSibling();
    }

    public void ExportConfigButton()
    {
        SetupTier[] setupTiers = FindObjectsOfType<SetupTier>();
        System.Array.Reverse(setupTiers);
        List<string> tierJsonStrings = new List<string>();

        foreach (SetupTier setupTier in setupTiers)
        {
            string tierJson = setupTier.GetTierConfigAsJson();
            tierJsonStrings.Add(tierJson);
        }

        string finalJson = "{ \"data\": [" + string.Join(",", tierJsonStrings) + "] }";
        string path = Path.Combine(Application.persistentDataPath, "tiers.json");

        File.WriteAllText(path, finalJson);
        Debug.Log($"Config saved to: {path}");
    }

    public void ImportConfigButton()
    {
        string path = Path.Combine(Application.persistentDataPath, "tiers.json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            var importData = JsonUtility.FromJson<TierDataList>(json);

            foreach (var tier in importData.data)
            {
                AddNewTierButton(tier);
            }
        }
        else
        {
            Debug.LogWarning("No config file found to import.");
        }
    }
}

[System.Serializable]
public class TierDataList
{
    public List<TierData> data;
}